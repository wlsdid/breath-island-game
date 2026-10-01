using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class StorageChestAuthoring
    {
        public static void Build()
        {
            const string source="Assets/BreathIsland/Art/ThirdParty/KayKit/Storage/";
            const string output="Assets/BreathIsland/Resources/World/StorageChest";
            var material=AssetDatabase.LoadAssetAtPath<Material>(output+".mat");
            if(material==null)
            {
                material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor",new Color(.43f,.26f,.13f));material.SetFloat("_Smoothness",.12f);
                AssetDatabase.CreateAsset(material,output+".mat");
            }
            var root=new GameObject("Storage Chest");
            var body=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source+"chest_common_empty.fbx"),root.transform);
            var lid=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source+"chestTop_common.fbx"),root.transform);
            var a=Bounds(body);var b=Bounds(lid);
            lid.transform.position+=new Vector3(a.center.x-b.center.x,a.max.y-b.min.y-.015f,a.center.z-b.center.z);
            foreach(var r in root.GetComponentsInChildren<Renderer>())
            {var slots=r.sharedMaterials;for(int i=0;i<slots.Length;i++)slots[i]=material;r.sharedMaterials=slots;}
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,output+".prefab");Object.DestroyImmediate(root);
            var definition=IslandLifeCatalog.Load().Furniture("chest");definition.model=prefab;definition.material=material;EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
        }
        private static Bounds Bounds(GameObject model)
        {var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);return bounds;}
    }
}
