using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    public sealed class IslandLifeWorld : System.IDisposable
    {
        private readonly Transform root;
        private readonly Material groundMaterial;
        private readonly Dictionary<string, GameObject> nodes = new();
        public Transform TrainingTarget { get; }
        public IslandLifeWorld(Transform parent, IslandLifeCatalog catalog)
        {
            root = new GameObject("Island Life World").transform; root.SetParent(parent, false);
            groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.63f, .49f, .32f) };
            foreach (var node in catalog.nodes)
            {
                var obj = ModelPresentation.Create(node.model, root, catalog.natureMaterial, node.height);
                obj.name = node.id; obj.transform.position = node.position;
                if (node.tool != LifeTool.None) { var collider = obj.AddComponent<CapsuleCollider>(); collider.radius = .35f; collider.height = node.height; collider.center = Vector3.up * node.height / 2; }
                nodes.Add(node.id, obj);
            }
            var dummy = Primitive("Training Target", new Vector3(0, 1.4f, -1), new Vector3(.7f, 1.5f, .7f));
            TrainingTarget = dummy.transform;
            HomeInteriorView.Create(root);
        }
        private GameObject Primitive(string name, Vector3 position, Vector3 size)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name;
            obj.transform.SetParent(root, false); obj.transform.position = position; obj.transform.localScale = size;
            obj.GetComponent<Renderer>().sharedMaterial = groundMaterial; return obj;
        }
        public void RefreshNodes(LandActivitySystem activities)
        {
            foreach (var pair in nodes) pair.Value.SetActive(!activities.Depleted(pair.Key));
        }
        public void Dispose() { root.gameObject.SetActive(false); Object.Destroy(root.gameObject); Object.Destroy(groundMaterial); }
    }
}
