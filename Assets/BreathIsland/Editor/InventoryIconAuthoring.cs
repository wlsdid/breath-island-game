using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace BreathIsland.Editor
{
    public static class InventoryIconAuthoring
    {
        private const string Output="Assets/BreathIsland/Resources/UI/ItemIcons/";
        public static void Build()
        {
            Directory.CreateDirectory(Output);AssetDatabase.Refresh();
            var catalog=IslandLifeCatalog.Load();
            Render("Axe",catalog.axeModel,catalog.toolMaterial,true);Render("Pickaxe",catalog.pickaxeModel,catalog.toolMaterial,true);Render("Hammer",catalog.hammerModel,catalog.toolMaterial,true);
            foreach(var weapon in catalog.weapons)Render(weapon.id,weapon.model,catalog.weaponMaterial,true);
            foreach(var furniture in catalog.furniture)Render(furniture.id,furniture.model,furniture.material!=null?furniture.material:catalog.furnitureMaterial);
            const string art="Assets/BreathIsland/Art/ThirdParty/KayKit/Resources/";
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(art+"resource_bits_texture.png"));
            Render("wood",AssetDatabase.LoadAssetAtPath<GameObject>(art+"Wood_Log_A.fbx"),material);
            Render("stone",AssetDatabase.LoadAssetAtPath<GameObject>(art+"Stone_Chunks_Small.fbx"),material);Object.DestroyImmediate(material);
            Render("herbs",catalog.nodes[System.Array.FindIndex(catalog.nodes,n=>n.id=="herb_a")].model,catalog.natureMaterial);
            AssetDatabase.Refresh();
            foreach(var path in Directory.GetFiles(Output,"*.png"))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=256;importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
        private static Bounds BoundsOf(GameObject model)
        {
            var bounds=new Bounds();bool first=true;
            foreach(var r in model.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
            return bounds;
        }
        private static void Render(string id,GameObject source,Material material,bool upright=false)
        {
            var root=new GameObject("Icon studio");root.transform.position=new Vector3(0,-100,0);
            var model=ModelPresentation.Create(source,root.transform,material,1);
            foreach(var t in model.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
            var bounds=BoundsOf(model);
            if(upright)
            {
                if(bounds.size.x>bounds.size.y && bounds.size.x>bounds.size.z)model.transform.localRotation=Quaternion.Euler(0,0,90);
                else if(bounds.size.z>bounds.size.y)model.transform.localRotation=Quaternion.Euler(90,0,0);
                bounds=BoundsOf(model);
                if(bounds.size.z>bounds.size.x)model.transform.rotation=Quaternion.Euler(0,90,0)*model.transform.rotation;
                bounds=BoundsOf(model);
            }
            var cameraObject=new GameObject("Icon camera",typeof(Camera));cameraObject.transform.SetParent(root.transform);
            var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z)*(upright?.56f:.62f);
            camera.cullingMask=1<<30;camera.nearClipPlane=.01f;camera.farClipPlane=30;
            camera.transform.position=bounds.center+(upright?new Vector3(.9f,.8f,6):new Vector3(3,2.2f,4));camera.transform.LookAt(bounds.center);
            var lightObject=new GameObject("Icon key",typeof(Light));lightObject.transform.SetParent(root.transform);
            var light=lightObject.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.6f;light.color=new Color(1,.95f,.85f);light.cullingMask=1<<30;light.transform.rotation=Quaternion.Euler(35,-25,0);
            var mode=RenderSettings.ambientMode;var ambient=RenderSettings.ambientLight;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.7f,.74f);
            var target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;
            var previous=RenderTexture.active;
            try
            {
                camera.Render();RenderTexture.active=target;
                var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
                File.WriteAllBytes(Output+id+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
            }
            finally
            {
                RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);
                RenderSettings.ambientMode=mode;RenderSettings.ambientLight=ambient;Object.DestroyImmediate(root);
            }
        }
    }
}
