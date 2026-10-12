using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    // Explicit editor command. Not called by startup or the player build.
    public static class PortfolioAuthoring
    {
        public static void Prepare()
        {
            string[,] entries = {
                {"gather_face_target", "대상을 바라보고 사용하세요", "Face the target to use your tool"},
                {"gather_hits", "채집 {0} / {1}회", "Gathering {0} / {1} hits"},
                {"gather_seconds", "E 유지 · 남은 시간 {0:0.0}초", "Hold E · {0:0.0}s remaining"},
                {"place_ready", "배치 가능 · 확정하면 저장돼요", "Ready to place · Confirm to save"},
                {"place_blocked", "빈 바닥을 선택하세요", "Choose an unoccupied floor area"},
                {"build_preview_only", "미리보기 · 적용하면 저장", "Preview · Apply to save"},
                {"build_need_wood", "가방에 나무가 부족해요", "Not enough wood in your bag"}
            };
            for (int language = 0; language < 2; language++)
            {
                var table = AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_" + (language == 0 ? "ko" : "en") + ".asset");
                for (int i = 0; i < entries.GetLength(0); i++) table.AddEntry(entries[i,0], entries[i,language+1]);
                EditorUtility.SetDirty(table); EditorUtility.SetDirty(table.SharedData);
            }
            PolishVillage();
            AssetDatabase.SaveAssets();
            ExportSeeds();
        }
        private static void PolishVillage()
        {
            const string materialPath="Assets/BreathIsland/Resources/World/VillagePath.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null)
            {
                material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor",new Color(.65f,.59f,.45f));
                material.SetFloat("_Smoothness",.05f);
                AssetDatabase.CreateAsset(material,materialPath);
            }
            const string path="Assets/BreathIsland/Resources/World/ForestVillage.prefab";
            var village=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(Transform child in village.transform)
                    if(child.name=="road") foreach(var renderer in child.GetComponentsInChildren<Renderer>())
                    {var slots=renderer.sharedMaterials;for(int i=0;i<slots.Length;i++)slots[i]=material;renderer.sharedMaterials=slots;}
                PrefabUtility.SaveAsPrefabAsset(village,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(village);}
        }
        [MenuItem("숨결섬/포트폴리오 시연 저장 생성")]
        public static void ExportSeeds()
        {
            string root = Path.GetFullPath("Documentation/Portfolio/Seeds");
            Directory.CreateDirectory(root);
            foreach (string scene in new[] {"village", "sea", "home", "gather"})
            {
                var save = GameSaveData.CreateDefault();
                save.resources.wood = 30; save.resources.stone = 8; save.resources.herbs = 2;
                save.housing.starterBedGranted = true;
                save.housing.outdoorPosition = HousingSystem.Entrance;
                save.housing.stock.Add(new FurnitureStock {id="chair",count=1});
                save.housing.stock.Add(new FurnitureStock {id="lamp",count=1});
                Add(save,"bed",1,5); Add(save,"cabinet",1,10); Add(save,"shelf",5,10);
                Add(save,"couch",7,7); Add(save,"table_low",7,5); Add(save,"lamp",11,7);
                Add(save,"table",4,4); Add(save,"chair",4,6); Add(save,"chest",1,3);
                var position = scene == "home" ? HousingSystem.InteriorOrigin + new Vector3(5, .1f, 2)
                    : scene == "sea" ? new Vector3(0,1.2f,23)
                    : scene == "gather" ? new Vector3(-5,1.2f,-3.5f) : new Vector3(0,1.2f,8);
                save.housing.inside = scene == "home";
                save.player.x=position.x; save.player.y=position.y; save.player.z=position.z;
                var catalog=IslandLifeCatalog.Load();
                for(int i=0;i<save.housing.placed.Count;i++)
                {
                    var item=save.housing.placed[i];
                    var others=save.housing.placed.GetRange(0,i);
                    if(!FurniturePlacement.Valid(catalog,catalog.Furniture(item.furnitureId),item.x,item.z,item.turns,others,save.housing.structure))
                        throw new InvalidOperationException("Invalid demo furniture: "+item.furnitureId);
                }
                File.WriteAllText(Path.Combine(root,scene+".json"),JsonUtility.ToJson(save,true));
            }
            Debug.Log("Portfolio seeds exported; no live saves were accessed.");
        }
        private static void Add(GameSaveData save,string id,int x,int z)
            => save.housing.placed.Add(new PlacedFurniture{instanceId="portfolio-"+id,furnitureId=id,x=x,z=z});
    }
}
