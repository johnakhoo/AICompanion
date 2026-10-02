using System;
using System.Collections.Generic;
using UnityEngine;

namespace Companion.Authoring
{
    /// <summary>
    /// Editable idle poses: one curve per <see cref="RigChannel"/> per pose. Channels without a curve stay
    /// at their rest value. Baked into a <see cref="PoseLibraryBlob"/>; nothing here ships in the player.
    /// </summary>
    [CreateAssetMenu(menuName = "Companion/Pose Library", fileName = "CompanionPoseLibrary")]
    public class CompanionPoseLibrary : ScriptableObject
    {
        public List<PoseClip> poses = new List<PoseClip>();

        public int IndexOf(string poseName) => poses.FindIndex(p => p.name == poseName);
    }

    [Serializable]
    public class PoseClip
    {
        public string name;
        [Min(0.1f)] public float duration = 4f;
        [Tooltip("Relative chance of the idle cycler picking this pose.")]
        [Min(0f)] public float weight = 1f;
        public List<ChannelCurve> channels = new List<ChannelCurve>();

        public AnimationCurve Find(RigChannel channel)
        {
            foreach (var track in channels)
                if (track.channel == channel)
                    return track.curve;
            return null;
        }
    }

    [Serializable]
    public class ChannelCurve
    {
        public RigChannel channel;
        public AnimationCurve curve = new AnimationCurve();
    }
}
