using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Skill")]
    public sealed class SkillDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public int damage = 25;
        public float range = 3;
        public float cooldown = 2;
        public Color effectColor = Color.cyan;
    }
}
