using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Job")]
    public sealed class JobDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
    }
}
