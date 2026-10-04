using UnityEngine;
namespace BreathIsland
{
    public enum LifeTool { None, Axe, Pickaxe, Hammer }
    public enum LifeResource { Wood, Stone, Herb }
    [CreateAssetMenu(menuName = "Breath Island/Resource Node")]
    public sealed class ResourceNodeDefinition : ScriptableObject
    {
        public string id;
        public string nameKey;
        public LifeTool tool;
        public LifeResource resource;
        public int yield = 1;
        public GameObject model;
        public float height = 1;
        public Vector3 position;
        [Min(1)] public int requiredHits = 1;
        [Min(0)] public float gatherSeconds;
    }
}
