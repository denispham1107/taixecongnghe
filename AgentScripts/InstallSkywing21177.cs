using System;
using System.IO;
using System.Linq;
using PhoViet;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class InstallSkywing21177
{
    const string Art="Assets/PhoViet/Art/Skywing/";
    const string Resource="Assets/PhoViet/Resources/Skywing/";
    const string Meshy="Assets/MeshyImports/Crimson_Skywing_20261110_013834/";

    static Transform Child(Transform parent,string name)
    {
        var child=new GameObject(name).transform;
        child.SetParent(parent,false);
        return child;
    }

    public static object Main()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Use Edit Mode to install the Skywing prefab.");
        Directory.CreateDirectory(Resource);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var importer=(ModelImporter)AssetImporter.GetAtPath(Art+"PV_Skywing.fbx");
        importer.materialImportMode=ModelImporterMaterialImportMode.None;
        importer.SaveAndReimport();
        var meshes=AssetDatabase.LoadAllAssetsAtPath(importer.assetPath).OfType<Mesh>().ToDictionary(mesh=>mesh.name);
        if(meshes.Count!=6)throw new InvalidOperationException("Expected two meshes per detail level.");
        var source=AssetDatabase.LoadAssetAtPath<Material>(Meshy+"Material.001.mat");
        if(source==null||source.mainTexture==null)throw new InvalidOperationException("Meshy textures were not imported.");
        string matPath=Art+"PV_Skywing.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if(material==null)
        {
            material=new Material(source);
            AssetDatabase.CreateAsset(material,matPath);
        }
        material.enableInstancing=true;
        EditorUtility.SetDirty(material);

        var scene=EditorSceneManager.NewPreviewScene();
        var root=new GameObject("Crimson Skywing");
        SceneManager.MoveGameObjectToScene(root,scene);
        try
        {
            var visual=root.AddComponent<SkywingVisual>();
            var airframe=Child(root.transform,"Airframe");
            var propeller=Child(airframe,"Front propeller");
            propeller.localPosition=new Vector3(-.0253f,0,1.8277f);
            var data=new SerializedObject(visual);
            data.FindProperty("airframe").objectReferenceValue=airframe;
            data.FindProperty("propeller").objectReferenceValue=propeller;
            data.ApplyModifiedPropertiesWithoutUndo();
            float[] heights={.15f,.06f,.007f};
            var lods=new LOD[heights.Length];
            for(int lod=0;lod<lods.Length;lod++)
            {
                var body=CreateRenderer(airframe,meshes["Skywing_Airframe_LOD"+lod],material,lod);
                var rotor=CreateRenderer(propeller,meshes["Skywing_Propeller_LOD"+lod],material,lod);
                lods[lod]=new LOD(heights[lod],new Renderer[]{body,rotor});
            }
            var group=root.AddComponent<LODGroup>();
            group.SetLODs(lods);
            group.RecalculateBounds();
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,Resource+"PV_Skywing.prefab");
            if(prefab==null)throw new InvalidOperationException("Could not save the Skywing prefab.");
            AssetDatabase.SaveAssets();
            return new {prefab=AssetDatabase.GetAssetPath(prefab),lodCount=lods.Length,meshCount=meshes.Count,
                material=material.shader.name,baseColor=AssetDatabase.GetAssetPath(material.mainTexture),
                meshBounds=meshes.Values.Select(mesh=>new {mesh.name,center=mesh.bounds.center.ToString(),size=mesh.bounds.size.ToString()}).ToArray()};
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    static MeshRenderer CreateRenderer(Transform parent,Mesh mesh,Material material,int lod)
    {
        var part=Child(parent,mesh.name);
        part.localRotation=Quaternion.Euler(-90,0,0);
        part.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=part.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial=material;
        renderer.shadowCastingMode=lod==0?ShadowCastingMode.On:ShadowCastingMode.Off;
        return renderer;
    }
}
