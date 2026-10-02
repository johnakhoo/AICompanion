using Unity.Entities;
using Unity.Mathematics;

namespace Companion
{
    public static class PoseSampling
    {
        public static float Wrap(float time, float duration)
        {
            return duration > 0f ? time - math.floor(time / duration) * duration : 0f;
        }

        /// <summary>Value of <paramref name="channel"/> at <paramref name="time"/> seconds into the looping pose.</summary>
        public static float Sample(ref PoseBlob pose, int channel, float time)
        {
            int count = pose.SampleCount;
            int start = channel * count;
            if (count < 2)
                return pose.Samples[start];

            float position = Wrap(time, pose.Duration) / pose.Duration * (count - 1);
            int i = math.min((int)position, count - 2);
            return math.lerp(pose.Samples[start + i], pose.Samples[start + i + 1], position - i);
        }

        /// <summary>Weighted random pick of any pose other than <paramref name="current"/>.</summary>
        public static int PickNext(ref BlobArray<PoseBlob> poses, int current, ref Random random)
        {
            int count = poses.Length;
            if (count <= 1)
                return 0;

            float total = 0f;
            for (int i = 0; i < count; i++)
                if (i != current)
                    total += math.max(0f, poses[i].Weight);
            if (total <= 0f)
                return (current + 1) % count;

            float pick = random.NextFloat(total);
            int last = current;
            for (int i = 0; i < count; i++)
            {
                if (i == current)
                    continue;
                last = i;
                pick -= math.max(0f, poses[i].Weight);
                if (pick < 0f)
                    return i;
            }
            return last;
        }

        /// <summary>
        /// How long to hold a pose: a whole number of loops close to a random target, so switches
        /// happen when the pose is back at rest.
        /// </summary>
        public static float HoldFor(float duration, float minHold, float maxHold, ref Random random)
        {
            float target = random.NextFloat(minHold, math.max(minHold, maxHold));
            float loops = math.max(1f, math.round(target / math.max(duration, 0.01f)));
            return loops * duration;
        }

        public static float Ease(float blend) => math.smoothstep(0f, 1f, blend);
    }

    public static class IdleCycling
    {
        public static void Begin(ref IdleCycler cycler, ref BlobArray<PoseBlob> poses, int next)
        {
            cycler.PreviousPose = cycler.CurrentPose;
            cycler.PreviousTime = cycler.CurrentTime;
            cycler.CurrentPose = next;
            cycler.CurrentTime = 0f;
            cycler.Blend = 0f;
            cycler.HoldRemaining = PoseSampling.HoldFor(poses[next].Duration, cycler.MinHold, cycler.MaxHold, ref cycler.Random);
        }

        public static void Step(ref IdleCycler cycler, ref BlobArray<PoseBlob> poses, float deltaTime)
        {
            cycler.CurrentTime += deltaTime;
            cycler.PreviousTime += deltaTime;
            cycler.Blend = math.min(1f, cycler.Blend + deltaTime / math.max(cycler.Crossfade, 0.01f));
            cycler.HoldRemaining -= deltaTime;
            if (cycler.HoldRemaining <= 0f)
                Begin(ref cycler, ref poses, PoseSampling.PickNext(ref poses, cycler.CurrentPose, ref cycler.Random));
        }
    }
}
