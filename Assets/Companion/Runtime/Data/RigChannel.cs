namespace Companion
{
    /// <summary>
    /// Animatable parameters of the companion rig. Each pose stores one curve per channel and
    /// <see cref="RigApplySystem"/> turns the blended values into part transforms.
    /// Angles are degrees; "left"/"right" mean screen-left/screen-right as the viewer sees them.
    /// </summary>
    public enum RigChannel
    {
        BodyHeight,     // metres the ball rises above its rest position
        BodySquash,     // 1 = rest, >1 stretches tall, <1 squashes flat (volume preserving)
        BodyRoll,       // positive leans the top toward screen-right
        BodyPitch,      // positive leans back so the face tips up
        BodyYaw,        // positive turns toward screen-right
        FaceYaw,        // the face glides across the ball; positive looks screen-right
        FacePitch,      // positive looks up
        FaceRoll,       // positive tilts the face's top toward screen-right
        BrowLeftRaise,  // degrees along the ball surface; positive = up
        BrowRightRaise,
        BrowLeftTilt,   // positive lifts the inner end (worried), negative lowers it (cross)
        BrowRightTilt,
        EyeOpen,        // 1 = open, 0 = shut; multiplied with blinking
        EyeScale,       // 1 = rest size
        MouthSmile,     // -1 frown .. 1 smile
        MouthOpen,      // 0 closed line .. 1 wide open
        MouthWidth,     // 1 = rest width
    }

    public static class RigChannels
    {
        public const int Count = 17;
        public const float RestSmile = 0.35f;

        /// <summary>Value a channel holds when no pose animates it.</summary>
        public static float RestValue(RigChannel channel)
        {
            switch (channel)
            {
                case RigChannel.BodySquash:
                case RigChannel.EyeOpen:
                case RigChannel.EyeScale:
                case RigChannel.MouthWidth:
                    return 1f;
                case RigChannel.MouthSmile:
                    return RestSmile;
                default:
                    return 0f;
            }
        }
    }
}
