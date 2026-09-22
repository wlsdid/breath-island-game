using UnityEngine;
namespace BreathIsland
{
    public sealed class CharacterPresentation : MonoBehaviour
    {
        private Animator animator;
        private Vector3 previous;
        public void Initialize(IslandLifeCatalog catalog)
        {
            foreach (Transform child in transform) if (child.name == "Hero Visual") { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var visual = ModelPresentation.Create(catalog.characterModel, transform, catalog.characterMaterial, 1.7f);
            visual.name = "Hero Visual";
            animator = visual.GetComponentInChildren<Animator>();
            if (animator == null) animator = visual.transform.GetChild(0).gameObject.AddComponent<Animator>();
            animator.avatar = catalog.characterAvatar;
            animator.runtimeAnimatorController = catalog.animator; animator.applyRootMotion = false;
            previous = transform.position;
        }
        private void LateUpdate()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
                animator.SetFloat("Speed", Vector3.Distance(previous, transform.position) / Mathf.Max(Time.deltaTime, .001f));
            previous = transform.position;
        }
    }
}
