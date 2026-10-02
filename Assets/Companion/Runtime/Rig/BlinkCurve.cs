using Unity.Mathematics;

namespace Companion
{
    public static class BlinkCurve
    {
        const float ClosedAt = 0.4f;          // lids close quickly, then open more slowly
        const float DoubleBlinkGap = 0.07f;

        /// <summary>Eye openness for a blink phase: 1 outside [0, 1), 0 at the moment the lids meet.</summary>
        public static float Openness(float phase)
        {
            if (phase < 0f || phase >= 1f)
                return 1f;
            return phase < ClosedAt
                ? 1f - math.smoothstep(0f, ClosedAt, phase)
                : math.smoothstep(ClosedAt, 1f, phase);
        }

        public static void Step(ref BlinkState blink, float deltaTime)
        {
            if (blink.Phase < 0f)
            {
                blink.Countdown -= deltaTime;
                if (blink.Countdown <= 0f)
                    blink.Phase = 0f;
            }
            else
            {
                blink.Phase += deltaTime / math.max(blink.Duration, 0.01f);
                if (blink.Phase >= 1f)
                {
                    blink.Phase = -1f;
                    if (!blink.InDoubleBlink && blink.Random.NextFloat() < blink.DoubleBlinkChance)
                    {
                        blink.InDoubleBlink = true;
                        blink.Countdown = DoubleBlinkGap;
                    }
                    else
                    {
                        blink.InDoubleBlink = false;
                        blink.Countdown = blink.Random.NextFloat(blink.MinInterval, math.max(blink.MinInterval, blink.MaxInterval));
                    }
                }
            }
            blink.Openness = Openness(blink.Phase);
        }
    }
}
