using UnityEngine;
namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Content Manifest")]
    public sealed class ContentManifest : ScriptableObject
    {
        public FishDefinition[] fish;
        public FacilityDefinition[] facilities;
        public NpcDefinition[] npcs;
    }
}
