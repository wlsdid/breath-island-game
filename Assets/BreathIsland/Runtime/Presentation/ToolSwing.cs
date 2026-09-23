using System;
using UnityEngine;
namespace BreathIsland
{
    // Applies the tool pose after locomotion, keeping the grip attached to the hand.
    [DefaultExecutionOrder(100)]
    public sealed class ToolSwing : MonoBehaviour
    {
        private Transform player, upper, lower, wrist;
        private Quaternion upperBasis, lowerBasis;
        private float elapsed = .5f;
        private bool toolHeld;
        public void Initialize(Transform owner)
        {
            player = owner;
            var bones = owner.GetComponentsInChildren<Transform>();
            upper = Array.Find(bones, t => t.name == "upperarm.r");
            lower = Array.Find(bones, t => t.name == "lowerarm.r");
            wrist = Array.Find(bones, t => t.name == "wrist.r");
            if (upper != null) upperBasis = Quaternion.Inverse(owner.rotation) * upper.rotation;
            if (lower != null) lowerBasis = Quaternion.Inverse(owner.rotation) * lower.rotation;
        }
        public void SetHeldTool(bool held) { toolHeld = held; elapsed = .5f; transform.localRotation = Quaternion.identity; }
        public void Play() { if (toolHeld) elapsed = 0; }
        private void LateUpdate()
        {
            if (!toolHeld || player == null) return;
            elapsed = Mathf.Min(.5f, elapsed + Time.deltaTime);
            ApplyPose(elapsed);
        }
        public void ApplyPose(float seconds)
        {
            if (!toolHeld || player == null) return;
            // Wind up, strike at the gameplay impact (0.25s), then return to carrying.
            float shoulder = Pose(seconds, -12, -160, -48);
            float elbow = Pose(seconds, -35, -155, -70);
            float blade = Pose(seconds, 0, -20, 78);
            float outward = Pose(seconds, .08f, .4f, .12f);
            // Start from a fixed rig basis so repeated poses do not accumulate arm twist.
            if (upper != null) upper.rotation = player.rotation * upperBasis;
            if (lower != null) lower.rotation = player.rotation * lowerBasis;
            Aim(upper, lower, player.rotation * (Quaternion.Euler(shoulder, 0, 0) * Vector3.down + Vector3.right * outward));
            Aim(lower, wrist, player.rotation * (Quaternion.Euler(elbow, 0, 0) * Vector3.down));
            // Corrected KayKit tools have a Y shaft and an X cutting edge.
            transform.rotation = player.rotation * Quaternion.Euler(blade, 0, 0) * Quaternion.Euler(0, -90, 0);
        }
        private static void Aim(Transform bone, Transform child, Vector3 direction)
        {
            if (bone != null && child != null)
                bone.rotation = Quaternion.FromToRotation(child.position - bone.position, direction) * bone.rotation;
        }
        private static float Pose(float t, float rest, float raised, float impact)
        {
            if (t < .16f) return Mathf.Lerp(rest, raised, Mathf.SmoothStep(0, 1, t / .16f));
            if (t < .25f) return Mathf.Lerp(raised, impact, (t - .16f) / .09f);
            return Mathf.Lerp(impact, rest, Mathf.SmoothStep(0, 1, (t - .25f) / .25f));
        }
    }
}
