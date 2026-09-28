using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class VillageLayout : MonoBehaviour
    {
        public Vector3[] safePoints;
        public bool Blocks(Vector3 feet)
        {
            var player = new Bounds(feet + Vector3.up * .82f, new Vector3(.84f, 1.65f, .84f));
            return GetComponentsInChildren<VillageObstacle>().Any(o => o.GetComponent<BoxCollider>().bounds.Intersects(player));
        }
        public Vector3 RestoreSafePosition(Vector3 position)
        {
            if (!Blocks(position)) return position;
            foreach (var point in safePoints.Select(transform.TransformPoint).OrderBy(p => (p - position).sqrMagnitude))
                if (!Blocks(point)) return point;
            return position;
        }
    }
}
