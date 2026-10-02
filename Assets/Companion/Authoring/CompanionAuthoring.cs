using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Companion.Authoring
{
    /// <summary>Root of the companion character in its SubScene. Bakes the poses and behaviour settings.</summary>
    [DisallowMultipleComponent]
    public class CompanionAuthoring : MonoBehaviour
    {
        public CompanionPoseLibrary poseLibrary;
        public uint randomSeed = 7;

        [Header("Idle cycling")]
        public float minHold = 4f;
        public float maxHold = 9f;
        public float crossfade = 0.45f;

        [Header("Blinking")]
        public float minBlinkInterval = 2f;
        public float maxBlinkInterval = 6f;
        public float blinkDuration = 0.16f;
        [Range(0f, 1f)] public float doubleBlinkChance = 0.2f;

        [Header("Proportions")]
        public float ballRadius = 0.5f;
        [Tooltip("Radians from the mouth centre to a corner.")]
        public float mouthHalfAngle = 0.2f;
        public float mouthSmileDepth = 0.07f;
        public float mouthOpenUp = 0.03f;
        public float mouthOpenDown = 0.13f;
        public float mouthStroke = 0.024f;

        [Header("Camera framing")]
        public Vector3 framingTarget = new Vector3(0f, 0.5f, 0f);
        [Tooltip("World-space width that always stays visible, however narrow the screen.")]
        public float minVisibleWidth = 1.8f;
        public float baseFieldOfView = 30f;

        public RigDimensions Dimensions => new RigDimensions
        {
            BallRadius = ballRadius,
            MouthHalfAngle = mouthHalfAngle,
            MouthSmileDepth = mouthSmileDepth,
            MouthOpenUp = mouthOpenUp,
            MouthOpenDown = mouthOpenDown,
            MouthStroke = mouthStroke,
        };

        class Baker : Baker<CompanionAuthoring>
        {
            public override void Bake(CompanionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var library = DependsOn(authoring.poseLibrary);
                if (library == null || library.poses.Count == 0)
                {
                    Debug.LogError($"{authoring.name}: assign a CompanionPoseLibrary with at least one pose.", authoring);
                    return;
                }

                var blob = PoseLibraryBaking.CreateBlob(library);
                AddBlobAsset(ref blob, out _);
                AddComponent(entity, new PoseLibrary { Value = blob });

                AddComponent(entity, new IdleCycler
                {
                    MinHold = authoring.minHold,
                    MaxHold = authoring.maxHold,
                    Crossfade = authoring.crossfade,
                    Blend = 1f,
                    // Start on the first pose (Breathe) for a couple of loops before cycling.
                    HoldRemaining = library.poses[0].duration * 2f,
                    Random = Random.CreateFromIndex(authoring.randomSeed),
                });

                AddComponent(entity, new PoseRequest());
                SetComponentEnabled<PoseRequest>(entity, false);

                var blinkRandom = Random.CreateFromIndex(authoring.randomSeed + 1);
                AddComponent(entity, new BlinkState
                {
                    MinInterval = authoring.minBlinkInterval,
                    MaxInterval = authoring.maxBlinkInterval,
                    Duration = authoring.blinkDuration,
                    DoubleBlinkChance = authoring.doubleBlinkChance,
                    Countdown = blinkRandom.NextFloat(authoring.minBlinkInterval, authoring.maxBlinkInterval),
                    Phase = -1f,
                    Openness = 1f,
                    Random = blinkRandom,
                });

                AddComponent(entity, RigParams.AtRest());
                AddComponent(entity, authoring.Dimensions);
                AddComponent(entity, new CameraFraming
                {
                    Target = authoring.framingTarget,
                    MinVisibleWidth = authoring.minVisibleWidth,
                    BaseFieldOfView = authoring.baseFieldOfView,
                });
            }
        }
    }
}
