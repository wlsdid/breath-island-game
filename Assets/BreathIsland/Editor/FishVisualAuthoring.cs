using System.IO;
using UnityEditor;
using UnityEngine;

namespace BreathIsland.Editor
{
    public static class FishVisualAuthoring
    {
        public static void Build()
        {
            const string folder="Assets/BreathIsland/Resources/World/Fish/";
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            string[] names={"BI_Fish_Glimmer","BI_Fish_Cloud","BI_Fish_Coral","BI_Fish_MoonRay","BI_Fish_Pearl","BI_Predator_Breaker"};
            Color[] colors={new(.12f,.62f,.56f),new(.9f,.5f,.09f),new(.83f,.19f,.12f),new(.24f,.25f,.65f),new(.7f,.25f,.5f),new(.08f,.3f,.4f)};
            for(int i=0;i<names.Length;i++)
            {
                var root=new GameObject(names[i]);
                try
                {
                    var model=Object.Instantiate(Resources.Load<GameObject>("Generated/"+names[i]),root.transform);
                    // FBX conversion mirrors the source X axis; preserve imported up and turn the nose toward +Z.
                    model.transform.localRotation=Quaternion.Euler(0,-90,0)*model.transform.localRotation;
                    var swim=root.AddComponent<FishSwimView>();
                    var body=Make(folder+names[i]+".mat",colors[i]);
                    var belly=Make(folder+names[i]+"_Belly.mat",Color.Lerp(colors[i],new Color(.9f,.87f,.68f),.55f));
                    var eyes=Make(folder+"Eyes.mat",new Color(.015f,.025f,.03f));
                    foreach(var renderer in model.GetComponentsInChildren<Renderer>())
                    {
                        renderer.sharedMaterial=renderer.name.StartsWith("Eye") ? eyes : renderer.name=="Belly" ? belly : body;
                        if(renderer.name=="Tail")swim.tail=renderer.transform;
                        if(renderer.name=="DorsalFin")swim.fin=renderer.transform;
                    }
                    foreach(var collider in root.GetComponentsInChildren<Collider>())Object.DestroyImmediate(collider);
                    PrefabUtility.SaveAsPrefabAsset(root,folder+names[i]+".prefab");
                }
                finally{Object.DestroyImmediate(root);}
            }
            AssetDatabase.SaveAssets();CoastalVisualAuthoring.Build();
        }
        private static Material Make(string path,Color color)
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",.22f);mat.SetFloat("_Metallic",0);
            EditorUtility.SetDirty(mat);return mat;
        }
    }
}
