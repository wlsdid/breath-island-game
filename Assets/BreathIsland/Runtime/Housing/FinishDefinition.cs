using UnityEngine;
namespace BreathIsland
{
    public enum FinishSurface { Wall, Floor }
    [CreateAssetMenu(menuName = "Breath Island/Interior Finish")]
    public sealed class FinishDefinition : ScriptableObject
    {
        public string id, nameKey;
        public FinishSurface surface;
        public Material material;
    }
}
