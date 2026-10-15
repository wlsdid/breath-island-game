using UnityEngine;
namespace BreathIsland
{
    public sealed class CharacterPresentation : MonoBehaviour
    {
        private Animator animator;
        private Vector3 previous;
        private ThirdPersonMotor motor;
        private Transform body;
        private Vector3 bodyScale;
        private bool airborne;
        private float landingTime = 1;
        public void Initialize(IslandLifeCatalog catalog)
        {
            foreach (Transform child in transform) if (child.name == "Hero Visual") { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var visual = ModelPresentation.Create(catalog.characterModel, transform, catalog.characterMaterial, 1.30f);
            visual.name = "Hero Visual";
            body = visual.transform; bodyScale = body.localScale;
            animator = visual.GetComponentInChildren<Animator>();
            if (animator == null) animator = visual.transform.GetChild(0).gameObject.AddComponent<Animator>();
            animator.avatar = catalog.characterAvatar;
            animator.runtimeAnimatorController = catalog.animator; animator.applyRootMotion = false;
            previous = transform.position;
            motor = GetComponent<ThirdPersonMotor>();
            body.localPosition = Vector3.down * .07f; // Match the controller contact margin without changing collision or saves.
        }
        private void LateUpdate()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat("Speed", Vector3.ProjectOnPlane(transform.position - previous, Vector3.up).magnitude / Mathf.Max(Time.deltaTime, .001f), .12f, Time.deltaTime);
                animator.SetBool("Airborne", motor != null && motor.CanMove && !motor.Grounded);
                animator.SetFloat("VerticalSpeed", motor != null ? motor.VerticalVelocity : 0);
                bool inAir = motor != null && motor.CanMove && !motor.Grounded;
                if (airborne && !inAir && motor.CanMove) landingTime = 0;
                airborne = inAir; landingTime += Time.deltaTime;
                float compression = motor != null && motor.CanMove && landingTime < .22f ? Mathf.Sin(landingTime / .22f * Mathf.PI) * .045f : 0;
                body.localScale = Vector3.Scale(bodyScale, new Vector3(1 + compression * .5f, 1 - compression, 1 + compression * .5f));
            }
            previous = transform.position;
        }
    }
}
