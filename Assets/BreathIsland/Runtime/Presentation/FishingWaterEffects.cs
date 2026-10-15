using UnityEngine;
using UnityEngine.Rendering;

namespace BreathIsland
{
    // Small reusable visual pool. No physics, random state, input or rewards.
    public sealed class FishingWaterEffects : MonoBehaviour
    {
        private sealed class Burst
        {
            public LineRenderer ring;
            public Transform[] drops=new Transform[12];
            public Vector3 center;
            public float age=2, strength;
        }
        private readonly Burst[] bursts=new Burst[4];
        private int next;
        private bool landed, above;
        private FishingStage previous;
        public int BurstCount { get; private set; }
        public int ActiveCount { get { int count=0;foreach(var b in bursts)if(b!=null&&b.age<1)count++;return count; } }
        private void Awake()
        {
            var material=Resources.Load<Material>("World/Coastal/Foam");
            for(int b=0;b<bursts.Length;b++)
            {
                var burst=bursts[b]=new Burst();
                var ring=new GameObject("Splash ring",typeof(LineRenderer));ring.transform.SetParent(transform,false);
                burst.ring=ring.GetComponent<LineRenderer>();burst.ring.sharedMaterial=material;
                burst.ring.positionCount=41;burst.ring.useWorldSpace=true;burst.ring.shadowCastingMode=ShadowCastingMode.Off;
                burst.ring.enabled=false;
                for(int i=0;i<burst.drops.Length;i++)
                {
                    var drop=GameObject.CreatePrimitive(PrimitiveType.Sphere);drop.name="Water droplet";drop.transform.SetParent(transform,false);
                    var collider=drop.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
                    drop.GetComponent<Renderer>().sharedMaterial=material;drop.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
                    burst.drops[i]=drop.transform;drop.SetActive(false);
                }
            }
        }
        public void ResetCast(){landed=false;above=false;}
        public void Sample(FishingStage stage,float castSeconds,Vector3 bobber,Vector3 fish,float dt)
        {
            if(stage==FishingStage.Waiting&&!landed&&castSeconds>=FishingPose.CastDuration){Emit(bobber,.55f);landed=true;}
            if(stage==FishingStage.Bite&&previous!=stage)Emit(bobber,.35f);
            bool nowAbove=stage==FishingStage.Hooked&&fish.y>OceanSurface.Height(fish,Time.time);
            if(stage==FishingStage.Hooked&&nowAbove!=above)Emit(fish,nowAbove?.7f:1.1f);
            above=nowAbove;previous=stage;
            foreach(var b in bursts)
            {
                b.age+=dt;bool active=b.age<1;b.ring.enabled=active;
                for(int i=0;i<b.drops.Length;i++)b.drops[i].gameObject.SetActive(active&&b.age<.6f);
                if(!active)continue;
                float radius=(.12f+b.age*1.6f)*b.strength;b.ring.widthMultiplier=.045f*(1-b.age);
                for(int i=0;i<41;i++)
                {
                    float angle=i*Mathf.PI*2/40;var p=b.center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*radius;
                    p.y=OceanSurface.Height(p,Time.time)+.025f;b.ring.SetPosition(i,p);
                }
                for(int i=0;i<b.drops.Length;i++)
                {
                    float a=i*Mathf.PI*2/b.drops.Length;
                    var p=b.center+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*b.age*b.strength*(1+i%3*.2f);
                    p.y=OceanSurface.Height(b.center,Time.time)+b.age*(2.8f+i%3*.3f)*b.strength-4.9f*b.age*b.age;
                    b.drops[i].position=p;b.drops[i].localScale=Vector3.one*.045f*(1-b.age);
                }
            }
        }
        private void Emit(Vector3 position,float strength)
        {
            var burst=bursts[next];next=(next+1)%bursts.Length;burst.center=position;burst.age=0;burst.strength=strength;BurstCount++;
        }
    }
}
