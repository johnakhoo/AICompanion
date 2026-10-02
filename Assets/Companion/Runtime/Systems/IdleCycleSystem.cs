using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Companion
{
    /// <summary>Advances each character's pose playback and picks the next idle pose when it's time.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct IdleCycleSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleCycler>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Explicit requests (e.g. from a future AI layer) jump straight to a pose, then switch off.
            foreach (var (request, cycler, library, entity) in
                     SystemAPI.Query<RefRO<PoseRequest>, RefRW<IdleCycler>, RefRO<PoseLibrary>>().WithEntityAccess())
            {
                ref var poses = ref library.ValueRO.Value.Value.Poses;
                int index = request.ValueRO.PoseIndex;
                if (index >= 0 && index < poses.Length)
                    IdleCycling.Begin(ref cycler.ValueRW, ref poses, index);
                SystemAPI.SetComponentEnabled<PoseRequest>(entity, false);
            }

            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (cycler, library) in SystemAPI.Query<RefRW<IdleCycler>, RefRO<PoseLibrary>>())
                IdleCycling.Step(ref cycler.ValueRW, ref library.ValueRO.Value.Value.Poses, deltaTime);
        }
    }
}
