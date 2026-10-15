using System.IO;
using UnityEditor;
using UnityEngine;

namespace BreathIsland.Editor
{
    public static class VillagePolishAuthoring
    {
        private const string World = "Assets/BreathIsland/Resources/World/";
        private const string Art = "Assets/BreathIsland/Art/ThirdParty/KayKit/Villagers/";

        [MenuItem("숨결섬/NPC와 마을 표현 보완")]
        public static void Build()
        {
            Directory.CreateDirectory(World + "Npcs");
            AssetDatabase.Refresh();
            var catalog = IslandLifeCatalog.Load();
            Npc("Nari", "Rogue", "rogue_texture", 1.42f, 150, catalog);
            Npc("Maru", "Mage", "mage_texture", 1.55f, 220, catalog);
            var root = PrefabUtility.LoadPrefabContents(World + "ForestVillage.prefab");
            try
            {
                var previous = root.transform.Find("Village Detail Pass");
                if (previous != null) Object.DestroyImmediate(previous.gameObject);
                var details = new GameObject("Village Detail Pass").transform;
                details.SetParent(root.transform, false);
                var timber = AssetDatabase.LoadAssetAtPath<Material>(World + "VillageTimber.mat");
                var stone = AssetDatabase.LoadAssetAtPath<Material>(World + "VillageStone.mat");
                var atlas = AssetDatabase.LoadAssetAtPath<Material>(World + "Town.mat");
                var cream = Material("VillagePlaster", new Color(.86f,.80f,.64f));
                var teal = Material("VillageCanvas", new Color(.14f,.40f,.39f));
                // Existing footprints and door centres stay fixed; details sit against facades.
                Facade(details, new Vector3(-9,.6f,5.7f),4,2.4f,timber,cream,teal);
                Facade(details, new Vector3(10,.6f,2.9f),3.4f,2.4f,timber,cream,teal);
                Facade(details, new Vector3(4,.6f,3.5f),2.6f,2.4f,timber,cream,teal);
                Facade(details, new Vector3(-9.7f,.6f,-4),2,2.2f,timber,cream,teal);
                Facade(details, new Vector3(4.45f,.6f,-6.5f),2.3f,3.3f,timber,cream,teal);
                foreach (var p in new[] {new Vector3(-10.5f,.6f,5.95f),new Vector3(-7.5f,.6f,5.95f),new Vector3(8.8f,.6f,3.2f),new Vector3(11.2f,.6f,3.2f),new Vector3(3,.6f,3.75f),new Vector3(5,.6f,3.75f)})
                    Planter(details,p,timber,catalog);
                // Rest areas stay off the roads and outside NPC dialogue approach space.
                Bench(details,new Vector3(-4.1f,.6f,9.4f),timber,stone);
                Bench(details,new Vector3(4.7f,.6f,8.8f),timber,stone);
                foreach (var p in new[] {new Vector3(-5.3f,.6f,10),new Vector3(5.9f,.6f,9.5f)})
                    Planter(details,p,timber,catalog);
                foreach (float x in new[]{-2.15f,2.15f})
                    for(int i=0;i<4;i++)
                        Block(details,"Plaza stone edging",new Vector3(x,.65f,5.5f+i),new Vector3(.17f,.1f,.85f),stone);
                foreach(var p in new[]{new Vector3(-10,3.3f,3.1f),new Vector3(5,4.1f,-8.5f)})
                {
                    Block(details,"Stone chimney",p,new Vector3(.5f,1.2f,.55f),stone);
                    Block(details,"Chimney cap",p+Vector3.up*.65f,new Vector3(.65f,.15f,.7f),timber);
                }
                // No new collider or interaction target is introduced by the dressing pass.
                foreach(var collider in details.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
                PrefabUtility.SaveAsPrefabAsset(root,World+"ForestVillage.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }

        private static void Npc(string name,string model,string texture,float height,float yaw,IslandLifeCatalog catalog)
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Art+model+".fbx");
            importer.animationType=ModelImporterAnimationType.Generic;
            importer.avatarSetup=ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar=catalog.characterAvatar;
            importer.SaveAndReimport();
            var material=Material("Npcs/"+name,Color.white);
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+texture+".png"));
            var root=new GameObject(name);
            var visual=ModelPresentation.Create(AssetDatabase.LoadAssetAtPath<GameObject>(Art+model+".fbx"),root.transform,material,height);
            visual.name="Villager Visual";
            // NPC anchors are retained at y=1.2; the visible ground is y=.6.
            visual.transform.localPosition=Vector3.down*.6f;
            visual.transform.localRotation=Quaternion.Euler(0,yaw,0);
            var animator=visual.GetComponentInChildren<Animator>();
            if(animator==null)animator=visual.transform.GetChild(0).gameObject.AddComponent<Animator>();
            animator.avatar=catalog.characterAvatar;
            animator.runtimeAnimatorController=catalog.animator;
            animator.applyRootMotion=false;
            var collider=root.AddComponent<CapsuleCollider>();collider.radius=.35f;collider.height=height;collider.center=new Vector3(0,height/2-.6f,0);
            root.AddComponent<NpcPresentation>().visual=visual.transform;
            PrefabUtility.SaveAsPrefabAsset(root,World+"Npcs/"+name+".prefab");
            Object.DestroyImmediate(root);
        }

        private static Material Material(string name,Color color)
        {
            string path=World+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material!=null)return material;
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetColor("_BaseColor",color);material.SetFloat("_Smoothness",.12f);
            AssetDatabase.CreateAsset(material,path);return material;
        }
        private static void Facade(Transform parent,Vector3 front,float width,float height,Material timber,Material plaster,Material canvas)
        {
            // Contrasting framed side panels, upper beam, and short door canopy.
            foreach(float sign in new[]{-1f,1f})
            {
                var p=front+new Vector3(sign*width*.34f,1.15f,.12f);
                Block(parent,"Plaster inset",p,new Vector3(width*.19f,1.25f,.08f),plaster);
                Block(parent,"Timber window sill",p+new Vector3(0,-.38f,.06f),new Vector3(width*.22f,.09f,.2f),timber);
                Block(parent,"Teal shutter",p+new Vector3(0,.1f,.06f),new Vector3(width*.12f,.46f,.06f),canvas);
            }
            Block(parent,"Facade crossbeam",front+new Vector3(0,height-.08f,.14f),new Vector3(width,.12f,.14f),timber);
            var canopy=Block(parent,"Door canopy",front+new Vector3(0,1.98f,.25f),new Vector3(1.05f,.1f,.55f),canvas);
            canopy.localRotation=Quaternion.Euler(-10,0,0);
        }
        private static void Planter(Transform parent,Vector3 p,Material wood,IslandLifeCatalog catalog)
        {
            Block(parent,"Timber planter",p+Vector3.up*.16f,new Vector3(.7f,.32f,.55f),wood);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BreathIsland/Art/ThirdParty/KayKit/Nature/Bush_1_A_Color1.fbx");
            var plant=ModelPresentation.Create(source,parent,catalog.natureMaterial,.55f);
            plant.name="Decorative planter foliage";plant.transform.localPosition=p+Vector3.up*.28f;
        }
        private static void Bench(Transform parent,Vector3 p,Material wood,Material stone)
        {
            Block(parent,"Bench seat",p+Vector3.up*.43f,new Vector3(1.7f,.12f,.52f),wood);
            Block(parent,"Bench back",p+new Vector3(0,.83f,-.22f),new Vector3(1.7f,.5f,.1f),wood);
            foreach(float x in new[]{-.6f,.6f})Block(parent,"Bench support",p+new Vector3(x,.2f,0),new Vector3(.2f,.4f,.45f),stone);
        }
        private static Transform Block(Transform parent,string name,Vector3 p,Vector3 size,Material material)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetParent(parent,false);obj.transform.localPosition=p;obj.transform.localScale=size;
            obj.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(obj.GetComponent<Collider>());return obj.transform;
        }
    }
}
