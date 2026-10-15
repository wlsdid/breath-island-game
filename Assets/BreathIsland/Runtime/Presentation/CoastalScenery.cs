using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BreathIsland
{
    // Render-only coastal layers. Never changes terrain meshes, colliders, or Unity's RNG.
    public sealed class CoastalScenery : MonoBehaviour
    {
        private readonly List<Mesh> meshes = new();
        private readonly List<Material> materials = new();
        private void Awake()
        {
            var sand=Material(new Color(.73f,.70f,.43f));
            var edge=Material(new Color(.35f,.55f,.27f));
            // Existing island radius is 19 x 16; all added surfaces are non-colliding.
            Ring("Grass fringe",new Vector3(0,.603f,-5),18.15f,15.15f,.36f,edge,false);
            Ring("Sand fringe",new Vector3(0,.802f,11),5.65f,3.65f,.30f,sand,false);
            transform.parent.gameObject.AddComponent<GroundCover>();
        }
        private Material Material(Color color)
        {
            var mat=new Material(Resources.Load<Material>("World/Coastal/Blend"));
            mat.SetColor("_BaseColor",color);materials.Add(mat);return mat;
        }
        private Transform Ring(string name,Vector3 center,float x,float z,float width,Material material,bool broken)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<128;i++)
            {
                if(broken && i%12>7)continue;
                // Keep the beach/dock connection free of an intersecting foam ring.
                if(broken && center.z<0 && i>22 && i<43)continue;
                float a=i*Mathf.PI/64,b=(i+1)*Mathf.PI/64;
                int n=vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a)*x,0,Mathf.Sin(a)*z));
                vertices.Add(new Vector3(Mathf.Cos(a)*(x+width),0,Mathf.Sin(a)*(z+width)));
                vertices.Add(new Vector3(Mathf.Cos(b)*x,0,Mathf.Sin(b)*z));
                vertices.Add(new Vector3(Mathf.Cos(b)*(x+width),0,Mathf.Sin(b)*(z+width)));
                triangles.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});
            }
            var result=MeshObject(name,center,vertices,triangles,material);
            var mesh=result.GetComponent<MeshFilter>().sharedMesh;
            var colors=new Color[vertices.Count];
            for(int i=0;i<colors.Length;i++)colors[i]=new Color(1,1,1,broken?.65f:(i%2==0?.60f:0));
            mesh.colors=colors;
            return result;
        }
        private Transform MeshObject(string name,Vector3 position,List<Vector3> vertices,List<int> indices,Material material)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var colors=new Color[vertices.Count];for(int i=0;i<colors.Length;i++)colors[i]=new Color(1,1,1,.7f);mesh.colors=colors;meshes.Add(mesh);
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);go.transform.localPosition=position;
            go.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
            return go.transform;
        }
        private void OnDestroy(){foreach(var mesh in meshes)Destroy(mesh);foreach(var mat in materials)Destroy(mat);}
    }
}
