using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Furniture")]
    public sealed class FurnitureDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public GameObject model;
        public Vector2Int footprint = Vector2Int.one;
        public float height = 1;
        public int woodCost;
        public int stoneCost;
    }
}
