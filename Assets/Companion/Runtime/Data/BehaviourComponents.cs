using Unity.Entities;
using Unity.Mathematics;

namespace Companion
{
    /// <summary>Plays one pose at a time and crossfades to a different random pose after a few loops.</summary>
    public struct IdleCycler : IComponentData
    {
        public float MinHold;
        public float MaxHold;
        public float Crossfade;

        public int CurrentPose;
        public int PreviousPose;
        public float CurrentTime;
        public float PreviousTime;
        public float Blend;          // 0 = all previous pose, 1 = all current pose
        public float HoldRemaining;  // seconds until the next pose is picked

        public Random Random;
    }

    /// <summary>
    /// Enable this to jump to a specific pose (for example from a future AI layer). The idle cycler
    /// consumes the request and disables it again.
    /// </summary>
    public struct PoseRequest : IComponentData, IEnableableComponent
    {
        public int PoseIndex;
    }

    public struct BlinkState : IComponentData
    {
        public float MinInterval;
        public float MaxInterval;
        public float Duration;
        public float DoubleBlinkChance;

        public float Countdown;      // seconds until the next blink starts
        public float Phase;          // < 0 while the eyes are open, otherwise 0..1 through the blink
        public bool InDoubleBlink;
        public float Openness;       // 1 open .. 0 shut

        public Random Random;
    }

    /// <summary>Keeps the character in frame on any aspect ratio.</summary>
    public struct CameraFraming : IComponentData
    {
        public float3 Target;
        public float MinVisibleWidth;
        public float BaseFieldOfView;
    }
}
