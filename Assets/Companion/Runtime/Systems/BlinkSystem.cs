using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Companion
{
    /// <summary>Random blinking, layered on top of whatever eye openness the current pose asks for.</summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct BlinkSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlinkState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var blink in SystemAPI.Query<RefRW<BlinkState>>())
                BlinkCurve.Step(ref blink.ValueRW, deltaTime);
        }
    }
}
