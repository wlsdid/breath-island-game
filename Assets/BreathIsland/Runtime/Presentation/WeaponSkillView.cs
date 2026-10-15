using UnityEngine;
namespace BreathIsland
{
    // Visual-only, target-local skill marks. Damage remains in TrainingCombatSystem.
    public sealed class WeaponSkillView : MonoBehaviour
    {
        private LineRenderer mark;
        private Material material;
        private float clock = 2;
        private string job;
        private Vector3 center, right, forward;
        public bool Active => mark != null && mark.enabled;
        public void Play(string kind, Vector3 target, Color color)
        {
            if (mark == null)
            {
                var go = new GameObject("Skill mark");
                mark = go.AddComponent<LineRenderer>(); mark.useWorldSpace = true; mark.positionCount = 40;
                material = new Material(Resources.Load<Material>("World/Coastal/Line")); mark.sharedMaterial = material;
            }
            job = kind; center = target; forward = (target - transform.position).normalized;
            right = Vector3.Cross(Vector3.up, forward).normalized;
            material.color = color; clock = 0; mark.enabled = true;
        }
        private void Update()
        {
            if (mark == null) return;
            clock += Time.deltaTime; mark.enabled = clock < .65f;
            if (!mark.enabled) return;
            float radius = Mathf.Lerp(.18f, job == "warrior" ? 1.1f : .85f, clock / .65f);
            mark.startWidth = mark.endWidth = Mathf.Lerp(.09f, .005f, clock / .65f);
            for (int i = 0; i < mark.positionCount; i++)
            {
                float a = (float)i / (mark.positionCount - 1) * (job == "warrior" ? Mathf.PI * 1.35f : Mathf.PI * 2);
                mark.SetPosition(i, center + radius * (right * Mathf.Cos(a) + (job == "warrior" ? Vector3.up : forward) * Mathf.Sin(a)));
            }
        }
        private void OnDisable() { if (mark != null) mark.enabled = false; clock = 2; }
        private void OnDestroy() { if (mark != null) Destroy(mark.gameObject); if (material != null) Destroy(material); }
    }
}
