using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PhoViet.Editor
{
    public static class PhoVietRouteUpgrade
    {
        [MenuItem("Phố Việt/Routes/Install connected junction roads")]
        public static void UpgradeOpenScene()
        {
            if(Application.isPlaying)throw new Exception("Exit Play Mode");var game=UnityEngine.Object.FindAnyObjectByType<PhoVietGame>();
            ApplyToGame(game);EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
        }
        public static void ApplyToGame(PhoVietGame game)
        {
            string materialPath="Assets/PhoViet/Fonts/WorldStreetText.mat";
            var textMaterial=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(textMaterial==null){textMaterial=new Material(Shader.Find("PhoViet/World Street Text")){name="PV_WorldStreetText"};AssetDatabase.CreateAsset(textMaterial,materialPath);}
            game.worldTextMaterial=textMaterial;StreetArt.ConfigureWorldText(game.vietnameseFont,textMaterial);
            foreach(var text in game.GetComponentsInChildren<TextMesh>(true))text.GetComponent<Renderer>().sharedMaterial=textMaterial;
            var arrowTemplate=JunctionStreetArt.CreateArrowMesh();var refreshed=new HashSet<Mesh>();
            foreach(var filter in game.scenery.GetComponentsInChildren<MeshFilter>(true))
            {
                if(!filter.name.StartsWith("Vạch mũi tên giao lộ",StringComparison.Ordinal)||filter.sharedMesh==null||!refreshed.Add(filter.sharedMesh))continue;
                EditorUtility.CopySerialized(arrowTemplate,filter.sharedMesh);EditorUtility.SetDirty(filter.sharedMesh);
            }
            UnityEngine.Object.DestroyImmediate(arrowTemplate);
            if(game.roadRows.Count==5)return;
            StreetArt.Font=game.vietnameseFont;
            foreach(var block in game.blocks)JunctionStreetArt.AddArrows(block);
            var source=game.blocks.ToArray();game.roadRows.Clear();
            var first=new PhoVietGame.RoadRow{root=StreetArt.Group("Đường chính • mô-đun nối tiếp",game.scenery)};
            foreach(var block in source){block.SetParent(first.root,true);first.tiles.Add(block);}game.roadRows.Add(first);
            var staticBikes=new Dictionary<string,Mesh>();
            for(int row=1;row<5;row++)
            {
                var road=new PhoVietGame.RoadRow{root=StreetArt.Group("Đường nhánh dự phòng • "+row,game.scenery)};
                for(int i=0;i<source.Length;i++)
                {
                    var tile=UnityEngine.Object.Instantiate(source[(i+row)%source.Length].gameObject,road.root).transform;
                    tile.name="Phố nhánh • "+row+" • khu "+i;tile.localPosition=new Vector3(0,0,i*80);tile.localRotation=Quaternion.identity;
                    foreach(var bike in tile.GetComponentsInChildren<BikeVisualAnimator>(true))
                    {
                        var skin=bike.visual.GetComponentsInChildren<SkinnedMeshRenderer>(true).First(s=>s.name.EndsWith("_Body",StringComparison.Ordinal));
                        string asset=bike.AssetName;
                        if(!staticBikes.TryGetValue(asset,out Mesh mesh))
                        {
                            string path="Assets/PhoViet/Generated/RouteParked_"+asset+".asset";mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                            if(mesh==null){mesh=new Mesh{name="PV_RouteParked_"+asset};skin.BakeMesh(mesh);AssetDatabase.CreateAsset(mesh,path);}staticBikes[asset]=mesh;
                        }
                        var material=skin.sharedMaterial;
                        for(int c=bike.transform.childCount-1;c>=0;c--)UnityEngine.Object.DestroyImmediate(bike.transform.GetChild(c).gameObject);
                        var visual=StreetArt.Group("Xe đậu chi tiết • tĩnh",bike.transform);visual.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;visual.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
                        UnityEngine.Object.DestroyImmediate(bike);
                    }
                    road.tiles.Add(tile);
                }
                road.root.gameObject.SetActive(false);game.roadRows.Add(road);
            }
            game.trafficLights=game.scenery.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("Tín hiệu ",StringComparison.Ordinal)).ToList();
            PhoVietBuilder.PersistGeometry(game.gameObject);
        }
    }
}
