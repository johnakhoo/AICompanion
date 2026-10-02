using System;
using Companion.Authoring;
using Companion.EditorTools;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Companion.Tests
{
    public class PoseLibraryTests
    {
        CompanionPoseLibrary m_Library;
        BlobAssetReference<PoseLibraryBlob> m_Blob;

        [SetUp]
        public void SetUp()
        {
            m_Library = ScriptableObject.CreateInstance<CompanionPoseLibrary>();
            DefaultPoses.Populate(m_Library);
            m_Blob = PoseLibraryBaking.CreateBlob(m_Library, Allocator.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            m_Blob.Dispose();
            UnityEngine.Object.DestroyImmediate(m_Library);
        }

        [Test]
        public void ChannelCountMatchesEnum()
        {
            Assert.AreEqual(Enum.GetValues(typeof(RigChannel)).Length, RigChannels.Count);
            Assert.AreEqual(RigChannels.Count, RigParams.AtRest().Values.Length);
        }

        [Test]
        public void BlobMatchesLibrary()
        {
            ref var poses = ref m_Blob.Value.Poses;
            Assert.AreEqual(m_Library.poses.Count, poses.Length);
            for (int p = 0; p < poses.Length; p++)
            {
                Assert.AreEqual(m_Library.poses[p].name, poses[p].Name.ToString());
                Assert.AreEqual(m_Library.poses[p].duration, poses[p].Duration, 1e-5f);
                Assert.AreEqual(RigChannels.Count * poses[p].SampleCount, poses[p].Samples.Length);
            }
            Assert.AreEqual(m_Library.IndexOf("Yawn"), m_Blob.Value.IndexOf(new FixedString32Bytes("Yawn")));
        }

        [Test]
        public void EveryPoseStartsAndEndsAtRest()
        {
            ref var poses = ref m_Blob.Value.Poses;
            for (int p = 0; p < poses.Length; p++)
            {
                ref var pose = ref poses[p];
                for (int c = 0; c < RigChannels.Count; c++)
                {
                    float rest = RigChannels.RestValue((RigChannel)c);
                    int first = c * pose.SampleCount;
                    int last = first + pose.SampleCount - 1;
                    Assert.AreEqual(rest, pose.Samples[first], 1e-4f, $"{pose.Name} {(RigChannel)c} start");
                    Assert.AreEqual(rest, pose.Samples[last], 1e-4f, $"{pose.Name} {(RigChannel)c} end");
                }
            }
        }

        [Test]
        public void SamplingLoops()
        {
            ref var pose = ref m_Blob.Value.Poses[m_Library.IndexOf("Bounce")];
            int height = (int)RigChannel.BodyHeight;
            for (float t = 0f; t < pose.Duration; t += 0.13f)
                Assert.AreEqual(PoseSampling.Sample(ref pose, height, t), PoseSampling.Sample(ref pose, height, t + pose.Duration), 1e-4f);
            Assert.Greater(PoseSampling.Sample(ref pose, height, 0.52f), 0.15f, "Bounce should hop near t = 0.52s");
        }

        [Test]
        public void PickNextNeverRepeatsCurrentPose()
        {
            ref var poses = ref m_Blob.Value.Poses;
            var random = Random.CreateFromIndex(3);
            int current = 0;
            var seen = new bool[poses.Length];
            for (int i = 0; i < 500; i++)
            {
                int next = PoseSampling.PickNext(ref poses, current, ref random);
                Assert.AreNotEqual(current, next);
                seen[next] = true;
                current = next;
            }
            CollectionAssert.DoesNotContain(seen, false, "every pose should come up eventually");
        }

        [Test]
        public void HoldIsAWholeNumberOfLoops()
        {
            var random = Random.CreateFromIndex(5);
            for (int i = 0; i < 50; i++)
            {
                float hold = PoseSampling.HoldFor(2.4f, 4f, 9f, ref random);
                float loops = hold / 2.4f;
                Assert.AreEqual(math.round(loops), loops, 1e-4f);
                Assert.GreaterOrEqual(loops, 1f);
            }
        }

        [Test]
        public void IdleCyclerSwitchesPosesAndFinishesCrossfades()
        {
            ref var poses = ref m_Blob.Value.Poses;
            var cycler = new IdleCycler
            {
                MinHold = 4f, MaxHold = 9f, Crossfade = 0.45f,
                Blend = 1f, HoldRemaining = poses[0].Duration, Random = Random.CreateFromIndex(11),
            };

            int switches = 0;
            int last = cycler.CurrentPose;
            for (int frame = 0; frame < 60 * 60; frame++)
            {
                IdleCycling.Step(ref cycler, ref poses, 1f / 60f);
                if (cycler.CurrentPose != last)
                {
                    switches++;
                    last = cycler.CurrentPose;
                    Assert.AreNotEqual(cycler.PreviousPose, cycler.CurrentPose);
                }
            }
            Assert.GreaterOrEqual(switches, 5, "a minute of idling should visit several poses");
            Assert.LessOrEqual(cycler.Blend, 1f);
        }
    }
}
