using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BreathIsland
{
    public sealed class WorldHandles
    {
        public GameObject player;
        public Camera camera;
        public IslandCameraRig cameraRig;
        public ThirdPersonMotor motor;
        public readonly Dictionary<NpcId, Transform> npcs = new();
        public Vector3 fishingPoint = new(0f, .2f, 24f);
    }

    public static class ProceduralWorld
    {
        private static readonly Dictionary<string, Material> Materials = new();

        public static WorldHandles Build(Transform parent)
        {
            ConfigureEnvironment();
            var handles = new WorldHandles();
            BuildLighting(parent);
            BuildWater(parent);
            BuildIsland(parent);
            BuildVillage(parent);
            BuildDock(parent);
            BuildDecor(parent);
            BuildSeaLife(parent);
            handles.player = BuildChildCharacter("Player", new Vector3(0f, 1.2f, 4f), new Color(.96f, .88f, .67f), new Color(.2f, .55f, .72f), parent);
            handles.motor = handles.player.AddComponent<ThirdPersonMotor>();
            var cameraObject = new GameObject("Gameplay Camera", typeof(Camera), typeof(AudioListener), typeof(IslandCameraRig));
            cameraObject.transform.SetParent(parent);
            handles.camera = cameraObject.GetComponent<Camera>();
            handles.camera.fieldOfView = 55f;
            handles.camera.nearClipPlane = .08f;
            handles.camera.farClipPlane = 230f;
            handles.camera.backgroundColor = new Color(.58f, .84f, .94f);
            handles.cameraRig = cameraObject.GetComponent<IslandCameraRig>();
            handles.cameraRig.target = handles.player.transform;
            handles.motor.SetCamera(cameraObject.transform);
            handles.npcs[NpcId.Nari] = BuildChildCharacter("Nari", new Vector3(-7f, 1.2f, 9f), new Color(.92f, .72f, .48f), new Color(.14f, .53f, .52f), parent).transform;
            handles.npcs[NpcId.Maru] = BuildChildCharacter("Maru", new Vector3(8f, 1.2f, 6f), new Color(.78f, .9f, .7f), new Color(.94f, .63f, .35f), parent).transform;
            return handles;
        }

        private static void ConfigureEnvironment()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.55f, .77f, .83f);
            RenderSettings.ambientEquatorColor = new Color(.42f, .62f, .62f);
            RenderSettings.ambientGroundColor = new Color(.2f, .35f, .34f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.63f, .83f, .85f);
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 210f;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }

        private static void BuildLighting(Transform parent)
        {
            var sun = new GameObject("Sun", typeof(Light)); sun.transform.SetParent(parent);
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            var light = sun.GetComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.15f; light.color = new Color(1f, .89f, .7f); light.shadows = LightShadows.Soft;
        }

        private static void BuildWater(Transform parent)
        {
            var water = Primitive("Calm Sea", PrimitiveType.Plane, new Vector3(0f, -.7f, 32f), new Vector3(22f, 1f, 22f), new Color(.07f, .58f, .72f), parent);
            Object.Destroy(water.GetComponent<Collider>());
        }

        private static void BuildIsland(Transform parent)
        {
            Primitive("Island Rock", PrimitiveType.Cylinder, new Vector3(0f, -.65f, 0f), new Vector3(18f, 1.1f, 16f), new Color(.52f, .48f, .35f), parent);
            Primitive("Island Grass", PrimitiveType.Cylinder, new Vector3(0f, .05f, 0f), new Vector3(17.3f, .55f, 15.3f), new Color(.38f, .68f, .36f), parent);
            Primitive("Beach", PrimitiveType.Cylinder, new Vector3(0f, .45f, 11f), new Vector3(12f, .35f, 8f), new Color(.9f, .78f, .52f), parent);
        }

        private static void BuildVillage(Transform parent)
        {
            BuildHouse("Boat Workshop", new Vector3(-8f, 1.25f, 5f), new Color(.45f, .25f, .13f), new Color(.18f, .46f, .58f), parent);
            BuildHouse("Aquarium", new Vector3(8f, 1.25f, 2f), new Color(.91f, .82f, .63f), new Color(.22f, .7f, .7f), parent);
            BuildHouse("Village House A", new Vector3(-10f, 1.1f, -4f), new Color(.93f, .86f, .7f), new Color(.86f, .45f, .27f), parent);
            BuildHouse("Village House B", new Vector3(5f, 1.1f, -7f), new Color(.9f, .82f, .62f), new Color(.72f, .36f, .25f), parent);
        }

        private static void BuildHouse(string name, Vector3 position, Color wall, Color roof, Transform parent)
        {
            var root = new GameObject(name); root.transform.SetParent(parent); root.transform.position = position;
            Primitive("Walls", PrimitiveType.Cube, Vector3.up * 1.6f, new Vector3(5.2f, 3.2f, 4.2f), wall, root.transform, true);
            var roofObject = Primitive("Roof", PrimitiveType.Cube, new Vector3(0f, 3.65f, 0f), new Vector3(5.8f, .55f, 4.8f), roof, root.transform, true);
            roofObject.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);
            Primitive("Door", PrimitiveType.Cube, new Vector3(0f, 1f, -2.15f), new Vector3(1.15f, 2f, .15f), new Color(.32f, .18f, .1f), root.transform, true);
            Primitive("Window", PrimitiveType.Cube, new Vector3(1.55f, 1.9f, -2.16f), new Vector3(1.1f, .85f, .12f), new Color(.47f, .85f, .91f), root.transform, true);
        }

        private static void BuildDock(Transform parent)
        {
            for (int i = 0; i < 7; i++) Primitive($"Dock Plank {i}", PrimitiveType.Cube, new Vector3(0f, .75f, 12f + i * 2f), new Vector3(4.5f, .35f, 1.85f), new Color(.48f, .3f, .16f), parent);
            for (int i = 0; i < 4; i++)
            {
                float z = 13f + i * 4f;
                Primitive("Dock Post L", PrimitiveType.Cylinder, new Vector3(-2.05f, -.05f, z), new Vector3(.28f, 1.45f, .28f), new Color(.32f, .2f, .1f), parent);
                Primitive("Dock Post R", PrimitiveType.Cylinder, new Vector3(2.05f, -.05f, z), new Vector3(.28f, 1.45f, .28f), new Color(.32f, .2f, .1f), parent);
            }
        }

        private static void BuildDecor(Transform parent)
        {
            var random = new System.Random(2409);
            for (int i = 0; i < 22; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float radius = 8f + (float)random.NextDouble() * 7f;
                Vector3 p = new(Mathf.Cos(angle) * radius, .75f, Mathf.Sin(angle) * radius - 1f);
                if (p.z > 7f && Mathf.Abs(p.x) < 4f) continue;
                var trunk = Primitive("Tree Trunk", PrimitiveType.Cylinder, p, new Vector3(.35f, 1.5f, .35f), new Color(.37f, .23f, .12f), parent);
                Primitive("Tree Crown", PrimitiveType.Sphere, p + Vector3.up * 2.1f, new Vector3(1.7f, 1.35f, 1.7f), new Color(.22f + (i % 3) * .04f, .57f, .27f), trunk.transform);
            }
            for (int i = 0; i < 12; i++)
            {
                Vector3 p = new(-13f + i * 2.1f, .75f, 8f + Mathf.Sin(i) * 1.4f);
                Primitive("Wildflower", PrimitiveType.Sphere, p, Vector3.one * .18f, i % 2 == 0 ? new Color(1f, .86f, .52f) : new Color(1f, .74f, .78f), parent);
            }
        }

        private static GameObject BuildChildCharacter(string name, Vector3 position, Color clothes, Color cape, Transform parent)
        {
            var root = new GameObject(name, typeof(CharacterController)); root.transform.SetParent(parent); root.transform.position = position;
            var controller = root.GetComponent<CharacterController>(); controller.height = 1.65f; controller.radius = .42f; controller.center = Vector3.up * .82f;
            if (name == "Player")
            {
                GameObject source = Resources.Load<GameObject>("Generated/BI_Hero_Child");
                if (source != null)
                {
                    GameObject visual = Object.Instantiate(source, root.transform);
                    visual.name = "Hero Visual";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    visual.transform.localScale = Vector3.one;
                    foreach (Collider childCollider in visual.GetComponentsInChildren<Collider>()) Object.Destroy(childCollider);
                    return root;
                }
            }
            Primitive("Body", PrimitiveType.Capsule, new Vector3(0f, .74f, 0f), new Vector3(.68f, .68f, .68f), clothes, root.transform, true);
            Primitive("Head", PrimitiveType.Sphere, new Vector3(0f, 1.55f, 0f), new Vector3(.82f, .78f, .82f), new Color(.92f, .7f, .51f), root.transform, true);
            Primitive("Hair", PrimitiveType.Sphere, new Vector3(0f, 1.73f, .03f), new Vector3(.86f, .46f, .86f), new Color(.26f, .14f, .08f), root.transform, true);
            Primitive("Cape", PrimitiveType.Cube, new Vector3(0f, .9f, .31f), new Vector3(.78f, 1.05f, .12f), cape, root.transform, true);
            return root;
        }

        private static void BuildSeaLife(Transform parent)
        {
            string[] names = { "BI_Fish_Glimmer", "BI_Fish_Cloud", "BI_Fish_Coral", "BI_Fish_MoonRay", "BI_Fish_Pearl" };
            for (int i = 0; i < names.Length; i++)
            {
                GameObject source = Resources.Load<GameObject>($"Generated/{names[i]}");
                if (source == null) continue;
                GameObject creature = Object.Instantiate(source, parent);
                creature.name = $"Ambient {names[i]}";
                creature.transform.position = new Vector3(-5f + i * 2.8f, -1.45f - (i % 2) * .25f, 27f + (i % 3) * 2.5f);
                creature.transform.localScale = Vector3.one * (1f + i * .06f);
                creature.AddComponent<AmbientFishMotion>().phase = i * 1.13f;
                foreach (Collider collider in creature.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
            }
        }

        private static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent, bool local = false)
        {
            var gameObject = GameObject.CreatePrimitive(type); gameObject.name = name; gameObject.transform.SetParent(parent);
            if (local) gameObject.transform.localPosition = position; else gameObject.transform.position = position;
            gameObject.transform.localScale = scale; gameObject.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
            return gameObject;
        }

        private static Material MaterialFor(Color color)
        {
            string key = ColorUtility.ToHtmlStringRGBA(color);
            if (Materials.TryGetValue(key, out var cached)) return cached;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color, name = $"Procedural {key}" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            material.enableInstancing = true; Materials[key] = material; return material;
        }
    }

    public sealed class AmbientFishMotion : MonoBehaviour
    {
        public float phase;
        private Vector3 origin;
        private void Start() => origin = transform.position;
        private void Update()
        {
            float time = Time.time * .45f + phase;
            transform.position = origin + new Vector3(Mathf.Sin(time) * 2.2f, Mathf.Sin(time * 1.7f) * .16f, Mathf.Cos(time) * .75f);
            transform.rotation = Quaternion.Euler(0f, -Mathf.Cos(time) * 45f, 0f);
        }
    }
}
