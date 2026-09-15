using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Fish Definition")]
    public sealed class FishDefinition : ScriptableObject
    {
        public string nameKey;
        public string id = "fish";
        public FishRarity rarity;
        public Color color = Color.cyan;
        [Min(0)] public int coinValue = 10;
        [Min(0)] public int materialValue = 1;
        [Range(1f, 20f)] public float pullStrength = 5f;
        [Range(0, 23)] public int startHour;
        [Range(0, 23)] public int endHour = 23;
        public string preferredBait = "Basic";
        public bool IsPredator => rarity == FishRarity.Predator;
        public string LocalizedName(bool korean) => GameText.Get(nameKey, korean);
        public string descriptionKey;
        public string LocalizedDescription(bool korean) => GameText.Get(descriptionKey, korean);
    }
}
