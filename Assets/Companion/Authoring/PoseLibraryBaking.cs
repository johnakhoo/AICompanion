using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Companion.Authoring
{
    public static class PoseLibraryBaking
    {
        public const float SampleRate = 30f;

        public static BlobAssetReference<PoseLibraryBlob> CreateBlob(CompanionPoseLibrary library, Allocator allocator = Allocator.Persistent)
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<PoseLibraryBlob>();
            var poses = builder.Allocate(ref root.Poses, library.poses.Count);

            for (int p = 0; p < library.poses.Count; p++)
            {
                var source = library.poses[p];
                float duration = Mathf.Max(0.1f, source.duration);
                int sampleCount = Mathf.Max(2, Mathf.CeilToInt(duration * SampleRate) + 1);

                ref var pose = ref poses[p];
                pose.Name.CopyFromTruncated(source.name ?? string.Empty);
                pose.Duration = duration;
                pose.Weight = Mathf.Max(0f, source.weight);
                pose.SampleCount = sampleCount;

                var samples = builder.Allocate(ref pose.Samples, RigChannels.Count * sampleCount);
                for (int c = 0; c < RigChannels.Count; c++)
                {
                    var curve = source.Find((RigChannel)c);
                    bool animated = curve != null && curve.length > 0;
                    float rest = RigChannels.RestValue((RigChannel)c);
                    for (int i = 0; i < sampleCount; i++)
                    {
                        float time = duration * i / (sampleCount - 1);
                        samples[c * sampleCount + i] = animated ? curve.Evaluate(time) : rest;
                    }
                }
            }

            return builder.CreateBlobAssetReference<PoseLibraryBlob>(allocator);
        }
    }
}
