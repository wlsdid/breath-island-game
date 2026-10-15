using UnityEditor;
using UnityEngine;

namespace BreathIsland.Editor
{
    public static class OceanAuthoring
    {
        public static void Build()
        {
            const string folder="Assets/BreathIsland/Resources/World/Coastal/";
            var water=AssetDatabase.LoadAssetAtPath<Material>(folder+"Ocean.mat");
            if(water==null){water=new Material(Shader.Find("BreathIsland/OceanWater"));AssetDatabase.CreateAsset(water,folder+"Ocean.mat");}
            var seabed=AssetDatabase.LoadAssetAtPath<Material>(folder+"Seabed.mat");
            if(seabed==null){seabed=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(seabed,folder+"Seabed.mat");}
            seabed.SetColor("_BaseColor",new Color(.35f,.48f,.40f));seabed.SetFloat("_Smoothness",0);
            EditorUtility.SetDirty(water);EditorUtility.SetDirty(seabed);AssetDatabase.SaveAssets();
        }
    }
}
