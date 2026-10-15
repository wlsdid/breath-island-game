using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace BreathIsland.Editor
{
    internal static class FishingHandAuthoring
    {
        public static Transform Build(Transform parent,string name,Vector3 position,Material skin,Material sleeve)
        {
            bool right=name.StartsWith("Right");string side=right?"Right":"Left";
            string path="Assets/BreathIsland/Resources/World/Coastal/"+side+"FishingHand.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            // Rebuild derived geometry while preserving its asset GUID.
            {
                var source=Object.Instantiate(IslandLifeCatalog.Load().characterModel);
                try
                {
                    var renderer=source.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Ranger_Arm"+side);
                    var hand=source.GetComponentsInChildren<Transform>().First(t=>t.name==(right?"hand.r":"hand.l"));
                    var baked=new Mesh();renderer.BakeMesh(baked);
                    var vertices=baked.vertices;var original=vertices.Select(renderer.transform.TransformPoint).ToArray();
                    var rotation=Quaternion.Euler(0,right?35:-35,right?90:-90);
                    for(int i=0;i<vertices.Length;i++)vertices[i]=rotation*(original[i]-hand.position)*.55f;
                    var triangles=new List<int>();var indices=baked.triangles;
                    for(int i=0;i<indices.Length;i+=3)
                        if(Enumerable.Range(0,3).All(k=>(right?original[indices[i+k]].x:-original[indices[i+k]].x)>.63f))
                            triangles.AddRange(new[]{indices[i],indices[i+1],indices[i+2]});
                    baked.vertices=vertices;baked.triangles=triangles.ToArray();baked.RecalculateNormals();baked.RecalculateBounds();
                    baked.name=side+" Ranger fishing hand";if(mesh==null){AssetDatabase.CreateAsset(baked,path);mesh=baked;}else{EditorUtility.CopySerialized(baked,mesh);Object.DestroyImmediate(baked);}
                }
                finally{Object.DestroyImmediate(source);}
            }
            var visibleBounds=new Bounds(mesh.vertices[mesh.triangles[0]],Vector3.zero);
            foreach(var index in mesh.triangles)visibleBounds.Encapsulate(mesh.vertices[index]);
            mesh.bounds=visibleBounds;EditorUtility.SetDirty(mesh);
            var root=new GameObject(name);root.transform.SetParent(parent,false);root.transform.localPosition=position;
            var palm=new GameObject("Palm",typeof(MeshFilter),typeof(MeshRenderer));palm.transform.SetParent(root.transform,false);
            palm.GetComponent<MeshFilter>().sharedMesh=mesh;palm.GetComponent<MeshRenderer>().sharedMaterial=IslandLifeCatalog.Load().characterMaterial;
            return root.transform;
        }
    }
}
