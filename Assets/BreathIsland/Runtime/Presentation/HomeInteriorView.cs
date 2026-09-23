using UnityEngine;
namespace BreathIsland
{
    public static class HomeInteriorView
    {
        public static void Create(Transform parent)
        {
            Place("HomeInterior", HousingSystem.InteriorOrigin, parent);
            Place("HomeDoorway", HousingSystem.Entrance, parent);
        }
        private static void Place(string name, Vector3 position, Transform parent)
        {
            var prefab = Resources.Load<GameObject>("World/" + name);
            if (prefab == null) throw new System.InvalidOperationException("Missing authored house prefab: " + name);
            Object.Instantiate(prefab, position, Quaternion.identity, parent);
        }
    }
}
