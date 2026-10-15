using UnityEngine;
namespace BreathIsland
{
    // Fits imported meshes without modifying the source asset, and uses authored URP materials.
    public static class ModelPresentation
    {
        public static GameObject Create(GameObject source, Transform parent, Material material, float height, bool heldItem = false)
        {
            var root = new GameObject(source.name + " View");
            root.transform.SetParent(parent, false);
            var model = Object.Instantiate(source, root.transform);
            model.transform.localPosition = Vector3.zero;
            // FBX roots carry the authored Z-up to Unity Y-up correction.
            model.transform.localRotation = source.transform.localRotation;
            model.transform.localScale = Vector3.one;
            foreach (var collider in model.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
            var renderers = model.GetComponentsInChildren<Renderer>();
            var bounds = new Bounds();
            bool first = true;
            foreach (var renderer in renderers)
            {
                if (material != null)
                {
                    var slots = renderer.sharedMaterials;
                    for (int i = 0; i < slots.Length; i++) slots[i] = material;
                    renderer.sharedMaterials = slots;
                }
                // Skinned renderer bounds cover every imported animation pose and can extend
                // below the soles. Fit the visible geometry instead of its culling envelope.
                var localBounds = VisibleBounds(renderer);
                Matrix4x4 matrix = root.transform.worldToLocalMatrix * renderer.localToWorldMatrix;
                for (int corner = 0; corner < 8; corner++)
                {
                    var offset = new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1);
                    Vector3 point = matrix.MultiplyPoint3x4(localBounds.center + Vector3.Scale(localBounds.extents, offset));
                    if (first) { bounds = new Bounds(point, Vector3.zero); first = false; } else bounds.Encapsulate(point);
                }
            }
            if (!first && (heldItem ? Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) : bounds.size.y) > .000001f)
            {
                float extent = heldItem ? Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) : bounds.size.y;
                float scale = height / extent;
                Vector3 localCenter = bounds.center;
                model.transform.localScale *= scale;
                model.transform.localPosition = heldItem ? Vector3.zero : new Vector3(-localCenter.x, -localCenter.y + bounds.size.y / 2, -localCenter.z) * scale;
            }
            return root;
        }
        public static Bounds WorldVisibleBounds(Renderer renderer)
        {
            var local = VisibleBounds(renderer);
            var world = new Bounds(renderer.transform.TransformPoint(local.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
                world.Encapsulate(renderer.transform.TransformPoint(local.center + Vector3.Scale(local.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
            return world;
        }
        private static Bounds VisibleBounds(Renderer renderer)
        {
            if (renderer is not SkinnedMeshRenderer skin) return renderer.localBounds;
            var mesh = new Mesh();
            skin.BakeMesh(mesh);
            mesh.RecalculateBounds();
            var bounds = mesh.bounds;
            if (Application.isPlaying) Object.Destroy(mesh); else Object.DestroyImmediate(mesh);
            return bounds;
        }
    }
}
