using UnityEngine;
namespace BreathIsland
{
    // String, arrow nock and flight follow the bow's draw/release timeline; never apply damage.
    [DefaultExecutionOrder(120)]
    public sealed class BowShotView : MonoBehaviour
    {
        private WeaponActionView action;
        private Transform owner;
        private LineRenderer bowstring;
        private Material stringMaterial;
        private GameObject arrow;
        private Vector3 center, target, release;
        private float halfHeight, clock = 2;
        private bool skill;
        private Color color;
        private WeaponSkillView impact;
        public bool Released => clock >= .30f;
        public void Initialize(Transform player, WeaponActionView motion)
        {
            owner = player; action = motion;
            var meshes = GetComponentsInChildren<Renderer>(); var bounds = ModelPresentation.WorldVisibleBounds(meshes[0]);
            foreach (var mesh in meshes) bounds.Encapsulate(ModelPresentation.WorldVisibleBounds(mesh));
            center = transform.InverseTransformPoint(bounds.center);
            halfHeight = bounds.size.y * .46f;
            bowstring = gameObject.AddComponent<LineRenderer>(); bowstring.useWorldSpace = true; bowstring.positionCount = 3;
            bowstring.startWidth = bowstring.endWidth = .008f;
            stringMaterial = new Material(Resources.Load<Material>("World/Coastal/Line")); bowstring.sharedMaterial = stringMaterial;
            impact = gameObject.AddComponent<WeaponSkillView>();
        }
        public void Play(Vector3 aim, bool special, Color tint)
        {
            target = aim; skill = special; color = tint; clock = 0;
            if (arrow == null) arrow = Instantiate(Resources.Load<GameObject>("World/Weapons/Arrow"));
            arrow.SetActive(true);
        }
        private void LateUpdate()
        {
            if (owner == null) return;
            clock += Time.deltaTime;
            Vector3 mid = transform.TransformPoint(center) - owner.forward * .06f;
            bowstring.SetPosition(0, mid + owner.up * halfHeight);
            bowstring.SetPosition(1, mid - owner.forward * (.4f * action.DrawFraction));
            bowstring.SetPosition(2, mid - owner.up * halfHeight);
            if (arrow == null || !arrow.activeSelf) return;
            if (clock < .30f) { release = mid; arrow.transform.position = bowstring.GetPosition(1); }
            else arrow.transform.position = Vector3.Lerp(release, target, (clock - .30f) / .12f);
            arrow.transform.rotation = Quaternion.FromToRotation(Vector3.up, (target - release).normalized);
            if (clock >= .42f) { arrow.SetActive(false); if (skill) impact.Play("ranger", target, color); }
        }
        private void OnDisable() { if (arrow != null) arrow.SetActive(false); }
        private void OnDestroy() { if (arrow != null) Destroy(arrow); if (stringMaterial != null) Destroy(stringMaterial); }
    }
}
