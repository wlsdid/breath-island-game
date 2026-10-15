using UnityEngine;

namespace BreathIsland
{
    // Authored models face +Z. Only visual parts move; the navigation root stays intact.
    public sealed class FishSwimView : MonoBehaviour
    {
        public Transform tail, fin;
        private Quaternion tailRest, finRest;
        private Vector3 tailAxis, finAxis;
        private float clock;
        private void Awake()
        {
            if(tail!=null){tailRest=tail.localRotation;tailAxis=tail.parent.InverseTransformDirection(transform.up);}
            if(fin!=null){finRest=fin.localRotation;finAxis=fin.parent.InverseTransformDirection(transform.forward);}
        }
        private void Update()
        {
            clock+=Time.deltaTime;
            if(tail!=null)tail.localRotation=Quaternion.AngleAxis(Mathf.Sin(clock*9)*23,tailAxis)*tailRest;
            if(fin!=null)fin.localRotation=Quaternion.AngleAxis(Mathf.Sin(clock*9+1)*7,finAxis)*finRest;
        }
    }
}
