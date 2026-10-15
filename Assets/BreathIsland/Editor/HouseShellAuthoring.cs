using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    public static class HouseShellAuthoring
    {
        public static void Build()
        {
            const string folder="Assets/BreathIsland/Resources/World/";
            var root=PrefabUtility.LoadPrefabContents(folder+"ForestVillage.prefab");
            var wall=AssetDatabase.LoadAssetAtPath<Material>(folder+"VillageTimber.mat");
            var plaster=AssetDatabase.LoadAssetAtPath<Material>(folder+"VillagePlaster.mat");
            var roofMaterial=AssetDatabase.LoadAssetAtPath<Material>(folder+"VillageRoof.mat");
            var roof=AssetDatabase.LoadAssetAtPath<Mesh>(folder+"ClosedRoof.asset");
            if(roof==null)
            {
                var p=new[]{new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(0,1,-.5f),new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f),new Vector3(0,1,.5f)};
                var vertices=new List<Vector3>();var sides=new List<int>();var ends=new List<int>();
                void Triangle(int a,int b,int c,List<int> indices){int n=vertices.Count;vertices.Add(p[a]);vertices.Add(p[b]);vertices.Add(p[c]);indices.AddRange(new[]{n,n+1,n+2});}
                Triangle(0,3,5,sides);Triangle(0,5,2,sides);Triangle(1,2,5,sides);Triangle(1,5,4,sides);Triangle(0,1,4,sides);Triangle(0,4,3,sides);
                Triangle(0,2,1,ends);Triangle(3,4,5,ends);
                roof=new Mesh{name="Closed pitched roof"};roof.SetVertices(vertices);roof.subMeshCount=2;roof.SetTriangles(sides,0);roof.SetTriangles(ends,1);roof.RecalculateNormals();roof.RecalculateBounds();
                AssetDatabase.CreateAsset(roof,folder+"ClosedRoof.asset");
            }
            try
            {
                foreach(var house in root.GetComponentsInChildren<VillageObstacle>())
                {
                    var box=house.GetComponent<BoxCollider>();if(box==null)continue;
                    var old=house.transform.Find("Solid shell");if(old!=null)Object.DestroyImmediate(old.gameObject);
                    var oldRoof=house.transform.Find("roof-gable");if(oldRoof!=null)oldRoof.gameObject.SetActive(false);
                    var shell=new GameObject("Solid shell").transform;shell.SetParent(house.transform,false);
                    var body=GameObject.CreatePrimitive(PrimitiveType.Cube);body.name="Recessed wall backing";body.transform.SetParent(shell,false);
                    body.transform.localPosition=box.center+Vector3.up*.08f;body.transform.localScale=box.size-new Vector3(.12f,0,.12f);
                    body.GetComponent<Renderer>().sharedMaterial=wall;Object.DestroyImmediate(body.GetComponent<Collider>());
                    var top=new GameObject("Closed roof",typeof(MeshFilter),typeof(MeshRenderer));top.transform.SetParent(shell,false);
                    top.transform.localPosition=box.center+Vector3.up*(box.size.y*.5f+.12f);
                    top.transform.localScale=new Vector3(box.size.x+.5f,1.3f,box.size.z+.5f);top.GetComponent<MeshFilter>().sharedMesh=roof;top.GetComponent<Renderer>().sharedMaterials=new[]{roofMaterial,plaster};
                }
                PrefabUtility.SaveAsPrefabAsset(root,folder+"ForestVillage.prefab");
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
            AssetDatabase.DeleteAsset(folder+"GableClosure.asset");AssetDatabase.SaveAssets();
        }
    }
}
