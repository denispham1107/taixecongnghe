using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PhoViet
{
    internal static class CarWheelDetail
    {
        // Positions just outside the original rim faces, measured from each FBX wheel pivot.
        static readonly Dictionary<string, float[]> RimFaces = new Dictionary<string, float[]>
        {
            { "PV_BlueSupercar", new[] { .047f, .054f } },
            { "PV_MeshySport", new[] { -.012f, -.012f } },
            { "PV_MeshyPickup", new[] { .077f, .077f } },
            { "PV_MeshyCovered", new[] { .053f, -.046f } },
            { "PV_MeshyContainer", new[] { .122f, .017f, .017f, -.010f } }
        };
        static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
        const string DetailName = "Rotating rim inlays";

        public static void Bind(string asset, Transform visual, Transform[] wheels, float localRadius)
        {
            if (!RimFaces.TryGetValue(asset, out var faces) || wheels.Length != faces.Length * 2 ||
                visual.Find(DetailName) != null) return;

            if (!Meshes.TryGetValue(asset, out var mesh) || mesh == null)
                Meshes[asset] = mesh = Build(asset, visual, wheels, localRadius, faces);

            var detail = new GameObject(DetailName);
            detail.layer = visual.gameObject.layer;
            detail.transform.SetParent(visual, false);
            var skin = detail.AddComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = mesh;
            skin.sharedMaterial = StreetArt.Mat(new Color(.47f, .51f, .55f));
            skin.bones = wheels;
            skin.rootBone = visual;
            skin.quality = SkinQuality.Bone1;
            skin.shadowCastingMode = ShadowCastingMode.Off;
            var bounds = mesh.bounds;
            bounds.Expand(.6f);
            skin.localBounds = bounds;
        }

        static Mesh Build(string asset, Transform visual, Transform[] wheels, float radius, float[] faces)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var weights = new List<BoneWeight>();
            var triangles = new List<int>();
            var binds = new Matrix4x4[wheels.Length];
            for (int i = 0; i < wheels.Length; i++)
            {
                var local = visual.worldToLocalMatrix * wheels[i].localToWorldMatrix;
                binds[i] = local.inverse;
                float side = i % 2 == 0 ? -1 : 1;
                float x = faces[i / 2] * side;
                var normal = local.MultiplyVector(Vector3.right * side).normalized;
                for (int spoke = 0; spoke < 5; spoke++)
                {
                    // A longer lead spoke makes rotation legible even on a nearly symmetrical rim.
                    float angle = spoke * (Mathf.PI * 2 / 5);
                    float length = (spoke == 0 ? .47f : .38f) * radius;
                    int start = vertices.Count;
                    vertices.Add(local.MultiplyPoint3x4(Point(x, angle - .24f, .17f * radius)));
                    vertices.Add(local.MultiplyPoint3x4(Point(x, angle - .13f, length)));
                    vertices.Add(local.MultiplyPoint3x4(Point(x, angle + .16f, length)));
                    vertices.Add(local.MultiplyPoint3x4(Point(x, angle + .25f, .17f * radius)));
                    for (int j = 0; j < 4; j++)
                    {
                        normals.Add(normal);
                        weights.Add(new BoneWeight { boneIndex0 = i, weight0 = 1 });
                    }
                    triangles.Add(start);
                    triangles.Add(start + (side > 0 ? 1 : 2));
                    triangles.Add(start + (side > 0 ? 2 : 1));
                    triangles.Add(start);
                    triangles.Add(start + (side > 0 ? 2 : 3));
                    triangles.Add(start + (side > 0 ? 3 : 2));
                }
            }

            var mesh = new Mesh { name = asset + " rim inlays" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.bindposes = binds;
            mesh.boneWeights = weights.ToArray();
            return mesh;
        }

        static Vector3 Point(float x, float angle, float radius)
            => new Vector3(x, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
    }
}
