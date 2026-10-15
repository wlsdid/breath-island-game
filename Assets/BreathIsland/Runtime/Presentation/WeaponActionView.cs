using System;
using UnityEngine;
namespace BreathIsland
{
    [DefaultExecutionOrder(110)]
    public sealed class WeaponActionView : MonoBehaviour
    {
        private Transform player,upper,lower,wrist,leftUpper,leftLower,leftWrist;
        private Quaternion upperRest,lowerRest,leftUpperRest,leftLowerRest;
        private string job;
        private float time=1;
        public void Initialize(Transform owner)
        {
            player=owner;var bones=owner.GetComponentsInChildren<Transform>();
            Transform Bone(string name)=>Array.Find(bones,t=>t.name==name);
            upper=Bone("upperarm.r");lower=Bone("lowerarm.r");wrist=Bone("wrist.r");
            leftUpper=Bone("upperarm.l");leftLower=Bone("lowerarm.l");leftWrist=Bone("wrist.l");
            Quaternion Rest(Transform bone)=>bone==null?Quaternion.identity:Quaternion.Inverse(owner.rotation)*bone.rotation;
            upperRest=Rest(upper);lowerRest=Rest(lower);leftUpperRest=Rest(leftUpper);leftLowerRest=Rest(leftLower);
        }
        public void Configure(string id){job=id;time=1;transform.localRotation=Quaternion.identity;}
        public void Play(){if(job!=null)time=0;}
        private void LateUpdate()
        {
            time+=Time.deltaTime;if(job==null)return;if(time>=.6f){transform.localRotation=Quaternion.identity;return;}
            float amount=time<.18f?Mathf.SmoothStep(0,1,time/.18f):1-Mathf.SmoothStep(0,1,(time-.18f)/.42f);
            if(upper==null||lower==null)return;
            upper.rotation=player.rotation*upperRest;lower.rotation=player.rotation*lowerRest;
            if(job=="ranger")
            {
                Aim(upper,lower,player.forward*.8f+player.right*.5f);
                Aim(lower,wrist,Vector3.Lerp(player.forward,-player.forward*.25f+player.right*.5f,amount));
                if(leftUpper!=null&&leftLower!=null)
                {
                    leftUpper.rotation=player.rotation*leftUpperRest;leftLower.rotation=player.rotation*leftLowerRest;
                    Aim(leftUpper,leftLower,player.forward-player.right*.25f);Aim(leftLower,leftWrist,player.forward);
                }
                transform.rotation=player.rotation*Quaternion.Euler(0,0,8*amount);
            }
            else
            {
                float lift=job=="mage"?-105:-150;
                Aim(upper,lower,player.rotation*(Quaternion.Euler(Mathf.Lerp(-15,lift,amount),0,0)*Vector3.down+Vector3.right*.2f));
                Aim(lower,wrist,player.rotation*(Quaternion.Euler(Mathf.Lerp(-30,-110,amount),0,0)*Vector3.down));
                transform.rotation=player.rotation*Quaternion.Euler(job=="mage"?-20*amount:70*amount,0,job=="mage"?0:-45*amount);
            }
        }
        private static void Aim(Transform bone,Transform child,Vector3 direction)
        {if(bone!=null&&child!=null)bone.rotation=Quaternion.FromToRotation(child.position-bone.position,direction)*bone.rotation;}
    }
}
