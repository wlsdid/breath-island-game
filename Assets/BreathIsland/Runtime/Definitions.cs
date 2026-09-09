using System;
using UnityEngine;

namespace BreathIsland
{
    public enum GamePhase { Explore, Fishing, CatchResolution, PredatorCombat, Dialogue, Facility, Collection, Pause }
    public enum CatchDisposition { Sell, Record, Display, Material }
    public enum FishRarity { Common, Uncommon, Rare, Predator }
    public enum FacilityId { Boatyard, Aquarium }
    public enum NpcId { Nari, Maru }

    [CreateAssetMenu(menuName = "Breath Island/Fish Definition")]
    public sealed class FishDefinition : ScriptableObject
    {
        public string id = "fish";
        public string koreanName = "물고기";
        public string englishName = "Fish";
        [TextArea] public string koreanDescription = "바다 생물";
        [TextArea] public string englishDescription = "A sea creature";
        public FishRarity rarity;
        public Color color = Color.cyan;
        [Min(0)] public int coinValue = 10;
        [Min(0)] public int materialValue = 1;
        [Range(1f, 20f)] public float pullStrength = 5f;
        [Range(0, 23)] public int startHour;
        [Range(0, 23)] public int endHour = 23;
        public string preferredBait = "Basic";
        public bool IsPredator => rarity == FishRarity.Predator;
        public string LocalizedName(bool korean) => korean ? koreanName : englishName;
        public string LocalizedDescription(bool korean) => korean ? koreanDescription : englishDescription;
    }

    [CreateAssetMenu(menuName = "Breath Island/Predator Definition")]
    public sealed class PredatorDefinition : ScriptableObject
    {
        public FishDefinition fish;
        [Min(1)] public int maximumHealth = 100;
        [Min(.1f)] public float attackInterval = 3.5f;
        [Min(1)] public int netDamage = 18;
        [Min(1)] public int shockDamage = 28;
    }

    [CreateAssetMenu(menuName = "Breath Island/Facility Definition")]
    public sealed class FacilityDefinition : ScriptableObject
    {
        public FacilityId id;
        public string koreanName;
        public string englishName;
        public int[] coinCosts = { 60, 110 };
        public int[] materialCosts = { 3, 7 };
        public string LocalizedName(bool korean) => korean ? koreanName : englishName;
    }

    [CreateAssetMenu(menuName = "Breath Island/NPC Definition")]
    public sealed class NpcDefinition : ScriptableObject
    {
        public NpcId id;
        public string koreanName;
        public string englishName;
        public string koreanRole;
        public string englishRole;
        [TextArea] public string[] koreanLines;
        [TextArea] public string[] englishLines;
        public Color color = Color.white;
        public string LocalizedName(bool korean) => korean ? koreanName : englishName;
        public string LocalizedRole(bool korean) => korean ? koreanRole : englishRole;
        public string LocalizedLine(bool korean, int index)
        {
            var lines = korean ? koreanLines : englishLines;
            return lines == null || lines.Length == 0 ? string.Empty : lines[Mathf.Abs(index) % lines.Length];
        }
    }
}

