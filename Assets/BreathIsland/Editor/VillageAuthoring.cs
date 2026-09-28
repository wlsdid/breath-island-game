using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class VillageAuthoring
    {
        private const string Output = "Assets/BreathIsland/Resources/World/";
        public static void Build()
        {
            var root = new GameObject("Forest Village");
            var layout = root.AddComponent<VillageLayout>();
            layout.safePoints = new[] {new Vector3(0,1.2f,4),new Vector3(0,1.2f,8),new Vector3(-7,1.2f,9),new Vector3(8,1.2f,6),new Vector3(4,1.2f,5.5f),new Vector3(-7,1.2f,-8),new Vector3(3,1.2f,-6),new Vector3(0,1.2f,11)};
            var timber = ColorMaterial("VillageTimber", new Color(.32f,.18f,.11f));
            var stone = ColorMaterial("VillageStone", new Color(.54f,.53f,.46f));
            var roof = ColorMaterial("VillageRoof", new Color(.58f,.12f,.08f));
            var atlas = AssetDatabase.LoadAssetAtPath<Material>(Output + "Town.mat");
            House(root.transform,"Boat Workshop",new Vector3(-9,.6f,4),new Vector2(4,3.4f),timber,stone,roof,atlas);
            House(root.transform,"Aquarium",new Vector3(10,.6f,1.2f),new Vector2(3.4f,3.4f),stone,timber,roof,atlas);
            House(root.transform,"Village House A",new Vector3(-9,.6f,-5.5f),new Vector2(3.4f,3),timber,stone,roof,atlas);
            House(root.transform,"Village House B",new Vector3(5,.6f,-8),new Vector2(3.4f,3),timber,stone,roof,atlas);
            House(root.transform,"Player Home",new Vector3(4,.6f,2),new Vector2(2.6f,3),timber,stone,roof,atlas);
            // The established interaction anchor remains in front of the new doorway.
            Road(root.transform,new Vector3(0,.61f,7),new Vector2(4,4),atlas);
            Road(root.transform,new Vector3(0,.61f,10.5f),new Vector2(2.4f,3),atlas);
            Road(root.transform,new Vector3(-4.5f,.61f,7),new Vector2(5,2.2f),atlas);
            Road(root.transform,new Vector3(6,.61f,6.4f),new Vector2(8,2.2f),atlas);
            Road(root.transform,new Vector3(4,.61f,4.4f),new Vector2(2.2f,1.8f),atlas);
            Road(root.transform,new Vector3(-12,.61f,0),new Vector2(2.2f,12),atlas);
            Road(root.transform,new Vector3(-9.5f,.61f,7),new Vector2(5,2.2f),atlas);
            Road(root.transform,new Vector3(-10.5f,.61f,-2.8f),new Vector2(3,2.2f),atlas);
            Road(root.transform,new Vector3(6.8f,.61f,0),new Vector2(2.2f,11),atlas);
            Road(root.transform,new Vector3(5.7f,.61f,-5.4f),new Vector2(4,2.2f),atlas);
            foreach (var p in new[] {new Vector3(-3,0,8.8f),new Vector3(3,0,8.8f),new Vector3(-11,0,8.8f),new Vector3(10.5f,0,6)})
            {
                HomeInteriorAuthoring.Part(root.transform,"pillar-wood",p+Vector3.up*.6f,new Vector3(.18f,2.5f,.18f),0,timber);
                HomeInteriorAuthoring.Part(root.transform,"lantern",p+Vector3.up*2.7f,new Vector3(.45f,.65f,.45f),0,atlas);
            }
            foreach (var p in new[] {new Vector3(-14,0,2),new Vector3(-13,0,-6),new Vector3(-6,0,-10),new Vector3(1,0,-12),new Vector3(11,0,-5),new Vector3(12,0,3)})
                HomeInteriorAuthoring.Part(root.transform,"tree",p+Vector3.up*.6f,new Vector3(2,3.8f,2),0,atlas);
            foreach (var p in new[] {new Vector3(-10,0,-8),new Vector3(-8,0,-8),new Vector3(4,0,-10.5f),new Vector3(6,0,-10.5f)})
                HomeInteriorAuthoring.Part(root.transform,"fence",p+Vector3.up*.6f,new Vector3(1.9f,.9f,.15f),0,timber);
            PrefabUtility.SaveAsPrefabAsset(root,Output+"ForestVillage.prefab"); Object.DestroyImmediate(root);
        }
        private static Material ColorMaterial(string name, Color color)
        {
            var material=AssetDatabase.LoadAssetAtPath<Material>(Output+name+".mat");
            if(material!=null)return material;
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetColor("_BaseColor",color);material.SetFloat("_Smoothness",.1f);
            AssetDatabase.CreateAsset(material,Output+name+".mat");return material;
        }
        private static void Road(Transform root, Vector3 p, Vector2 size, Material material)
            => HomeInteriorAuthoring.Part(root,"road",p,new Vector3(size.x,.025f,size.y),0,material);
        private static void House(Transform parent,string name,Vector3 p,Vector2 size,Material wall,Material trim,Material roof,Material atlas)
        {
            var root=new GameObject(name);root.transform.SetParent(parent,false);
            float h=2.4f,front=p.z+size.y/2;
            HomeInteriorAuthoring.Part(root.transform,"wall",p,new Vector3(size.x,.16f,size.y),0,trim);
            HomeInteriorAuthoring.Part(root.transform,"wall-wood",p+new Vector3(0,.16f,-size.y/2),new Vector3(size.x,h,.18f),0,wall);
            foreach(float sign in new[]{-1f,1f})
            {
                HomeInteriorAuthoring.Part(root.transform,"wall-wood-window-small",p+new Vector3(sign*size.x/2,.16f,0),new Vector3(size.y,h,.18f),90,wall);
                float segment=(size.x-1.1f)/2;
                HomeInteriorAuthoring.Part(root.transform,"wall-wood",new Vector3(p.x+sign*(.55f+segment/2),p.y+.16f,front),new Vector3(segment,h,.18f),0,wall);
            }
            HomeInteriorAuthoring.Part(root.transform,"wall-wood-doorway-square",new Vector3(p.x,p.y+.16f,front),new Vector3(1.1f,h,.2f),180,trim);
            HomeInteriorAuthoring.Part(root.transform,"wall-wood-door",new Vector3(p.x,p.y+.16f,front+.04f),new Vector3(.9f,1.9f,.12f),180,atlas);
            HomeInteriorAuthoring.Part(root.transform,"roof-gable",p+Vector3.up*(h+.12f),new Vector3(size.x+.5f,1.3f,size.y+.5f),0,roof);
            root.AddComponent<VillageObstacle>();var collider=root.GetComponent<BoxCollider>();
            collider.center=p+Vector3.up*(h/2);collider.size=new Vector3(size.x,h,size.y);
        }
    }
}
