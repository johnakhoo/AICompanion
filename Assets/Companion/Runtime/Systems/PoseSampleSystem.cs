using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Companion
{
    /// <summary>Samples the current (and fading-out previous) pose into each character's <see cref="RigParams"/>.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(IdleCycleSystem))]
    public partial struct PoseSampleSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PoseLibrary>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (rig, cycler, library) in
                     SystemAPI.Query<RefRW<RigParams>, RefRO<IdleCycler>, RefRO<PoseLibrary>>())
            {
                ref var poses = ref library.ValueRO.Value.Value.Poses;
                var playback = cycler.ValueRO;
                ref var current = ref poses[playback.CurrentPose];
                ref var previous = ref poses[playback.PreviousPose];
                float blend = PoseSampling.Ease(playback.Blend);

                ref var values = ref rig.ValueRW.Values;
                for (int channel = 0; channel < RigChannels.Count; channel++)
                {
                    float target = PoseSampling.Sample(ref current, channel, playback.CurrentTime);
                    values[channel] = blend >= 1f
                        ? target
                        : math.lerp(PoseSampling.Sample(ref previous, channel, playback.PreviousTime), target, blend);
                }
            }
        }
    }
}
