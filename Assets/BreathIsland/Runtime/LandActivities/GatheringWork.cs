using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    // Transient work, never a second source of inventory or harvest rewards.
    public sealed class GatheringWork
    {
        private readonly Dictionary<string,int> hits=new();
        private string held;
        private float seconds;
        public int Hits(string id)=>hits.TryGetValue(id,out var value)?value:0;
        public float Seconds(string id)=>held==id?seconds:0;
        public bool Hit(string id,int required)
        {
            if(string.IsNullOrEmpty(id))return false;
            hits[id]=Mathf.Min(Mathf.Max(1,required),Hits(id)+1);
            return hits[id]>=Mathf.Max(1,required);
        }
        public bool Hold(string id,float delta,float required)
        {
            if(string.IsNullOrEmpty(id) || delta<0 || float.IsNaN(delta) || float.IsInfinity(delta))return false;
            if(held!=id){held=id;seconds=0;}
            seconds+=delta;return seconds>=Mathf.Max(0,required);
        }
        public void CancelHold(){held=null;seconds=0;}
        public void Complete(string id){hits.Remove(id);if(held==id)CancelHold();}
        public void Reset(){hits.Clear();CancelHold();}
    }
}
