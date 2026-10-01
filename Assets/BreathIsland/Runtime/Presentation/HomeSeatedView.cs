using System;
using UnityEngine;
namespace BreathIsland
{
    // Presentation only: a pose layer on the existing Generic rig, with model-space joint bases.
    [DefaultExecutionOrder(150)]
    public sealed class HomeSeatedView : MonoBehaviour
    {
        private HomeInteractionSystem home;
        private Transform visual, thighL, thighR, shinL, shinR, footL, footR;
        private Vector3 originalPosition, targetPosition;
        private Quaternion originalRotation, targetRotation;
        private Quaternion thighLBasis,thighRBasis,shinLBasis,shinRBasis;
        private float blend,hipHeight=.85f;
        private bool seated;
        private EquipmentView equipment;
        public void Initialize(HomeInteractionSystem system,EquipmentView equipmentView)
        {
            home=system;equipment=equipmentView;
            // The procedural placeholder is disabled and destroyed at the end of this frame.
            visual=Array.Find(GetComponentsInChildren<Transform>(),t=>t.name=="Hero Visual" && t.gameObject.activeSelf);
            if(visual==null)return;
            originalPosition=visual.localPosition;originalRotation=visual.localRotation;
            var bones=visual.GetComponentsInChildren<Transform>();
            thighL=Array.Find(bones,t=>t.name=="upperleg.l");thighR=Array.Find(bones,t=>t.name=="upperleg.r");
            shinL=Array.Find(bones,t=>t.name=="lowerleg.l");shinR=Array.Find(bones,t=>t.name=="lowerleg.r");
            footL=Array.Find(bones,t=>t.name=="foot.l");footR=Array.Find(bones,t=>t.name=="foot.r");

            thighLBasis=Basis(thighL);thighRBasis=Basis(thighR);shinLBasis=Basis(shinL);shinRBasis=Basis(shinR);
            home.SeatChanged+=SetPose;
        }
        private Quaternion Basis(Transform bone)=>bone!=null?Quaternion.Inverse(visual.rotation)*bone.rotation:Quaternion.identity;
        private void SetPose(bool value,Vector3 position,Quaternion rotation)
        {
            seated=value;
            if(value){if(thighL!=null && thighR!=null)hipHeight=(thighL.position.y+thighR.position.y)*.5f-visual.position.y;targetPosition=position-Vector3.up*hipHeight;targetRotation=rotation;}
            equipment.SetVisible(!value);
        }
        private void LateUpdate()
        {
            if(visual==null)return;
            blend=Mathf.MoveTowards(blend,seated?1:0,Time.deltaTime*5);
            visual.localPosition=Vector3.Lerp(originalPosition,transform.InverseTransformPoint(targetPosition),blend);
            visual.localRotation=Quaternion.Slerp(originalRotation,Quaternion.Inverse(transform.rotation)*targetRotation,blend);
            if(blend<=0)return;
            Aim(thighL,shinL,thighLBasis,visual.forward);Aim(thighR,shinR,thighRBasis,visual.forward);
            Aim(shinL,footL,shinLBasis,Vector3.down);Aim(shinR,footR,shinRBasis,Vector3.down);
        }
        private void Aim(Transform bone,Transform child,Quaternion basis,Vector3 direction)
        {
            if(bone==null || child==null)return;
            bone.rotation=visual.rotation*basis;
            var target=Quaternion.FromToRotation(child.position-bone.position,direction)*bone.rotation;
            bone.rotation=Quaternion.Slerp(bone.rotation,target,blend);
        }
        public void Dispose(){if(home!=null)home.SeatChanged-=SetPose;enabled=false;Destroy(this);}
        private void OnDestroy(){if(home!=null)home.SeatChanged-=SetPose;}
    }
}
