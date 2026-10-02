using System.Collections.Generic;
using Companion.Authoring;
using UnityEditor;
using UnityEngine;
using static Companion.RigChannel;

namespace Companion.EditorTools
{
    /// <summary>
    /// Starting keyframes for the idle poses. Every track begins and ends at the channel's rest value so
    /// poses loop cleanly and crossfade without pops. Tweak the generated asset in the Inspector afterwards.
    /// </summary>
    public static class DefaultPoses
    {
        const float Smile = RigChannels.RestSmile;

        public static void Populate(CompanionPoseLibrary library)
        {
            library.poses = new List<PoseClip> { Breathe(), LookAround(), Sway(), Bounce(), Curious(), Yawn() };
        }

        static PoseClip Breathe() => Pose("Breathe", 3.2f, 2f,
            Track(BodySquash, 0f, 1f, 1.6f, 1.035f, 3.2f, 1f),
            Track(BodyHeight, 0f, 0f, 1.6f, 0.006f, 3.2f, 0f),
            Track(FacePitch, 0f, 0f, 1.6f, 1.5f, 3.2f, 0f),
            Track(BrowLeftRaise, 0f, 0f, 1.6f, 1.5f, 3.2f, 0f),
            Track(BrowRightRaise, 0f, 0f, 1.6f, 1.5f, 3.2f, 0f));

        static PoseClip LookAround() => Pose("LookAround", 6f, 1f,
            Track(FaceYaw, 0f, 0f, 0.5f, 0f, 1.1f, -26f, 2.2f, -26f, 2.9f, 24f, 4.2f, 24f, 5f, 0f, 6f, 0f),
            Track(FacePitch, 0f, 0f, 1.1f, 3f, 2.2f, 3f, 2.9f, 5f, 4.2f, 5f, 5f, 0f, 6f, 0f),
            Track(BodyYaw, 0f, 0f, 1.1f, -8f, 2.2f, -8f, 2.9f, 7f, 4.2f, 7f, 5f, 0f, 6f, 0f),
            Track(BodyRoll, 0f, 0f, 1.1f, -2f, 2.2f, -2f, 2.9f, 2f, 4.2f, 2f, 5f, 0f, 6f, 0f),
            Track(BrowLeftRaise, 0f, 0f, 1.1f, 3f, 2.2f, 3f, 2.9f, 5f, 4.2f, 5f, 5f, 0f, 6f, 0f),
            Track(BrowRightRaise, 0f, 0f, 1.1f, 3f, 2.2f, 3f, 2.9f, 5f, 4.2f, 5f, 5f, 0f, 6f, 0f),
            Track(MouthSmile, 0f, Smile, 1.1f, 0.15f, 4.2f, 0.15f, 5f, Smile, 6f, Smile),
            Track(BodySquash, 0f, 1f, 1.5f, 1.025f, 3f, 1f, 4.5f, 1.025f, 6f, 1f));

        static PoseClip Sway() => Pose("Sway", 4f, 1f,
            Track(BodyRoll, 0f, 0f, 1f, 7f, 2f, 0f, 3f, -7f, 4f, 0f),
            Track(FaceRoll, 0f, 0f, 1f, 5f, 2f, 0f, 3f, -5f, 4f, 0f),
            Track(FaceYaw, 0f, 0f, 1f, 6f, 2f, 0f, 3f, -6f, 4f, 0f),
            Track(BodyHeight, 0f, 0f, 1f, 0.015f, 2f, 0f, 3f, 0.015f, 4f, 0f),
            Track(MouthSmile, 0f, Smile, 0.5f, 0.85f, 3.5f, 0.85f, 4f, Smile),
            Track(MouthWidth, 0f, 1f, 0.5f, 1.1f, 3.5f, 1.1f, 4f, 1f),
            Track(BrowLeftRaise, 0f, 0f, 0.5f, 4f, 3.5f, 4f, 4f, 0f),
            Track(BrowRightRaise, 0f, 0f, 0.5f, 4f, 3.5f, 4f, 4f, 0f),
            Track(EyeOpen, 0f, 1f, 0.5f, 0.85f, 3.5f, 0.85f, 4f, 1f));

        static PoseClip Bounce() => Pose("Bounce", 2.4f, 1f,
            Track(BodyHeight, 0f, 0f, 0.25f, 0f, 0.52f, 0.2f, 0.78f, 0f, 1.35f, 0f, 1.62f, 0.2f, 1.88f, 0f, 2.4f, 0f),
            Track(BodySquash, 0f, 1f, 0.12f, 0.88f, 0.25f, 1.1f, 0.52f, 1f, 0.74f, 1.06f, 0.82f, 0.87f, 1f, 1.03f,
                1.22f, 0.88f, 1.35f, 1.1f, 1.62f, 1f, 1.84f, 1.06f, 1.92f, 0.87f, 2.1f, 1.03f, 2.4f, 1f),
            Track(FacePitch, 0f, 0f, 0.52f, 6f, 0.82f, -2f, 1f, 0f, 1.62f, 6f, 1.92f, -2f, 2.1f, 0f, 2.4f, 0f),
            Track(MouthSmile, 0f, Smile, 0.2f, 0.95f, 2.1f, 0.95f, 2.4f, Smile),
            Track(MouthOpen, 0f, 0f, 0.2f, 0.3f, 2.1f, 0.3f, 2.4f, 0f),
            Track(BrowLeftRaise, 0f, 0f, 0.2f, 6f, 2.1f, 6f, 2.4f, 0f),
            Track(BrowRightRaise, 0f, 0f, 0.2f, 6f, 2.1f, 6f, 2.4f, 0f),
            Track(EyeScale, 0f, 1f, 0.52f, 1.08f, 0.78f, 1f, 1.62f, 1.08f, 1.88f, 1f, 2.4f, 1f));

        static PoseClip Curious() => Pose("Curious", 4.5f, 1f,
            Track(BodyRoll, 0f, 0f, 0.6f, -9f, 3.8f, -9f, 4.5f, 0f),
            Track(FaceRoll, 0f, 0f, 0.6f, -6f, 3.8f, -6f, 4.5f, 0f),
            Track(FacePitch, 0f, 0f, 0.6f, 5f, 3.8f, 5f, 4.5f, 0f),
            Track(FaceYaw, 0f, 0f, 0.6f, 8f, 2.2f, 11f, 3.8f, 8f, 4.5f, 0f),
            Track(BrowLeftRaise, 0f, 0f, 0.6f, 9f, 3.8f, 9f, 4.5f, 0f),
            Track(BrowRightRaise, 0f, 0f, 0.6f, -2f, 3.8f, -2f, 4.5f, 0f),
            Track(BrowRightTilt, 0f, 0f, 0.6f, -6f, 3.8f, -6f, 4.5f, 0f),
            Track(MouthSmile, 0f, Smile, 0.6f, 0f, 3.8f, 0f, 4.5f, Smile),
            Track(MouthOpen, 0f, 0f, 0.6f, 0.35f, 3.8f, 0.35f, 4.5f, 0f),
            Track(MouthWidth, 0f, 1f, 0.6f, 0.45f, 3.8f, 0.45f, 4.5f, 1f),
            Track(EyeScale, 0f, 1f, 0.6f, 1.12f, 3.8f, 1.12f, 4.5f, 1f));

        static PoseClip Yawn() => Pose("Yawn", 5f, 0.6f,
            Track(BodySquash, 0f, 1f, 1f, 1.08f, 2.2f, 1.12f, 3f, 0.93f, 3.5f, 1f, 5f, 1f),
            Track(BodyPitch, 0f, 0f, 1f, 4f, 2.2f, 6f, 3f, -2f, 3.5f, 0f, 5f, 0f),
            Track(FacePitch, 0f, 0f, 1f, 6f, 2.2f, 9f, 3f, 0f, 5f, 0f),
            Track(MouthOpen, 0f, 0f, 0.6f, 0.15f, 1.4f, 1f, 2.6f, 1f, 3.1f, 0.05f, 3.5f, 0f, 5f, 0f),
            Track(MouthWidth, 0f, 1f, 1.4f, 0.7f, 2.6f, 0.7f, 3.2f, 1f, 5f, 1f),
            Track(MouthSmile, 0f, Smile, 0.6f, 0f, 2.6f, 0f, 3.6f, Smile, 5f, Smile),
            Track(EyeOpen, 0f, 1f, 0.6f, 0.75f, 1.4f, 0.22f, 2.6f, 0.18f, 3.1f, 1f, 5f, 1f),
            Track(BrowLeftRaise, 0f, 0f, 1.4f, 5f, 2.6f, 6f, 3.1f, 0f, 5f, 0f),
            Track(BrowRightRaise, 0f, 0f, 1.4f, 5f, 2.6f, 6f, 3.1f, 0f, 5f, 0f),
            Track(BrowLeftTilt, 0f, 0f, 1.4f, 6f, 2.6f, 6f, 3.1f, 0f, 5f, 0f),
            Track(BrowRightTilt, 0f, 0f, 1.4f, 6f, 2.6f, 6f, 3.1f, 0f, 5f, 0f),
            Track(FaceYaw, 0f, 0f, 3.2f, 0f, 3.4f, -7f, 3.6f, 7f, 3.8f, -4f, 4f, 0f, 5f, 0f));

        static PoseClip Pose(string name, float duration, float weight, params ChannelCurve[] channels)
        {
            return new PoseClip { name = name, duration = duration, weight = weight, channels = new List<ChannelCurve>(channels) };
        }

        /// <summary>A curve through (time, value) pairs that eases between keys without overshooting.</summary>
        static ChannelCurve Track(RigChannel channel, params float[] timeValuePairs)
        {
            var keys = new Keyframe[timeValuePairs.Length / 2];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = new Keyframe(timeValuePairs[2 * i], timeValuePairs[2 * i + 1]);

            var curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }

            // Flat ends so each loop joins smoothly with the next.
            FlattenKey(curve, 0);
            FlattenKey(curve, curve.length - 1);
            return new ChannelCurve { channel = channel, curve = curve };
        }

        static void FlattenKey(AnimationCurve curve, int index)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Free);
            AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.Free);
            var key = curve[index];
            key.inTangent = 0f;
            key.outTangent = 0f;
            curve.MoveKey(index, key);
        }
    }
}
