using UnityEngine;

namespace BreathIsland
{
    // Presentation-only curves. No random calls, input polling, or gameplay writes.
    public static class FishingPose
    {
        public const float CastDuration = .9f;
        public static Vector3 RodPoint(float along, float tension)
        {
            float t = Mathf.Clamp01(along);
            float bend = Mathf.Clamp01(tension / 100f);
            return new Vector3(0, -bend * .85f * t * t * t, t * 2.25f);
        }
        public static Vector3 CastPoint(Vector3 start, Vector3 end, float progress)
        {
            float t = Mathf.Clamp01(progress);
            return Vector3.Lerp(start, end, t) + Vector3.up * (4f * t * (1 - t) * 2.6f);
        }
        public static Vector3 LinePoint(Vector3 start, Vector3 end, float along, float tension)
        {
            float t = Mathf.Clamp01(along);
            return Vector3.Lerp(start, end, t) + Vector3.down * (4 * t * (1 - t) * Mathf.Lerp(.65f, .03f, Mathf.Clamp01(tension / 100)));
        }
        public static float JumpHeight(float seconds)
        {
            float cycle = Mathf.Repeat(seconds, 4.8f);
            return cycle < .95f ? Mathf.Sin(cycle / .95f * Mathf.PI) * 1.15f : 0;
        }
    }
}
