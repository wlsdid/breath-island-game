using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Facility Definition")]
    public sealed class FacilityDefinition : ScriptableObject
    {
        public string nameKey;
        public FacilityId id;
        public int[] coinCosts = { 60, 110 };
        public int[] materialCosts = { 3, 7 };
        public string LocalizedName(bool korean) => GameText.Get(nameKey, korean);
    }
}
