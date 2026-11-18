using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoViet.OpenWorld
{
    // 21.259 — móp thân xe theo điểm chạm (bản đồ mở). Lõi lấy từ PlayerCarDamage.BodyDentMesh (21.192) của game cũ:
    // biến dạng các đỉnh gắn xương Body/Frame/Bumper* quanh điểm chạm (hình elip, sâu 0,025–0,31 m theo vận tốc tiếp cận
    // 0,35–36 m/s, tối đa 0,38 m mỗi đỉnh), nghiêng pháp tuyến theo vết lõm, giữ UV; tạo mesh riêng khi bị móp, trả mesh gốc
    // khi chơi lại/xe NPC sinh lại. Dùng chung cho xe người chơi (Supercar) và 4 mẫu ô tô Meshy (cùng khung xương; cần bật
    // Read/Write cho FBX). Lưới không đọc được thì bỏ qua (chỉ còn mảnh vỡ).
    public sealed class OpenWorldCarDent
    {
        readonly Transform car;
        readonly List<Patch> patches = new List<Patch>();
        public bool Damaged { get; private set; }

        public OpenWorldCarDent(Transform carRoot, IEnumerable<Renderer> renderers)
        {
            car = carRoot;
            foreach (var r in renderers) if (r is SkinnedMeshRenderer s && s.sharedMesh) patches.Add(new Patch(s));
        }

        // point/outward theo toạ độ thế giới; outward = hướng từ trong thân xe ra phía bị tông.
        public void Dent(Vector3 worldPoint, Vector3 worldOutward, float closing)
        {
            if (closing < .35f || !car) return;
            var m = car.worldToLocalMatrix;
            var p = m.MultiplyPoint3x4(worldPoint);
            var o = m.MultiplyVector(worldOutward); o.y = 0; if (o.sqrMagnitude < 1e-4f) return; o.Normalize();
            float strength = Mathf.InverseLerp(.35f, 36f, closing);
            foreach (var patch in patches) if (patch.Dent(m, p, o, strength)) Damaged = true;
        }

        public void Restore()
        {
            foreach (var patch in patches) patch.Restore();
            Damaged = false;
        }

        sealed class Patch
        {
            readonly SkinnedMeshRenderer skin; readonly Mesh source; Mesh instance;
            Vector3[] rest, vertices, normals; BoneWeight[] weights; Matrix4x4[] bindPoses, toCar, toMesh; Transform[] bones; bool[] body;

            public Patch(SkinnedMeshRenderer s) { skin = s; source = s.sharedMesh; }

            public bool Dent(Matrix4x4 carFromWorld, Vector3 point, Vector3 outward, float strength)
            {
                if (!source || !source.isReadable || !skin) return false;
                if (rest == null)
                {
                    rest = source.vertices; vertices = (Vector3[])rest.Clone(); normals = source.normals;
                    weights = source.boneWeights; bindPoses = source.bindposes; bones = skin.bones;
                    body = Array.ConvertAll(bones, b => b && (b.name == "Body" || b.name == "Frame" || b.name.StartsWith("Bumper", StringComparison.Ordinal)));
                    toCar = new Matrix4x4[bones.Length]; toMesh = new Matrix4x4[bones.Length];
                }
                if (weights.Length != vertices.Length || normals.Length != vertices.Length || bindPoses.Length != bones.Length) return false;
                for (int i = 0; i < bones.Length; i++)
                {
                    if (!body[i]) continue;
                    toCar[i] = carFromWorld * bones[i].localToWorldMatrix * bindPoses[i]; toMesh[i] = toCar[i].inverse;
                }
                float depth = Mathf.Lerp(.025f, .31f, strength), width = Mathf.Lerp(.36f, .85f, strength), height = Mathf.Lerp(.30f, .58f, strength);
                var across = Vector3.Cross(Vector3.up, outward);
                bool changed = false;
                for (int i = 0; i < vertices.Length; i++)
                {
                    int bone = weights[i].boneIndex0; if (!body[bone]) continue;
                    var delta = toCar[bone].MultiplyPoint3x4(rest[i]) - point;
                    float x = Vector3.Dot(delta, across) / width, y = delta.y / height, z = Vector3.Dot(delta, outward) / .58f;
                    float falloff = 1 - x * x - y * y - z * z; if (falloff <= 0) continue;
                    var previous = toCar[bone].MultiplyVector(vertices[i] - rest[i]);
                    var next = Vector3.ClampMagnitude(previous - outward * (depth * falloff * falloff), .38f);
                    if ((next - previous).sqrMagnitude < 1e-10f) continue;
                    vertices[i] = rest[i] + toMesh[bone].MultiplyVector(next);
                    var gradient = (across * (x / width) + Vector3.up * (y / height)) * (-4 * depth * falloff);
                    var normal = toMesh[bone].transpose.MultiplyVector(normals[i]).normalized;
                    normal += gradient * Vector3.Dot(normal, outward);
                    normals[i] = toCar[bone].transpose.MultiplyVector(normal).normalized;
                    changed = true;
                }
                if (!changed) return false;
                if (!instance) { instance = UnityEngine.Object.Instantiate(source); instance.name = source.name + " (móp)"; instance.MarkDynamic(); skin.sharedMesh = instance; }
                instance.SetVertices(vertices, 0, vertices.Length, UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds);
                instance.normals = normals;
                return true;
            }

            public void Restore()
            {
                if (!instance) return;
                if (skin) skin.sharedMesh = source;
                UnityEngine.Object.Destroy(instance); instance = null;
                if (rest != null) { vertices = (Vector3[])rest.Clone(); normals = source.normals; }
            }
        }
    }
}
