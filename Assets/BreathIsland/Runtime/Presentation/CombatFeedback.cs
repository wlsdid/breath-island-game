using UnityEngine;
namespace BreathIsland
{
    // Presentation hook retained for callers; the placeholder beam is intentionally removed.
    public sealed class CombatFeedback : MonoBehaviour
    {
        public void Initialize(Material source) { }
        public void Show(Vector3 from, Vector3 to, Color color) { }
    }
}
