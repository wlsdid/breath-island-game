using System.IO;
using UnityEditor;
using UnityEngine;

namespace BreathIsland.Editor
{
    public static class CoastalVisualAuthoring
    {
        private const string Folder = "Assets/BreathIsland/Resources/World/Coastal/";
        [MenuItem("숨결섬/해안과 낚시 표현 작성")]
        public static void Build()
        {
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            var blend=AssetDatabase.LoadAssetAtPath<Material>(Folder+"Blend.mat");
            if(blend==null){blend=new Material(Shader.Find("BreathIsland/CoastalBlend"));AssetDatabase.CreateAsset(blend,Folder+"Blend.mat");}
            var wood = Material("Cork", new Color(.54f,.32f,.15f));
            var rodMat = Material("Rod", new Color(.075f,.19f,.21f));
            var metal = Material("Brass", new Color(.75f,.55f,.24f));
            var foam = Material("Foam", new Color(.67f,.91f,.87f), true);
            var lineMat = Material("Line", new Color(.9f,.94f,.83f), true);
            var coral = Material("Float", new Color(.91f,.29f,.17f));
            var hand = Material("Hand", new Color(.85f,.60f,.39f));
            var root = new GameObject("FishingRig");
            try
            {
                var rod = new GameObject("Rod",typeof(FishingRodView)); rod.transform.SetParent(root.transform,false);
                var view = rod.GetComponent<FishingRodView>();
                var shaft = new GameObject("Flexible shaft",typeof(MeshFilter),typeof(MeshRenderer)); shaft.transform.SetParent(rod.transform,false);
                shaft.GetComponent<MeshRenderer>().sharedMaterial=rodMat; view.shaft=shaft.GetComponent<MeshFilter>();
                Part(rod.transform,"Cork grip",PrimitiveType.Cylinder,new Vector3(0,0,-.12f),new Vector3(.075f,.25f,.075f),Quaternion.Euler(90,0,0),wood);
                Part(rod.transform,"Reel seat",PrimitiveType.Cube,new Vector3(0,-.075f,-.1f),new Vector3(.04f,.14f,.06f),Quaternion.identity,metal);
                Part(rod.transform,"Spool",PrimitiveType.Cylinder,new Vector3(0,-.16f,-.1f),new Vector3(.15f,.045f,.15f),Quaternion.Euler(0,0,90),metal);
                var crank=new GameObject("Reel crank");crank.transform.SetParent(rod.transform,false);crank.transform.localPosition=new Vector3(-.065f,-.16f,-.1f);
                Part(crank.transform,"Crank",PrimitiveType.Cube,new Vector3(0,0,-.08f),new Vector3(.025f,.025f,.16f),Quaternion.identity,metal);
                Part(crank.transform,"Crank grip",PrimitiveType.Sphere,new Vector3(-.025f,0,-.16f),new Vector3(.065f,.05f,.05f),Quaternion.identity,wood);
                view.reelHandle=crank.transform;
                Part(rod.transform,"Right mitten",PrimitiveType.Capsule,new Vector3(.025f,0,-.21f),new Vector3(.12f,.085f,.12f),Quaternion.Euler(0,0,80),hand);
                Part(crank.transform,"Left mitten",PrimitiveType.Sphere,new Vector3(-.07f,0,-.16f),new Vector3(.13f,.10f,.15f),Quaternion.identity,hand);
                view.guides=new Transform[6];
                for(int i=0;i<6;i++)
                {
                    var guide=Line(rod.transform,"Guide "+i,metal,17,.009f,false);
                    float r=Mathf.Lerp(.045f,.018f,i/5f);
                    for(int j=0;j<17;j++){float a=j*Mathf.PI/8;guide.SetPosition(j,new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0));}
                    view.guides[i]=guide.transform;
                }
                view.thread=Line(rod.transform,"Thread through guides",lineMat,7,.006f,true);
                Line(root.transform,"Line",lineMat,24,.012f,true);
                Line(root.transform,"Ripple",foam,49,.035f,true);
                var bobber=new GameObject("Bobber");bobber.transform.SetParent(root.transform,false);
                Part(bobber.transform,"Float body",PrimitiveType.Sphere,Vector3.zero,new Vector3(.12f,.20f,.12f),Quaternion.identity,coral);
                Part(bobber.transform,"Float tip",PrimitiveType.Cylinder,new Vector3(0,.13f,0),new Vector3(.025f,.10f,.025f),Quaternion.identity,lineMat);
                var fish=new GameObject("Fish",typeof(FishingCatchView));fish.transform.SetParent(root.transform,false);
                var catchView=fish.GetComponent<FishingCatchView>();
                catchView.fishIds=new[]{"glimmer","cloud","coral","moon","pearl","breaker"};
                string[] sources={"BI_Fish_Glimmer","BI_Fish_Cloud","BI_Fish_Coral","BI_Fish_MoonRay","BI_Fish_Pearl","BI_Predator_Breaker"};
                catchView.models=new GameObject[sources.Length];
                for(int i=0;i<sources.Length;i++)
                {
                    var model=Object.Instantiate(Resources.Load<GameObject>("Generated/"+sources[i]),fish.transform);
                    foreach(var collider in model.GetComponentsInChildren<Collider>())Object.DestroyImmediate(collider);
                    model.transform.localScale=Vector3.one*(i==5?.48f:.65f);
                    catchView.models[i]=model;model.SetActive(false);
                }
                PrefabUtility.SaveAsPrefabAsset(root,"Assets/BreathIsland/Resources/World/FishingRig.prefab");
            }
            finally {Object.DestroyImmediate(root);}
            AssetDatabase.SaveAssets();
        }
        private static Material Material(string name,Color color,bool unlit=false)
        {
            string path=Folder+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.color=color;material.SetColor("_BaseColor",color);
            if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.28f);
            EditorUtility.SetDirty(material);return material;
        }
        private static void Part(Transform parent,string name,PrimitiveType type,Vector3 pos,Vector3 size,Quaternion rot,Material mat)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);
            go.transform.localPosition=pos;go.transform.localScale=size;go.transform.localRotation=rot;
            Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;
        }
        private static LineRenderer Line(Transform parent,string name,Material material,int count,float width,bool world)
        {
            var go=new GameObject(name,typeof(LineRenderer));go.transform.SetParent(parent,false);
            var line=go.GetComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=count;line.widthMultiplier=width;line.useWorldSpace=world;
            line.numCapVertices=3;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return line;
        }
    }
}
