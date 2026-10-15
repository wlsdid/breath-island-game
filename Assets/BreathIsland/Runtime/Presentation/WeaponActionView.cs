using System;
using UnityEngine;
namespace BreathIsland
{
    // Local-space carrying and attack poses, applied after locomotion.
    [DefaultExecutionOrder(110)]
    public sealed class WeaponActionView : MonoBehaviour
    {
        private Transform player, upper, lower, wrist, leftUpper, leftLower, leftWrist;
        private Quaternion upperRest, lowerRest, leftUpperRest, leftLowerRest;
        private string job;
        private float time = 1;
        public Transform Owner => player;
        public float DrawFraction => job == "ranger" ? DrawAt(time) : 0;
        public static float DrawAt(float elapsed) => elapsed < .10f ? 0 : elapsed < .25f ? Mathf.SmoothStep(0,1,(elapsed-.10f)/.15f) : elapsed < .30f ? 1 : elapsed < .35f ? 1-Mathf.SmoothStep(0,1,(elapsed-.30f)/.05f) : 0;
        public void Initialize(Transform owner)
        {
            player = owner; var bones = owner.GetComponentsInChildren<Transform>();
            Transform Bone(string name) => Array.Find(bones, t => t.name == name);
            upper = Bone("upperarm.r"); lower = Bone("lowerarm.r"); wrist = Bone("wrist.r");
            leftUpper = Bone("upperarm.l"); leftLower = Bone("lowerarm.l"); leftWrist = Bone("wrist.l");
            Quaternion Rest(Transform bone) => bone == null ? Quaternion.identity : Quaternion.Inverse(owner.rotation) * bone.rotation;
            upperRest = Rest(upper); lowerRest = Rest(lower); leftUpperRest = Rest(leftUpper); leftLowerRest = Rest(leftLower);
        }
        public void Configure(string id) { job = id; time = 1; transform.localRotation = Quaternion.identity; }
        public void Play() { if (job != null) time = 0; }
        private void LateUpdate()
        {
            time += Time.deltaTime;
            if (job == null || upper == null || lower == null) return;
            float pose = time < .2f ? Mathf.SmoothStep(0, 1, time / .2f) : 1 - Mathf.SmoothStep(0, 1, (time - .2f) / .4f);
            upper.rotation = player.rotation * upperRest; lower.rotation = player.rotation * lowerRest;
            Vector3 shoulder = new Vector3(.65f, -.8f, .25f);
            Vector3 forearm = new Vector3(.15f, -.2f, 1);
            float pitch = 12;
            if (job == "ranger")
            {
                float aim = time < .10f ? Mathf.SmoothStep(0,1,time/.10f) : 1-Mathf.SmoothStep(0,1,(time-.36f)/.14f);
                shoulder = Vector3.Lerp(shoulder, new Vector3(.4f, -.1f, 1), aim);
                forearm = Vector3.Lerp(forearm, new Vector3(0, .05f, 1), aim);
                if (leftUpper != null && leftLower != null)
                {
                    leftUpper.rotation = player.rotation * leftUpperRest; leftLower.rotation = player.rotation * leftLowerRest;
                    Aim(leftUpper, leftLower, player.rotation * Vector3.Lerp(new Vector3(-.4f,-1,0),new Vector3(-.25f,-.1f,.8f),aim));
                    Aim(leftLower, leftWrist, player.rotation * Vector3.Lerp(Vector3.down,new Vector3(.6f,.1f,Mathf.Lerp(.9f,-.8f,DrawFraction)),aim));
                }                pitch = 0;
            }
            else
            {
                shoulder = Vector3.Lerp(shoulder, new Vector3(.6f, .55f, .4f), pose);
                forearm = Vector3.Lerp(forearm, new Vector3(.1f, 1, .15f), pose);
                // Wind-up followed by a forward strike, then settle to the same carrying pose.
                pitch = time < .2f ? Mathf.Lerp(12, -65, pose) : time < .32f ? Mathf.Lerp(-65, 75, Mathf.SmoothStep(0, 1, (time - .2f) / .12f)) : Mathf.Lerp(75, 12, Mathf.SmoothStep(0, 1, (time - .32f) / .28f));
                if (time >= .6f) pitch = 12;
                if (job == "mage") pitch *= .4f;
            }
            Aim(upper, lower, player.rotation * shoulder);
            Aim(lower, wrist, player.rotation * forearm);
            transform.rotation = player.rotation * Quaternion.Euler(pitch, 0, job == "warrior" ? -15 : 0);
        }
        private static void Aim(Transform bone, Transform child, Vector3 direction)
        { if (bone != null && child != null) bone.rotation = Quaternion.FromToRotation(child.position - bone.position, direction) * bone.rotation; }
    }
}
