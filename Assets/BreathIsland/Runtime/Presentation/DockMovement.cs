using UnityEngine;
namespace BreathIsland
{
    public static class DockMovement
    {
        // Visible planks: x +/-2.25, z 11.075..24.925. Keep the capsule inside their edges.
        public static Vector3 Clamp(Vector3 point)
            => new Vector3(Mathf.Clamp(point.x,-1.78f,1.78f),point.y,Mathf.Clamp(point.z,11.55f,24.45f));
        public static Vector3 ConstrainStep(Vector3 position,Vector3 step)
        {
            // Old saves may start outside the pier: never teleport them to a boundary.
            step.x=Mathf.Clamp(step.x,Mathf.Min(0,-1.78f-position.x),Mathf.Max(0,1.78f-position.x));
            step.z=Mathf.Clamp(step.z,Mathf.Min(0,11.55f-position.z),Mathf.Max(0,24.45f-position.z));
            return step;
        }
    }
}
