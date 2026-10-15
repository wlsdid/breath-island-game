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
            Tufts(Material(new Color(.28f,.49f,.23f)),Material(new Color(.51f,.66f,.31f)));
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
        private void Tufts(Material dark,Material light)
        {
            for(int variant=0;variant<2;variant++)
            {
                var vertices=new List<Vector3>();var triangles=new List<int>();
                for(int i=0;i<100;i++)
                {
                    float a=(i*.618034f+variant*.31f)*Mathf.PI*2;
                    float radius=.82f+(i%7)*.018f;
                    Vector3 pos=new Vector3(Mathf.Cos(a)*18*radius,.608f,-5+Mathf.Sin(a)*15*radius);
                    if(pos.z>7 && Mathf.Abs(pos.x)<8)continue;
                    for(int blade=0;blade<3;blade++)
                    {
                        float angle=a+blade*2.1f;
                        Vector3 across=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*.07f;
                        Vector3 top=pos+Vector3.up*(.18f+(i%4)*.04f)+across;
                        int n=vertices.Count;vertices.Add(pos-across);vertices.Add(pos+across);vertices.Add(top);
                        triangles.AddRange(new[]{n,n+2,n+1,n,n+1,n+2});
                    }
                }
                MeshObject("Coastal grass "+variant,Vector3.zero,vertices,triangles,variant==0?dark:light);
            }
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
