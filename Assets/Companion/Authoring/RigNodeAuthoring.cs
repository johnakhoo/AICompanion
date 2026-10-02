using Unity.Entities;
using UnityEngine;

namespace Companion.Authoring
{
    /// <summary>
    /// Marks a GameObject in the companion hierarchy. Every node keeps a dynamic transform so the
    /// hierarchy survives baking; animated kinds also get a <see cref="RigPart"/> and their rest pose.
    /// </summary>
    [DisallowMultipleComponent]
    public class RigNodeAuthoring : MonoBehaviour
    {
        public PartKind kind = PartKind.Fixed;
        [Tooltip("-1 screen-left, +1 screen-right, 0 centre.")]
        [Range(-1, 1)] public int side;
        [Tooltip("Mouth segment or fill column index.")]
        public int index;

        class Baker : Baker<RigNodeAuthoring>
        {
            public override void Bake(RigNodeAuthoring authoring)
            {
                if (authoring.kind == PartKind.Fixed)
                {
                    GetEntity(TransformUsageFlags.Dynamic);
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.NonUniformScale);
                var companion = GetComponentInParent<CompanionAuthoring>();
                if (companion == null)
                {
                    Debug.LogError($"{authoring.name}: rig parts must sit under a CompanionAuthoring.", authoring);
                    return;
                }

                var transform = GetComponent<Transform>();
                AddComponent(entity, new RigPart
                {
                    Character = GetEntity(companion, TransformUsageFlags.Dynamic),
                    Kind = authoring.kind,
                    Side = (sbyte)authoring.side,
                    Index = (byte)authoring.index,
                });
                AddComponent(entity, new RigRest
                {
                    Position = transform.localPosition,
                    Rotation = transform.localRotation,
                    Scale = transform.localScale,
                });
            }
        }
    }
}
