using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Predator Definition")]
    public sealed class PredatorDefinition : ScriptableObject
    {
        public FishDefinition fish;
        [Min(1)] public int maximumHealth = 100;
        [Min(.1f)] public float attackInterval = 3.5f;
        [Min(1)] public int netDamage = 18;
        [Min(1)] public int shockDamage = 28;
    }
}
