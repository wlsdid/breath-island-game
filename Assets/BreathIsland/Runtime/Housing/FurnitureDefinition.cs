using UnityEngine;
namespace BreathIsland
{
    public enum FurnitureInteraction { None, Seat, Bed, Storage }
    [CreateAssetMenu(menuName = "Breath Island/Furniture")]
    public sealed class FurnitureDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public GameObject model;
        public Material material;
        public Vector2Int footprint = Vector2Int.one;
        public float height = 1;
        public int woodCost;
        public int stoneCost;
        public FurnitureInteraction interaction;
        public Vector3 seatOffset = new(0, .5f, 0);
        public float seatYaw;
    }
}
