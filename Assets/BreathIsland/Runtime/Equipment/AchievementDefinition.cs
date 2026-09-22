using UnityEngine;
namespace BreathIsland
{
    public enum AchievementCondition { CraftFurniture, TrainingHits, AllResources }
    [CreateAssetMenu(menuName = "Breath Island/Achievement")]
    public sealed class AchievementDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public AchievementCondition condition;
        public int threshold = 1;
    }
}
