using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class TrainingPresentationAuthoring
    {
        const string Art = "Assets/BreathIsland/Art/ThirdParty/KayKit/Character/";
        const string Folder = "Assets/BreathIsland/Resources/World/JobAppearances/";
        public static void Build()
        {
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            Outfit("warrior", "Knight", "knight_texture"); Outfit("mage", "Mage", "mage_texture");
            Dummy();
            string arrowPath = "Assets/BreathIsland/Resources/World/Weapons/Arrow.prefab";
            var arrow = PrefabUtility.LoadPrefabContents(arrowPath);
            try { arrow.transform.GetChild(0).localRotation = Quaternion.identity; PrefabUtility.SaveAsPrefabAsset(arrow, arrowPath); }
            finally { PrefabUtility.UnloadPrefabContents(arrow); }
            AssetDatabase.SaveAssets();
        }
        static void Outfit(string id, string model, string texture)
        {
            var root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art + model + ".fbx"));
            try
            {
                var material = Material(id, Color.white); material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + texture + ".png"); EditorUtility.SetDirty(material);
                var ranger = AssetDatabase.LoadAssetAtPath<GameObject>(Art + "Ranger.fbx");
                var names = ranger.GetComponentsInChildren<Transform>().Select(t => t.name).ToHashSet();
                foreach (var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (skin.bones.Any(b => b == null || !names.Contains(b.name))) throw new System.InvalidOperationException(model + " incompatible bones");
                    skin.sharedMaterials = skin.sharedMaterials.Select(_ => material).ToArray();
                }
                PrefabUtility.SaveAsPrefabAsset(root, Folder + id + ".prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }
        static Material Material(string name, Color color)
        {
            string path = Folder + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            material.color = color; EditorUtility.SetDirty(material); return material;
        }
        static void Dummy()
        {
            var root = new GameObject("Training Scarecrow");
            var wood = Material("DummyWood", new Color(.28f,.14f,.07f));
            var straw = Material("DummyStraw", new Color(.76f,.54f,.22f));
            var cloth = Material("DummyCloth", new Color(.22f,.4f,.43f));
            var target = Material("DummyTarget", new Color(.75f,.25f,.14f));
            void Part(string name, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
            {
                var part = GameObject.CreatePrimitive(shape); part.name = name; part.transform.SetParent(root.transform,false);
                part.transform.localPosition=position; part.transform.localScale=scale; part.GetComponent<Renderer>().sharedMaterial=material;
                Object.DestroyImmediate(part.GetComponent<Collider>());
            }
            try
            {
                Part("Post",PrimitiveType.Cylinder,new Vector3(0,-.25f,0),new Vector3(.12f,.5f,.12f),wood);
                Part("Straw torso",PrimitiveType.Capsule,Vector3.zero,new Vector3(.55f,.32f,.32f),cloth);
                Part("Crossbar",PrimitiveType.Cube,new Vector3(0,.12f,0),new Vector3(1.3f,.11f,.12f),wood);
                Part("Head",PrimitiveType.Sphere,new Vector3(0,.48f,0),new Vector3(.36f,.39f,.32f),straw);
                Part("Hat brim",PrimitiveType.Cylinder,new Vector3(0,.66f,0),new Vector3(.55f,.035f,.46f),wood);
                Part("Hat crown",PrimitiveType.Cylinder,new Vector3(0,.75f,0),new Vector3(.3f,.09f,.28f),straw);
                Part("Target patch",PrimitiveType.Sphere,new Vector3(0,0,-.17f),new Vector3(.27f,.27f,.035f),target);
                Part("Left eye",PrimitiveType.Sphere,new Vector3(-.08f,.5f,-.15f),Vector3.one*.04f,wood);
                Part("Right eye",PrimitiveType.Sphere,new Vector3(.08f,.5f,-.15f),Vector3.one*.04f,wood);
                var collider=root.AddComponent<BoxCollider>();collider.center=new Vector3(0,-.05f,0);collider.size=new Vector3(.65f,1.4f,.45f);
                PrefabUtility.SaveAsPrefabAsset(root,"Assets/BreathIsland/Resources/World/TrainingDummy.prefab");
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
