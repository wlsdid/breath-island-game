using UnityEditor;
using UnityEngine.Localization.Tables;

namespace BreathIsland.Editor
{
    public static class FishingBiteLocalization
    {
        public static void Build()
        {
            var ko=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_ko.asset");
            var en=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_en.asset");
            var rows=new (string key,string ko,string en)[]
            {
                ("fishing_bite_now","입질! 지금 챔질하세요","A bite! Set the hook"),
                ("fishing_hook_hint","좌클릭 / Space / 패드 A로 챔질","Left click / Space / Pad A to hook"),
                ("fishing_bite_time","남은 시간 {0:0.0}초","Time left: {0:0.0}s"),
                ("fishing_hook_action","챔질하기","Set the hook"),
                ("fishing_bite_missed","입질을 놓쳤어요","The fish got away")
            };
            foreach(var row in rows)
            {
                if(ko.GetEntry(row.key)==null)ko.AddEntry(row.key,row.ko);
                if(en.GetEntry(row.key)==null)en.AddEntry(row.key,row.en);
            }
            EditorUtility.SetDirty(ko);EditorUtility.SetDirty(en);EditorUtility.SetDirty(ko.SharedData);AssetDatabase.SaveAssets();
        }
    }
}
