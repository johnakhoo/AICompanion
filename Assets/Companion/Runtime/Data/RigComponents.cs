using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Companion
{
    /// <summary>Current value of every <see cref="RigChannel"/> for one character.</summary>
    public struct RigParams : IComponentData
    {
        public FixedList128Bytes<float> Values;

        public float this[RigChannel channel]
        {
            get => Values[(int)channel];
            set => Values[(int)channel] = value;
        }

        public static RigParams AtRest()
        {
            var rest = new RigParams();
            for (int i = 0; i < RigChannels.Count; i++)
                rest.Values.Add(RigChannels.RestValue((RigChannel)i));
            return rest;
        }
    }

    public enum PartKind : byte
    {
        Fixed,          // never animated; only kept in the hierarchy
        Body,
        Face,
        Eye,
        BrowPivot,
        Brow,
        MouthSegment,
        MouthFill,
        Shadow,
    }

    /// <summary>Links an animated part entity to the character whose <see cref="RigParams"/> drive it.</summary>
    public struct RigPart : IComponentData
    {
        public Entity Character;
        public PartKind Kind;
        public sbyte Side;   // -1 screen-left, +1 screen-right, 0 centre
        public byte Index;   // mouth segment / fill column index
    }

    /// <summary>The part's authored local transform, including non-uniform scale.</summary>
    public struct RigRest : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
        public float3 Scale;
    }

    /// <summary>Proportions the rig solver needs, baked from the character's authoring.</summary>
    public struct RigDimensions : IComponentData
    {
        public float BallRadius;
        public float MouthHalfAngle;   // radians from centre to corner at MouthWidth = 1
        public float MouthSmileDepth;  // radians the centre dips at MouthSmile = 1
        public float MouthOpenUp;      // radians the upper lip lifts at MouthOpen = 1
        public float MouthOpenDown;    // radians the lower lip drops at MouthOpen = 1
        public float MouthStroke;      // lip line thickness in metres

        public static RigDimensions Default => new RigDimensions
        {
            BallRadius = 0.5f,
            MouthHalfAngle = 0.2f,
            MouthSmileDepth = 0.07f,
            MouthOpenUp = 0.03f,
            MouthOpenDown = 0.13f,
            MouthStroke = 0.024f,
        };
    }
}
