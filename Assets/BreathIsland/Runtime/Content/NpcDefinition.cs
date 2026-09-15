using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/NPC Definition")]
    public sealed class NpcDefinition : ScriptableObject
    {
        public string nameKey;
        public NpcId id;
        public Color color = Color.white;
        public string LocalizedName(bool korean) => GameText.Get(nameKey, korean);
        public string roleKey;
        public string[] lineKeys;
        public string LocalizedRole(bool korean) => GameText.Get(roleKey, korean);
        public string LocalizedLine(bool korean, int index)
        {
            var lines = lineKeys;
            return lines == null || lines.Length == 0 ? string.Empty : GameText.Get(lines[(int)(System.Math.Abs((long)index) % lines.Length)], korean);
        }
    }
}
