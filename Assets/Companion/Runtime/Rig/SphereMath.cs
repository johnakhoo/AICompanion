using Unity.Mathematics;

namespace Companion
{
    /// <summary>
    /// Helpers for placing things on the ball. The face looks toward -Z (at the camera), so an
    /// azimuth/elevation of zero points straight at the viewer.
    /// </summary>
    public static class SphereMath
    {
        /// <summary>Unit direction for an azimuth (positive = screen-right) and elevation (positive = up), in radians.</summary>
        public static float3 Direction(float azimuth, float elevation)
        {
            math.sincos(azimuth, out float sinAzimuth, out float cosAzimuth);
            math.sincos(elevation, out float sinElevation, out float cosElevation);
            return new float3(cosElevation * sinAzimuth, sinElevation, -cosElevation * cosAzimuth);
        }

        /// <summary>Rotation that carries -Z onto <see cref="Direction"/>; used for the feature pivots.</summary>
        public static quaternion Orientation(float azimuth, float elevation)
        {
            return quaternion.EulerZXY(elevation, -azimuth, 0f);
        }

        /// <summary>Rotation for the pitch/yaw/roll channel convention in <see cref="RigChannel"/> (degrees).</summary>
        public static quaternion PartRotation(float pitchDegrees, float yawDegrees, float rollDegrees)
        {
            return quaternion.EulerZXY(math.radians(pitchDegrees), math.radians(-yawDegrees), math.radians(-rollDegrees));
        }
    }
}
