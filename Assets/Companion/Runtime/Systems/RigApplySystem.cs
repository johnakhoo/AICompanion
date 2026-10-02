using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Companion
{
    /// <summary>Turns each character's blended <see cref="RigParams"/> into its parts' local transforms.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PoseSampleSystem))]
    [UpdateAfter(typeof(BlinkSystem))]
    public partial struct RigApplySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RigPart>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new RigApplyJob
            {
                Params = SystemAPI.GetComponentLookup<RigParams>(true),
                Blinks = SystemAPI.GetComponentLookup<BlinkState>(true),
                Dimensions = SystemAPI.GetComponentLookup<RigDimensions>(true),
            }.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    partial struct RigApplyJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<RigParams> Params;
        [ReadOnly] public ComponentLookup<BlinkState> Blinks;
        [ReadOnly] public ComponentLookup<RigDimensions> Dimensions;

        void Execute(in RigPart part, in RigRest rest, ref LocalTransform local, ref PostTransformMatrix post)
        {
            if (!Params.TryGetComponent(part.Character, out var rig))
                return;

            float3 position = rest.Position;
            quaternion rotation = rest.Rotation;
            float3 scale = rest.Scale;

            switch (part.Kind)
            {
                case PartKind.Body:
                {
                    position.y += rig[RigChannel.BodyHeight];
                    rotation = math.mul(rest.Rotation, SphereMath.PartRotation(
                        rig[RigChannel.BodyPitch], rig[RigChannel.BodyYaw], rig[RigChannel.BodyRoll]));
                    float squash = math.max(0.2f, rig[RigChannel.BodySquash]);
                    float spread = math.rsqrt(squash);
                    scale *= new float3(spread, squash, spread);
                    break;
                }
                case PartKind.Face:
                    rotation = math.mul(rest.Rotation, SphereMath.PartRotation(
                        rig[RigChannel.FacePitch], rig[RigChannel.FaceYaw], rig[RigChannel.FaceRoll]));
                    break;
                case PartKind.Eye:
                {
                    float blink = Blinks.TryGetComponent(part.Character, out var blinkState) ? blinkState.Openness : 1f;
                    float size = rig[RigChannel.EyeScale];
                    float open = math.max(0.07f, math.saturate(rig[RigChannel.EyeOpen]) * blink);
                    scale *= new float3(size, size * open, size);
                    break;
                }
                case PartKind.BrowPivot:
                {
                    float raise = part.Side < 0 ? rig[RigChannel.BrowLeftRaise] : rig[RigChannel.BrowRightRaise];
                    rotation = math.mul(rest.Rotation, quaternion.RotateX(math.radians(raise)));
                    break;
                }
                case PartKind.Brow:
                {
                    // The brow's rest rotation lays the capsule sideways, so a positive Z turn lifts its +X end.
                    // The inner end is +X for the screen-left brow and -X for the screen-right one.
                    float tilt = part.Side < 0 ? rig[RigChannel.BrowLeftTilt] : -rig[RigChannel.BrowRightTilt];
                    rotation = math.mul(rest.Rotation, quaternion.RotateZ(math.radians(tilt)));
                    break;
                }
                case PartKind.MouthSegment:
                {
                    if (Dimensions.TryGetComponent(part.Character, out var dims))
                        MouthContour.Segment(dims, MouthShape.From(rig), part.Index, out position, out rotation, out scale);
                    break;
                }
                case PartKind.MouthFill:
                {
                    if (Dimensions.TryGetComponent(part.Character, out var dims))
                        MouthContour.Fill(dims, MouthShape.From(rig), part.Index, out position, out rotation, out scale);
                    break;
                }
                case PartKind.Shadow:
                {
                    // The shadow quad lies flat; shrink it while the ball is in the air, widen it when squashed.
                    float lift = math.saturate(rig[RigChannel.BodyHeight] * 2.5f);
                    float spread = math.rsqrt(math.max(0.2f, rig[RigChannel.BodySquash])) * math.lerp(1f, 0.6f, lift);
                    scale *= new float3(spread, spread, 1f);
                    break;
                }
            }

            local = LocalTransform.FromPositionRotation(position, rotation);
            post.Value = float4x4.Scale(scale);
        }
    }
}
