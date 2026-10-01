using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class HomeInteractionAuthoring
    {
        public static void Build()
        {
            var catalog=IslandLifeCatalog.Load();
            var chest=catalog.Furniture("chest");
            if(chest==null)
            {
                chest=ScriptableObject.CreateInstance<FurnitureDefinition>();chest.id="chest";chest.nameKey="life_furniture_chest";
                chest.model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BreathIsland/Art/ThirdParty/KayKit/Storage/chest_common_empty.fbx");
                chest.footprint=Vector2Int.one;chest.height=.85f;chest.woodCost=5;chest.interaction=FurnitureInteraction.Storage;
                AssetDatabase.CreateAsset(chest,"Assets/BreathIsland/Resources/Content/Life/FurnitureDefinition_chest.asset");
                catalog.furniture=catalog.furniture.Concat(new[]{chest}).ToArray();EditorUtility.SetDirty(catalog);
            }
            foreach(var id in new[]{"chair","couch","bed"})
            {
                var f=catalog.Furniture(id);f.interaction=id=="bed"?FurnitureInteraction.Bed:FurnitureInteraction.Seat;
                f.seatOffset=new Vector3(0,id=="couch"?.48f:.52f,0);f.seatYaw=0;EditorUtility.SetDirty(f);
            }
            foreach(var name in new[]{"HomeDecorateHud","HomeStorageHud","HomeRestHud"})
            {
                string path="Assets/BreathIsland/Resources/UI/Screens/"+name+".prefab";
                if(!File.Exists(path))AssetDatabase.CopyAsset("Assets/BreathIsland/Resources/UI/Screens/HousingHud.prefab",path);
            }
            const string roomPath="Assets/BreathIsland/Resources/World/HomeInterior.prefab";
            var room=PrefabUtility.LoadPrefabContents(roomPath);
            if(room.transform.Find("Walking bounds")==null)
            {
                var bounds=new GameObject("Walking bounds");bounds.transform.SetParent(room.transform,false);
                Boundary(bounds.transform,new Vector3(-.15f,1.5f,4),new Vector3(.3f,3,8.6f));
                Boundary(bounds.transform,new Vector3(8.15f,1.5f,4),new Vector3(.3f,3,8.6f));
                Boundary(bounds.transform,new Vector3(4,1.5f,8.15f),new Vector3(8,.3f+2.7f,.3f));
                Boundary(bounds.transform,new Vector3(4,1.5f,-.15f),new Vector3(8,3,.3f));
            }
            PrefabUtility.SaveAsPrefabAsset(room,roomPath);PrefabUtility.UnloadPrefabContents(room);
            var ko=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            foreach(var row in Text){if(ko.GetEntry(row.Item1)==null)ko.AddEntry(row.Item1,row.Item2);if(en.GetEntry(row.Item1)==null)en.AddEntry(row.Item1,row.Item3);}
            EditorUtility.SetDirty(ko);EditorUtility.SetDirty(en);EditorUtility.SetDirty(ko.SharedData);
            AssetDatabase.SaveAssets();StorageChestAuthoring.Build();

        }
        private static void Boundary(Transform parent,Vector3 center,Vector3 size)
        {var g=new GameObject("Room boundary");g.transform.SetParent(parent,false);var box=g.AddComponent<BoxCollider>();box.center=center;box.size=size;}
        private static readonly (string,string,string)[] Text={
            ("life_furniture_chest","보관 상자","Storage chest"),
            ("home_decorate","집 꾸미기","Decorate room"),
            ("home_walk_hint","WASD 이동 · 가구를 바라보고 E로 사용","WASD to walk · Face furniture and press E"),
            ("home_stand_hint","E 또는 이동 키 · 일어나기","E or movement keys · Stand up"),
            ("home_exit_hint","E · 집 나가기","E · Leave home"),
            ("home_sit_hint","E · 앉기","E · Sit down"),
            ("home_sleep_hint","E · 다음 날까지 쉬기","E · Rest until tomorrow"),
            ("home_storage_hint","E · 보관 상자 열기","E · Open storage chest"),
            ("home_sleep_title","오늘은 여기까지 쉴까요?","Rest for the day?"),
            ("home_sleep_body","다음 날 오전 9시에 일어나요. 섬의 자원이 다시 자라요.","Wake up at 09:00 tomorrow. The island's resources regrow."),
            ("home_chest_not_empty","상자를 비운 뒤 회수해주세요.","Empty this chest before reclaiming it."),
            ("home_storage_title","소중한 물건 보관함","A place for little treasures"),
            ("home_storage_capacity","이 상자 · 16칸","This chest · 16 slots"),
            ("home_storage_help","자원은 한 칸에 99개, 가구는 1개씩. 제작에는 가방 속 자원을 사용해요.","Each slot holds 99 resources or 1 furniture item. Crafting uses resources in your satchel."),
            ("home_quantity","수량 {0}","Quantity {0}"),
            ("home_withdraw","상자 → 가방","Chest → Satchel"),
            ("home_deposit","가방 → 상자","Satchel → Chest"),
            ("home_transfer_one","1개 옮기기","Move one"),
            ("home_transfer_quantity","선택한 수량 옮기기","Move selected quantity"),
            ("home_transfer_all","가능한 만큼 옮기기","Move as many as fit"),
            ("home_storage_full","옮길 수 있는 물건이나 빈 공간이 없어요.","No items or free space available for this transfer.")
        };
    }
}
