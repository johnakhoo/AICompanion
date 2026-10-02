using NUnit.Framework;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Companion.Tests
{
    public class RigMathTests
    {
        static readonly RigDimensions Dims = RigDimensions.Default;

        [TestCase(0f, 0f)]
        [TestCase(0.3f, 0.1f)]
        [TestCase(-0.25f, -0.35f)]
        public void OrientationCarriesForwardOntoDirection(float azimuth, float elevation)
        {
            float3 rotated = math.mul(SphereMath.Orientation(azimuth, elevation), new float3(0f, 0f, -1f));
            float3 expected = SphereMath.Direction(azimuth, elevation);
            Assert.That(math.distance(rotated, expected), Is.LessThan(1e-5f));
        }

        [Test]
        public void PositiveAzimuthIsScreenRight()
        {
            Assert.Greater(SphereMath.Direction(0.3f, 0f).x, 0f);
            Assert.Greater(SphereMath.Direction(0f, 0.3f).y, 0f);
        }

        [Test]
        public void ClosedMouthLipsMeet()
        {
            var shape = new MouthShape { Smile = 0.6f, Open = 0f, Width = 1f };
            for (float t = -1f; t <= 1f; t += 0.25f)
            {
                float3 upper = MouthContour.LipPoint(Dims, shape, false, t, 0.5f);
                float3 lower = MouthContour.LipPoint(Dims, shape, true, t, 0.5f);
                Assert.That(math.distance(upper, lower), Is.LessThan(1e-6f));
            }
        }

        [Test]
        public void OpenMouthCornersStillMeet()
        {
            var shape = new MouthShape { Smile = 0.2f, Open = 1f, Width = 0.7f };
            foreach (float corner in new[] { -1f, 1f })
            {
                float3 upper = MouthContour.LipPoint(Dims, shape, false, corner, 0.5f);
                float3 lower = MouthContour.LipPoint(Dims, shape, true, corner, 0.5f);
                Assert.That(math.distance(upper, lower), Is.LessThan(1e-6f));
            }
            float3 top = MouthContour.LipPoint(Dims, shape, false, 0f, 0.5f);
            float3 bottom = MouthContour.LipPoint(Dims, shape, true, 0f, 0.5f);
            Assert.Greater(top.y - bottom.y, 0.05f, "a wide-open mouth should have a visible gap");
        }

        [Test]
        public void SmilingMouthCurvesUpAtTheCorners()
        {
            var shape = new MouthShape { Smile = 1f, Open = 0f, Width = 1f };
            float centre = MouthContour.LipAngles(Dims, shape, false, 0f).y;
            float corner = MouthContour.LipAngles(Dims, shape, false, 1f).y;
            Assert.Greater(corner, centre);
        }

        [Test]
        public void LipSegmentsLieOnTheBall()
        {
            var shape = new MouthShape { Smile = 0.5f, Open = 0.6f, Width = 1.1f };
            for (int i = 0; i < MouthContour.SegmentCount; i++)
            {
                MouthContour.Segment(Dims, shape, i, out var position, out _, out var scale);
                Assert.AreEqual(MouthContour.LipRadius(Dims), math.length(position), 1e-5f);
                Assert.Greater(scale.y, 0f);
            }
        }

        [Test]
        public void MouthFillHiddenWhenClosed()
        {
            var shape = new MouthShape { Smile = 0.35f, Open = 0f, Width = 1f };
            for (int i = 0; i < MouthContour.FillColumns; i++)
            {
                MouthContour.Fill(Dims, shape, i, out _, out _, out var scale);
                Assert.LessOrEqual(scale.x, 1e-4f);
            }
        }

        [Test]
        public void BlinkCurveShape()
        {
            Assert.AreEqual(1f, BlinkCurve.Openness(-1f));
            Assert.AreEqual(1f, BlinkCurve.Openness(0f), 1e-5f);
            Assert.AreEqual(0f, BlinkCurve.Openness(0.4f), 1e-5f);
            Assert.AreEqual(1f, BlinkCurve.Openness(1f));
        }

        [Test]
        public void BlinkingStaysInRangeAndHappensRegularly()
        {
            var blink = new BlinkState
            {
                MinInterval = 2f, MaxInterval = 6f, Duration = 0.16f, DoubleBlinkChance = 0.2f,
                Countdown = 1f, Phase = -1f, Openness = 1f, Random = Random.CreateFromIndex(9),
            };

            int blinks = 0;
            bool wasClosing = false;
            for (int frame = 0; frame < 60 * 60; frame++)
            {
                BlinkCurve.Step(ref blink, 1f / 60f);
                Assert.That(blink.Openness, Is.InRange(0f, 1f));
                bool closing = blink.Phase >= 0f;
                if (closing && !wasClosing)
                    blinks++;
                wasClosing = closing;
            }
            Assert.That(blinks, Is.InRange(10, 40), "about one blink every 2-6 seconds over a minute");
        }
    }
}
