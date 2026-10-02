using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Companion
{
    /// <summary>
    /// Widens the camera's field of view on narrow (portrait) screens so the character always fits.
    /// Not Burst-compiled: URP has no ECS camera, so this talks to the scene's Camera component.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct CameraFramingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraFraming>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var camera = Camera.main;
            if (camera == null)
                return;

            var framing = SystemAPI.GetSingleton<CameraFraming>();
            float distance = math.max(0.01f, math.distance((float3)camera.transform.position, framing.Target));
            float halfWidthAngle = math.atan(framing.MinVisibleWidth * 0.5f / distance);
            float verticalForWidth = 2f * math.degrees(math.atan(math.tan(halfWidthAngle) / math.max(camera.aspect, 0.01f)));
            float fieldOfView = math.max(framing.BaseFieldOfView, verticalForWidth);
            if (math.abs(camera.fieldOfView - fieldOfView) > 0.01f)
                camera.fieldOfView = fieldOfView;
        }
    }
}
