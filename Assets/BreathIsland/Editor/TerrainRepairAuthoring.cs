using System.Linq;
using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class TerrainRepairAuthoring
    {
        public static void Build()
        {
            var root=new GameObject("GroundCover");
            try
            {
                for(int i=0;i<100;i++)
                {
                    float a=i*2.399963f,r=.78f+(i%7)*.023f;
                    var p=new Vector3(Mathf.Cos(a)*18*r,.595f,-5+Mathf.Sin(a)*15*r);
                    if(p.z>6&&Mathf.Abs(p.x)<8)continue;
                    var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BreathIsland/Art/ThirdParty/KayKit/Nature/Grass_1_"+(i%2==0?"A":"B")+"_Color1.fbx");
                    var tuft=ModelPresentation.Create(model,root.transform,IslandLifeCatalog.Load().natureMaterial,.16f+(i%3)*.035f);
                    tuft.transform.localPosition=p;tuft.transform.localRotation=Quaternion.Euler(0,i*137.5f,0);
                }
                PrefabUtility.SaveAsPrefabAsset(root,"Assets/BreathIsland/Resources/World/GroundCover.prefab");
            }
            finally{Object.DestroyImmediate(root);}
            Material("GrassGround",new Color(.28f,.46f,.19f),new Color(.43f,.37f,.22f));
            Material("SandGround",new Color(.76f,.64f,.40f),new Color(.9f,.79f,.55f));
            Material("RockGround",new Color(.37f,.36f,.27f),new Color(.52f,.47f,.34f));
            AssetDatabase.SaveAssets();
        }
        private static void Material(string name,Color color,Color accent)
        {
            string path="Assets/BreathIsland/Resources/World/Coastal/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("BreathIsland/Ground"));AssetDatabase.CreateAsset(mat,path);}
            mat.SetColor("_BaseColor",color);mat.SetColor("_Accent",accent);EditorUtility.SetDirty(mat);
        }
    }
}
