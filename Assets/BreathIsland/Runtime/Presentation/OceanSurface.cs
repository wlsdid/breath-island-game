using UnityEngine;
using UnityEngine.Rendering;

namespace BreathIsland
{
    // Shared visual surface; shader and floating actors sample the same wave equations.
    public sealed class OceanSurface : MonoBehaviour
    {
        public const float Level = -.7f;
        private Mesh mesh;
        public static float Height(Vector3 position, float time)
            => Level + .11f*Mathf.Sin(position.x*.52f+position.z*.31f+time*1.1f)
                + .055f*Mathf.Sin(position.x*-.73f+position.z*.57f+time*1.65f)
                + .025f*Mathf.Sin(position.x*1.7f+position.z*1.1f-time*2.4f);

        public static Vector3 Normal(Vector3 position,float time)
        {
            float a=.11f*Mathf.Cos(position.x*.52f+position.z*.31f+time*1.1f);
            float b=.055f*Mathf.Cos(position.x*-.73f+position.z*.57f+time*1.65f);
            float c=.025f*Mathf.Cos(position.x*1.7f+position.z*1.1f-time*2.4f);
            return new Vector3(-(a*.52f-b*.73f+c*1.7f),1,-(a*.31f+b*.57f+c*1.1f)).normalized;
        }

        public static float FloatHeight(Vector3 position,float time,bool biting,float biteSeconds)
            => Height(position,time)+.035f-(biting ? .16f+.10f*(.5f+.5f*Mathf.Sin(biteSeconds*15)) : 0);

        private void Awake()
        {
            const int cells=192;
            var vertices=new Vector3[(cells+1)*(cells+1)];
            var triangles=new int[cells*cells*6];
            for(int z=0;z<=cells;z++)for(int x=0;x<=cells;x++)
                vertices[z*(cells+1)+x]=new Vector3(Axis(x/(float)cells),Level,Axis(z/(float)cells));
            int index=0;
            for(int z=0;z<cells;z++)for(int x=0;x<cells;x++)
            {
                int a=z*(cells+1)+x,b=a+cells+1;
                triangles[index++]=a;triangles[index++]=b;triangles[index++]=a+1;
                triangles[index++]=a+1;triangles[index++]=b;triangles[index++]=b+1;
            }
            mesh=new Mesh{name="Ocean surface grid"};mesh.vertices=vertices;mesh.triangles=triangles;
            mesh.bounds=new Bounds(new Vector3(0,Level,0),new Vector3(1000,2,1000));
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=Resources.Load<Material>("World/Coastal/Ocean");
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=true;
            var bed=GameObject.CreatePrimitive(PrimitiveType.Plane);bed.name="Seabed";bed.transform.SetParent(transform,false);
            bed.transform.localPosition=new Vector3(0,-5.5f,0);bed.transform.localScale=new Vector3(100,1,100);
            var collider=bed.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            bed.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("World/Coastal/Seabed");
            bed.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
        }
        private static float Axis(float value)
        {
            float t=value*2-1,a=Mathf.Abs(t);
            return Mathf.Sign(t)*(a<.8f ? a*80 : 64+Mathf.Pow((a-.8f)/.2f,2)*436);
        }
        private void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
