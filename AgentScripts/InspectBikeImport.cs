using System.Linq;
using UnityEditor;
using UnityEngine;
public static class InspectBikeImport
{
    public static object Main()
    {
        PhoViet.Editor.PhoVietBikeImport.ConfigureAll();
        string path="Assets/PhoViet/Art/Bikes/PV_HeroScooter.fbx";
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var instance=Object.Instantiate(model);
        try
        {
            var transforms=instance.GetComponentsInChildren<Transform>(true);
            return new {success=true,scale=instance.transform.localScale.ToString(),
                clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Select(c=>new{name=c.name,length=c.length,legacy=c.legacy}).ToArray(),
                bones=transforms.Where(t=>t.name=="WheelFront"||t.name=="WheelRear"||t.name=="Head").Select(t=>new{name=t.name,position=t.position.ToString(),rotation=t.rotation.eulerAngles.ToString()}).ToArray(),
                meshes=instance.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(s=>new{name=s.name,bounds=s.bounds.ToString(),vertices=s.sharedMesh.vertexCount,bones=s.bones.Length}).ToArray()};
        }
        finally{Object.DestroyImmediate(instance);}
    }
}
