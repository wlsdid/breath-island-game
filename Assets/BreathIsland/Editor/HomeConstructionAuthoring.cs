using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class HomeConstructionAuthoring
    {
        public static void Build()
        {
            const string output="Assets/BreathIsland/Resources/World/";
            var material=AssetDatabase.LoadAssetAtPath<Material>(output+"Town.mat");
            var root=new GameObject("Build Floor");
            var visual=GameObject.CreatePrimitive(PrimitiveType.Cube);visual.name="Visual";visual.transform.SetParent(root.transform,false);
            visual.transform.localPosition=new Vector3(0,-.1f,0);visual.transform.localScale=new Vector3(1,.2f,1);
            Object.DestroyImmediate(visual.GetComponent<Collider>());visual.GetComponent<Renderer>().sharedMaterial=material;
            var box=root.AddComponent<BoxCollider>();box.center=new Vector3(0,-.1f,0);box.size=new Vector3(1,.2f,1);
            PrefabUtility.SaveAsPrefabAsset(root,output+"BuildFloor.prefab");Object.DestroyImmediate(root);
            root=new GameObject("Build Wall");var holder=new GameObject("Visual");holder.transform.SetParent(root.transform,false);
            HomeInteriorAuthoring.Part(holder.transform,"wall-wood",Vector3.zero,new Vector3(1,2.7f,.16f),0,material);
            box=root.AddComponent<BoxCollider>();box.center=new Vector3(0,1.5f,0);box.size=new Vector3(1,3,.16f);
            PrefabUtility.SaveAsPrefabAsset(root,output+"BuildWall.prefab");Object.DestroyImmediate(root);
            var words=new (string key,string ko,string en)[]{
                ("build_title","집 확장·변형","Remodel home"),("build_floor","바닥","Floor"),
                ("build_wall_n","위쪽 벽","North wall"),("build_wall_e","오른쪽 벽","East wall"),
                ("build_apply","변경 적용","Apply changes"),
                ("build_cost","나무 비용 {0} · 보유 {1}","Wood cost {0} · Owned {1}"),
                ("build_help","최대 32×32 · 칸을 눌러 추가/철거\n바닥 2, 벽 1 나무\n출입구·가구·통로는 유지됩니다.","Up to 32×32 · Click to add/remove\nFloor: 2 wood · Wall: 1\nKeep the entrance and furniture clear."),
                ("build_blocked","출입구·가구·플레이어와 연결된 통로를 유지해주세요.","Keep the entrance, furniture and player clear, with connected paths."),
                ("build_need_wood","변경에 필요한 나무가 부족합니다.","Not enough wood for these changes.")};
            foreach(var locale in new[]{"ko","en"})
            {
                var table=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_"+locale+".asset");
                foreach(var word in words)table.AddEntry(word.key,locale=="ko"?word.ko:word.en);
                EditorUtility.SetDirty(table);EditorUtility.SetDirty(table.SharedData);
            }
            VillageAuthoring.Build();AssetDatabase.SaveAssets();
        }
    }
}
