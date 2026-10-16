using UnityEngine;

namespace BreathIsland
{
    // Visual attention only: never moves the interaction anchor or writes game state.
    public sealed class NpcPresentation : MonoBehaviour
    {
        public Transform visual;
        private Transform player;
        private Quaternion restingRotation;

        public void Initialize(Transform target)
        {
            player = target;
            restingRotation = visual.localRotation;
        }

        private void LateUpdate()
        {
            if (visual == null || player == null) return;
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            Quaternion desired = restingRotation;
            if (direction.sqrMagnitude > .01f && direction.sqrMagnitude < 25f)
                desired = Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(direction);
            visual.localRotation = Quaternion.RotateTowards(visual.localRotation, desired, 100f * Time.deltaTime);
        }
    }
}
