using Unity.Collections;
using Unity.Entities;

namespace Companion
{
    /// <summary>One looping idle pose, sampled at a fixed rate for every <see cref="RigChannel"/>.</summary>
    public struct PoseBlob
    {
        public FixedString32Bytes Name;
        public float Duration;
        public float Weight;             // relative chance of being picked by the idle cycler
        public int SampleCount;          // samples per channel; the first and last coincide so the pose loops
        public BlobArray<float> Samples; // channel-major: Samples[channel * SampleCount + i]
    }

    public struct PoseLibraryBlob
    {
        public BlobArray<PoseBlob> Poses;

        public int IndexOf(in FixedString32Bytes name)
        {
            for (int i = 0; i < Poses.Length; i++)
                if (Poses[i].Name == name)
                    return i;
            return -1;
        }
    }

    public struct PoseLibrary : IComponentData
    {
        public BlobAssetReference<PoseLibraryBlob> Value;
    }
}
