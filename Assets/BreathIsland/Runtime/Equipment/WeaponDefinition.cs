using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public JobDefinition job;
        public SkillDefinition basicAttack;
        public SkillDefinition skill;
        public GameObject model;
        public string achievementId;
    }
}
