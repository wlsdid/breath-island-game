using UnityEngine;
namespace BreathIsland
{
    public sealed class CombatFeedback : MonoBehaviour
    {
        private LineRenderer trace;
        private float remaining;
        private Material material;
        private Material template;
        public void Initialize(Material source) => template = source;
        public void Show(Vector3 from, Vector3 to, Color color)
        {
            if (trace == null)
            {
                trace = gameObject.AddComponent<LineRenderer>();
                material = new Material(Resources.Load<Material>("World/Coastal/Line")); material.mainTexture = null;
                trace.sharedMaterial = material; trace.positionCount = 2; trace.startWidth = .025f; trace.endWidth = .012f;
            }
            material.color = color; trace.SetPosition(0, from); trace.SetPosition(1, to); trace.enabled = true; remaining = .12f;
        }
        private void Update() { if (trace != null && (remaining -= Time.deltaTime) <= 0) trace.enabled = false; }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
