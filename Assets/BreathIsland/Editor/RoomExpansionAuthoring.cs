using UnityEditor;
namespace BreathIsland.Editor
{
    public static class RoomExpansionAuthoring
    {
        public static void Build()
        {
            ValidateLayout();
            HomeInteriorAuthoring.Build();
            FinishMeshAuthoring.Build();
            AssetDatabase.SaveAssets();
        }
        public static void ValidateLayout()
        {
            var layout=RoomLayoutDefinition.Default;
            if(layout.entranceClearance.width<=0 || layout.entranceClearance.height<=0)
            {
                layout.entranceClearance=new UnityEngine.Rect(3,0,2,2);
                EditorUtility.SetDirty(layout);AssetDatabase.SaveAssets();
            }
        }
    }
}
