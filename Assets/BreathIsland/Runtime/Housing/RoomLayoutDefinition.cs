using UnityEngine;

namespace BreathIsland
{
    [CreateAssetMenu(menuName = "Breath Island/Room Layout")]
    public sealed class RoomLayoutDefinition : ScriptableObject
    {
        public string id = "home_default";
        public int width = 20;
        public int depth = 20;
        public Vector2 arrival = new(4, 1);
        public Rect entranceClearance = new(3, 0, 2, 2);
        public RectInt[] excludedAreas = System.Array.Empty<RectInt>();
        public bool Contains(Rect area)
        {
            if (area.xMin < 0 || area.yMin < 0 || area.xMax > width || area.yMax > depth) return false;
            foreach (var excluded in excludedAreas)
                if (area.Overlaps(new Rect(excluded.x, excluded.y, excluded.width, excluded.height))) return false;
            return true;
        }
        public static RoomLayoutDefinition Default => Resources.Load<RoomLayoutDefinition>("Content/Rooms/HomeDefault")
            ?? throw new System.InvalidOperationException("Missing authored home layout.");
    }
}
