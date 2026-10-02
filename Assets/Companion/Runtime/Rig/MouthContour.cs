using Unity.Mathematics;

namespace Companion
{
    public struct MouthShape
    {
        public float Smile;
        public float Open;
        public float Width;

        public static MouthShape From(in RigParams rig)
        {
            return new MouthShape
            {
                Smile = rig[RigChannel.MouthSmile],
                Open = math.saturate(rig[RigChannel.MouthOpen]),
                Width = math.max(0.1f, rig[RigChannel.MouthWidth]),
            };
        }
    }

    /// <summary>
    /// The mouth is a chain of capsules lying on the ball: an upper and a lower lip that run corner to
    /// corner, plus dark fill columns between them. All positions are in mouth-pivot space, whose origin
    /// is the ball centre and whose -Z points at the mouth's centre.
    /// </summary>
    public static class MouthContour
    {
        public const int SegmentsPerLip = 8;
        public const int SegmentCount = SegmentsPerLip * 2;
        public const int FillColumns = 7;

        const float LipLift = 0.003f;    // lip line centre sits just above the ball surface
        const float FillLift = 0.0015f;  // fill sits between the ball and the lip lines
        const float FillDepth = 0.004f;
        const float HiddenScale = 1e-4f;

        public static float LipRadius(in RigDimensions dims) => dims.BallRadius + LipLift;

        /// <summary>Azimuth/elevation (radians) of a lip at <paramref name="t"/> in [-1, 1] from corner to corner.</summary>
        public static float2 LipAngles(in RigDimensions dims, in MouthShape shape, bool lower, float t)
        {
            float bend = 1f - t * t;
            float round = math.sqrt(math.max(0f, bend));
            float centre = -shape.Smile * dims.MouthSmileDepth * bend;
            float elevation = lower
                ? centre - shape.Open * dims.MouthOpenDown * round
                : centre + shape.Open * dims.MouthOpenUp * round;
            return new float2(t * dims.MouthHalfAngle * shape.Width, elevation);
        }

        public static float3 LipPoint(in RigDimensions dims, in MouthShape shape, bool lower, float t, float radius)
        {
            float2 angles = LipAngles(dims, shape, lower, t);
            return SphereMath.Direction(angles.x, angles.y) * radius;
        }

        /// <summary>Transform of lip segment <paramref name="index"/>: upper lip first, then lower lip.</summary>
        public static void Segment(in RigDimensions dims, in MouthShape shape, int index,
            out float3 position, out quaternion rotation, out float3 scale)
        {
            bool lower = index >= SegmentsPerLip;
            int i = lower ? index - SegmentsPerLip : index;
            float t0 = -1f + 2f * i / SegmentsPerLip;
            float t1 = -1f + 2f * (i + 1) / SegmentsPerLip;
            float radius = LipRadius(dims);
            float3 a = LipPoint(dims, shape, lower, t0, radius);
            float3 b = LipPoint(dims, shape, lower, t1, radius);

            float3 along = b - a;
            float length = math.length(along);
            float3 axis = length > 1e-6f ? along / length : new float3(1f, 0f, 0f);
            float3 normal = math.normalize(a + b);
            position = normal * radius;
            rotation = quaternion.LookRotationSafe(normal, axis);
            // Unity's capsule mesh is 2 long and 1 wide. Overhang half the stroke at each end so the
            // rounded caps of neighbouring segments overlap into one smooth line.
            float stroke = dims.MouthStroke;
            scale = new float3(stroke, (length + stroke) * 0.5f, stroke * 0.6f);
        }

        /// <summary>Transform of dark fill column <paramref name="index"/>, spanning the gap between the lips.</summary>
        public static void Fill(in RigDimensions dims, in MouthShape shape, int index,
            out float3 position, out quaternion rotation, out float3 scale)
        {
            float t = (-1f + 2f * (index + 0.5f) / FillColumns) * 0.9f;
            float radius = dims.BallRadius + FillLift;
            float3 top = LipPoint(dims, shape, false, t, radius);
            float3 bottom = LipPoint(dims, shape, true, t, radius);

            float3 along = top - bottom;
            float length = math.length(along);
            float3 axis = length > 1e-6f ? along / length : new float3(0f, 1f, 0f);
            float3 normal = math.normalize(top + bottom);
            position = normal * radius;
            rotation = quaternion.LookRotationSafe(normal, axis);

            // Columns overlap sideways; their ends stop at the lip centre lines so the lips hide them.
            float columnWidth = 2f * dims.MouthHalfAngle * shape.Width * dims.BallRadius / FillColumns * 1.5f;
            float visible = math.saturate(shape.Open * 12f);
            scale = new float3(
                math.max(HiddenScale, columnWidth * visible),
                math.max(HiddenScale, length * 0.5f),
                FillDepth);
        }
    }
}
