using UnityEditor;
using UnityEngine.Localization.Tables;
namespace BreathIsland.Editor
{
    public static class GatheringDisplayAuthoring
    {
        public static void Build()
        {
            foreach(var locale in new[]{"ko","en"})
            {
                var table=AssetDatabase.LoadAssetAtPath<StringTable>("Assets/BreathIsland/Localization/GameText_"+locale+".asset");
                if(table.GetEntry("bag_gather_hold")==null)table.AddEntry("bag_gather_hold",locale=="ko"?"E 길게 · {0} 채집":"Hold E · Gather {0}");
                EditorUtility.SetDirty(table);EditorUtility.SetDirty(table.SharedData);
            }
            // Gathering balance remains unchanged until the user supplies the requested values.
            AssetDatabase.SaveAssets();
        }
    }
}
