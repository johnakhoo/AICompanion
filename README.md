# AICompanion

A 3D AI companion for the web. A round, ball-shaped character with a simple face (two eyebrows, two eyes, a nose and a mouth) stands in the middle of the screen and switches between animated idle poses.

- **Unity** 6000.6.4f1, URP (Forward+)
- **Pure DOTS ECS at runtime:** Entities 6.6 and Entities Graphics, `ISystem` + `IJobEntity` + Burst. None of the project's runtime code is a MonoBehaviour.
- **Web build is WebGPU only.** Entities Graphics needs compute/storage buffers that WebGL 2 lacks. Unity lists Entities Graphics as unsupported on web platforms, so treat this as experimental.

## How it works

| Piece | Where |
| --- | --- |
| Character hierarchy (primitive meshes + `RigNodeAuthoring`) | `Assets/Scenes/Companion/CompanionSubScene.unity` (generated) |
| Idle poses: one `AnimationCurve` per `RigChannel` per pose | `Assets/Companion/Generated/CompanionPoseLibrary.asset` |
| Bakers: pose curves → `PoseLibraryBlob`, parts → `RigPart` + `RigRest` | `Assets/Companion/Authoring` (compiled only in the Editor) |
| Systems: `IdleCycleSystem` → `PoseSampleSystem` + `BlinkSystem` → `RigApplySystem` | `Assets/Companion/Runtime/Systems` |
| Mouth geometry (capsule lip lines and fill on the ball surface) | `Assets/Companion/Runtime/Rig/MouthContour.cs` |

Facial features hang off pivots at the ball's centre. Rotating a pivot slides its feature across the sphere, so looking around, raising brows and so on never lift anything off the surface. Squash and stretch is a `PostTransformMatrix` on `Body`, which the whole face inherits.

To drive a pose from code (for example, a future AI layer), set `PoseRequest.PoseIndex` on the character entity and enable the component.

## Editing

- **Tools ▸ Companion ▸ Rebuild All** regenerates materials, scenes and settings. It keeps your pose edits.
- **Tools ▸ Companion ▸ Reset Poses To Defaults** restores the poses from `DefaultPoses.cs`.
- Tweak the poses by editing the curves in `CompanionPoseLibrary.asset`. The SubScene rebakes automatically.

## Command line

```bash
# Regenerate content
"/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents/MacOS/Unity" -batchmode -quit -projectPath . \
  -executeMethod Companion.EditorTools.CompanionContentBuilder.BuildAll

# EditMode tests
unity test . --editor-version 6000.6.4f1 --mode EditMode

# Web build, then serve locally. WebGPU needs a secure context, which localhost counts as.
unity build . --editor-version 6000.6.4f1 --target WebGL \
  --execute-method Companion.EditorTools.WebBuild.Build --output-path Builds/Web
python3 -m http.server 8080 --directory Builds/Web
```
