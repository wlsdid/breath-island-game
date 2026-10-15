using UnityEngine;

namespace BreathIsland
{
    public sealed class FishingCatchView : MonoBehaviour
    {
        public string[] fishIds;
        public GameObject[] models;
        private string current;
        public void Show(string id)
        {
            if (current == id) return;
            current = id;
            for (int i = 0; i < models.Length; i++) models[i].SetActive(i < fishIds.Length && fishIds[i] == id);
        }
    }
}
