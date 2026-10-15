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
        public VillageLayout village;
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
            var coast = new GameObject("Coastal scenery"); coast.transform.SetParent(parent, false); coast.AddComponent<CoastalScenery>();
            handles.village = Object.Instantiate(Resources.Load<GameObject>("World/ForestVillage"), parent).GetComponent<VillageLayout>();
            BuildDock(parent);
            BuildSeaLife(parent);
            handles.player = BuildChildCharacter("Player", new Vector3(0f, 1.2f, 4f), new Color(.96f, .88f, .67f), new Color(.2f, .55f, .72f), parent);
            handles.motor = handles.player.AddComponent<ThirdPersonMotor>();
            handles.player.AddComponent<FirstPersonBodyView>();
            var cameraObject = new GameObject("Gameplay Camera", typeof(Camera), typeof(AudioListener), typeof(IslandCameraRig));
            cameraObject.transform.SetParent(parent);
            handles.camera = cameraObject.GetComponent<Camera>();
            var cameraData = cameraObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            cameraData.requiresDepthTexture = true;
            cameraData.requiresColorTexture = true;
            handles.camera.fieldOfView = 55f;
            handles.camera.nearClipPlane = .08f;
            handles.camera.farClipPlane = 230f;
            handles.camera.backgroundColor = new Color(.58f, .84f, .94f);
            handles.cameraRig = cameraObject.GetComponent<IslandCameraRig>();
            handles.cameraRig.target = handles.player.transform;
            handles.motor.SetCamera(cameraObject.transform);
            handles.npcs[NpcId.Nari] = BuildNpc("Nari", new Vector3(-7f, 1.2f, 9f), parent, handles.player.transform);
            handles.npcs[NpcId.Maru] = BuildNpc("Maru", new Vector3(8f, 1.2f, 6f), parent, handles.player.transform);
            return handles;
        }

        private static Transform BuildNpc(string name, Vector3 anchor, Transform parent, Transform player)
        {
            var prefab = Resources.Load<GameObject>("World/Npcs/" + name);
            // Keep older checkouts playable until the explicit editor authoring has run.
            if (prefab == null)
                return BuildChildCharacter(name, anchor,
                    name == "Nari" ? new Color(.92f,.72f,.48f) : new Color(.78f,.9f,.7f),
                    name == "Nari" ? new Color(.14f,.53f,.52f) : new Color(.94f,.63f,.35f), parent).transform;
            var instance = Object.Instantiate(prefab, parent);
            instance.name = name;
            instance.transform.position = anchor;
            instance.GetComponent<NpcPresentation>().Initialize(player);
            return instance.transform;
        }

        private static void ConfigureEnvironment()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.62f, .76f, .84f);
            RenderSettings.ambientEquatorColor = new Color(.44f, .53f, .51f);
            RenderSettings.ambientGroundColor = new Color(.24f, .28f, .23f);
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
            light.shadowStrength = .72f; light.shadowBias = .035f; light.shadowNormalBias = .18f;
        }

        private static void BuildWater(Transform parent)
        {
            var water = new GameObject("Living sea"); water.transform.SetParent(parent, false);
            water.AddComponent<OceanSurface>();
        }

        private static void BuildIsland(Transform parent)
        {
            Primitive("Island Rock", PrimitiveType.Cylinder, new Vector3(0f, -.65f, -5f), new Vector3(38f, 1.1f, 32f), new Color(.52f, .48f, .35f), parent);
            Primitive("Island Grass", PrimitiveType.Cylinder, new Vector3(0f, .05f, -5f), new Vector3(37f, .55f, 31f), new Color(.38f, .68f, .36f), parent);
            Primitive("Beach", PrimitiveType.Cylinder, new Vector3(0f, .45f, 11f), new Vector3(12f, .35f, 8f), new Color(.9f, .78f, .52f), parent);
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
                GameObject source = Resources.Load<GameObject>($"World/Fish/{names[i]}");
                if (source == null) continue;
                GameObject creature = Object.Instantiate(source, parent);
                creature.name = $"Ambient {names[i]}";
                creature.transform.position = new Vector3(-5f + i * 2.8f, -1.45f - (i % 2) * .25f, 27f + (i % 3) * 2.5f);
                creature.transform.localScale = Vector3.one * (.55f + i * .025f);
                creature.AddComponent<AmbientFishMotion>().phase = i * 1.13f;
                foreach (Collider collider in creature.GetComponentsInChildren<Collider>()) Object.Destroy(collider);
            }
        }

        private static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent, bool local = false)
        {
            var gameObject = GameObject.CreatePrimitive(type); gameObject.name = name; gameObject.transform.SetParent(parent);
            if (local) gameObject.transform.localPosition = position; else gameObject.transform.position = position;
            gameObject.transform.localScale = scale; gameObject.GetComponent<Renderer>().sharedMaterial = MaterialFor(color);
            // Unity cylinders use a capsule collider, which becomes a tall dome when flattened.
            // Static terrain must collide with the visible cylinder mesh instead.
            if (name == "Island Rock" || name == "Island Grass" || name == "Beach")
            {
                var original = gameObject.GetComponent<Collider>();
                original.enabled = false;
                Object.Destroy(original);
                gameObject.AddComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            }
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
            var position = origin + new Vector3(Mathf.Sin(time) * 3.2f, 0, Mathf.Cos(time) * 1.3f);
            position.y = OceanSurface.Height(position,Time.time) - .85f - .15f * Mathf.Sin(time*1.7f);
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(new Vector3(Mathf.Cos(time)*3.2f,0,-Mathf.Sin(time)*1.3f));
        }
    }
}
