using System;
using System.Linq;
using UnityEngine;

namespace PhoViet
{
    public sealed partial class PlayerCarVisual
    {
        BodyDentMesh[] damageMeshes;

        internal void DentBody(Vector3 point, Vector3 outward, float closing)
        {
            if (closing < .35f || visual == null) return;
            if (damageMeshes == null)
            {
                var lod = visual.GetComponent<LODGroup>();
                if (lod == null) return;
                damageMeshes = lod.GetLODs().SelectMany(level => level.renderers).OfType<SkinnedMeshRenderer>()
                    .Distinct().Select(skin => new BodyDentMesh(skin)).ToArray();
            }
            float strength = Mathf.InverseLerp(.35f, 36f, closing);
            foreach (var mesh in damageMeshes)
                mesh.Dent(transform.worldToLocalMatrix, point, outward, strength);
        }

        internal void ResetBodyDamage()
        {
            if (damageMeshes == null) return;
            foreach (var mesh in damageMeshes) mesh.Restore();
            damageMeshes = null;
        }

        void OnDestroy() => ResetBodyDamage();

        sealed class BodyDentMesh
        {
            readonly SkinnedMeshRenderer skin;
            readonly Mesh source;
            Mesh instance;
            Vector3[] rest, vertices, normals;
            BoneWeight[] weights;
            Matrix4x4[] bindPoses, toCar, toMesh;
            Transform[] bones;
            bool[] bodyBones;

            public BodyDentMesh(SkinnedMeshRenderer skin)
            {
                this.skin = skin;
                source = skin.sharedMesh;
            }

            public void Dent(Matrix4x4 carFromWorld, Vector3 point, Vector3 outward, float strength)
            {
                if (source == null || !source.isReadable) return;
                if (rest == null)
                {
                    rest = source.vertices;
                    vertices = (Vector3[])rest.Clone();
                    normals = source.normals;
                    weights = source.boneWeights;
                    bindPoses = source.bindposes;
                    bones = skin.bones;
                    bodyBones = Array.ConvertAll(bones, bone => bone != null &&
                        (bone.name == "Body" || bone.name == "Frame" || bone.name.StartsWith("Bumper", StringComparison.Ordinal)));
                    toCar = new Matrix4x4[bones.Length];
                    toMesh = new Matrix4x4[bones.Length];
                }
                if (weights.Length != vertices.Length || normals.Length != vertices.Length || bindPoses.Length != bones.Length) return;

                // The Supercar uses rigid skin weights. Work in the player frame
                // so the patch stays in metres through suspension, turns and roll.
                for (int i = 0; i < bones.Length; i++)
                {
                    if (!bodyBones[i]) continue;
                    toCar[i] = carFromWorld * bones[i].localToWorldMatrix * bindPoses[i];
                    toMesh[i] = toCar[i].inverse;
                }

                float depth = Mathf.Lerp(.025f, .31f, strength);
                float width = Mathf.Lerp(.36f, .85f, strength);
                float height = Mathf.Lerp(.30f, .58f, strength);
                Vector3 across = Vector3.Cross(Vector3.up, outward);
                bool changed = false;
                for (int i = 0; i < vertices.Length; i++)
                {
                    int bone = weights[i].boneIndex0;
                    if (!bodyBones[bone]) continue;
                    Vector3 delta = toCar[bone].MultiplyPoint3x4(rest[i]) - point;
                    float x = Vector3.Dot(delta, across) / width;
                    float y = delta.y / height;
                    float z = Vector3.Dot(delta, outward) / .58f;
                    float falloff = 1 - x*x - y*y - z*z;
                    if (falloff <= 0) continue;

                    Vector3 previous = toCar[bone].MultiplyVector(vertices[i] - rest[i]);
                    Vector3 next = Vector3.ClampMagnitude(previous - outward * (depth * falloff * falloff), .38f);
                    if ((next - previous).sqrMagnitude < 1e-10f) continue;
                    vertices[i] = rest[i] + toMesh[bone].MultiplyVector(next);

                    // Tilt the authored normals with the dent profile while keeping
                    // UV seams and the original shading away from the struck panel.
                    Vector3 gradient = (across*(x/width) + Vector3.up*(y/height)) * (-4 * depth * falloff);
                    Vector3 normal = toMesh[bone].transpose.MultiplyVector(normals[i]).normalized;
                    normal += gradient * Vector3.Dot(normal, outward);
                    normals[i] = toCar[bone].transpose.MultiplyVector(normal).normalized;
                    changed = true;
                }
                if (!changed) return;
                if (instance == null)
                {
                    instance = UnityEngine.Object.Instantiate(source);
                    instance.name = source.name + " (player damage)";
                    instance.MarkDynamic();
                    skin.sharedMesh = instance;
                }
                instance.SetVertices(vertices, 0, vertices.Length, UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
                instance.normals = normals;
            }

            public void Restore()
            {
                if (instance == null) return;
                if (skin != null) skin.sharedMesh = source;
                UnityEngine.Object.Destroy(instance);
            }
        }
    }
}
