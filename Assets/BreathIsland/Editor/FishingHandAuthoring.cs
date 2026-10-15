using UnityEditor;
using UnityEngine;

namespace BreathIsland.Editor
{
    internal static class FishingHandAuthoring
    {
        public static Transform Build(Transform parent,string name,Vector3 position,Material skin,Material sleeve)
        {
            var root=new GameObject(name);root.transform.SetParent(parent,false);root.transform.localPosition=position;
            Part(root.transform,"Palm",new Vector3(.025f,0,0),new Vector3(.115f,.13f,.085f),skin);
            Part(root.transform,"Thumb",new Vector3(-.025f,.045f,.032f),new Vector3(.065f,.075f,.065f),skin);
            for(int i=0;i<4;i++)Part(root.transform,"Curled finger "+i,new Vector3(-.038f,-.05f+i*.032f,-.015f),new Vector3(.075f,.026f,.09f),skin);
            Part(root.transform,"Cuff",new Vector3(.045f,-.07f,-.04f),new Vector3(.14f,.08f,.13f),sleeve);
            Part(root.transform,"Sleeve",new Vector3(.065f,-.13f,-.12f),new Vector3(.14f,.15f,.22f),sleeve);
            return root.transform;
        }
        private static void Part(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var part=GameObject.CreatePrimitive(PrimitiveType.Sphere);part.name=name;part.transform.SetParent(parent,false);
            part.transform.localPosition=position;part.transform.localScale=scale;
            Object.DestroyImmediate(part.GetComponent<Collider>());part.GetComponent<Renderer>().sharedMaterial=material;
        }
    }
}
