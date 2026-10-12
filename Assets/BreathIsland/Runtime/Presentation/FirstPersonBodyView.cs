using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    // Hide only this player's visual hierarchy; never disable its motor or other actors.
    public sealed class FirstPersonBodyView : MonoBehaviour
    {
        private readonly Dictionary<Renderer,bool> previous=new();
        private bool hidden;
        public void SetFirstPerson(bool value)
        {
            if(hidden==value)return;
            hidden=value;
            if(value)
            {
                foreach(var renderer in GetComponentsInChildren<Renderer>(true))
                {previous[renderer]=renderer.forceRenderingOff;renderer.forceRenderingOff=true;}
            }
            else Restore();
        }
        private void Restore()
        {
            foreach(var pair in previous)if(pair.Key!=null)pair.Key.forceRenderingOff=pair.Value;
            previous.Clear();hidden=false;
        }
        private void OnDisable()=>Restore();
    }
}
