using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    public sealed class HousingView : System.IDisposable
    {
        private readonly HousingSystem housing;
        private readonly IslandLifeCatalog catalog;
        private readonly Transform root;
        private readonly List<GameObject> objects = new();
        private GameObject preview;
        public GameObject Preview => preview;
        public HousingView(HousingSystem housing, IslandLifeCatalog catalog, Transform parent)
        {
            this.housing = housing; this.catalog = catalog;
            root = new GameObject("Furniture Layout").transform; root.SetParent(parent, false); root.position = HousingSystem.InteriorOrigin;
            housing.Changed += Refresh; Refresh();
        }
        private void Refresh()
        {
            foreach (var obj in objects) { obj.SetActive(false); Object.Destroy(obj); }
            objects.Clear(); preview = null;
            foreach (var item in housing.State.placed)
            {
                var definition = catalog.Furniture(item.furnitureId);
                if (definition != null) Add(definition, item.x, item.z, item.turns, false);
            }
            if (housing.Selection != null) preview = Add(housing.Selection, housing.X, housing.Z, housing.Turns, true);
        }
        private GameObject Add(FurnitureDefinition definition, int x, int z, int turns, bool ghost)
        {
            var obj = ModelPresentation.Create(definition.model, root, catalog.furnitureMaterial, definition.height);
            var size = FurniturePlacement.Size(definition, turns);
            obj.transform.localPosition = new Vector3(x + size.x / 2f, 0, z + size.y / 2f);
            obj.transform.localRotation = Quaternion.Euler(0, turns * 90, 0);
            // Uniformly contain the mesh inside its reserved grid footprint.
            var bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (var r in obj.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            float fit = Mathf.Min(1, (size.x - .1f) / Mathf.Max(.01f, bounds.size.x), (size.y - .1f) / Mathf.Max(.01f, bounds.size.z));
            obj.transform.localScale *= fit;
            if (ghost)
            {
                var block = new MaterialPropertyBlock(); block.SetColor("_BaseColor", housing.IsValid ? new Color(.35f, 1, .65f) : new Color(1, .25f, .25f));
                foreach (var r in obj.GetComponentsInChildren<Renderer>()) r.SetPropertyBlock(block);
            }
            else { var box = obj.AddComponent<BoxCollider>(); box.center = Vector3.up * definition.height / 2; box.size = new Vector3(definition.footprint.x / fit, definition.height, definition.footprint.y / fit) * .9f; }
            objects.Add(obj); return obj;
        }
        public void Dispose() { housing.Changed -= Refresh; if (root != null) Object.Destroy(root.gameObject); }
    }
}
