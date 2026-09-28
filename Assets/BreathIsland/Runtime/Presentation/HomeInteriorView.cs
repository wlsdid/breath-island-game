using UnityEngine;
namespace BreathIsland
{
    public static class HomeInteriorView
    {
        public static Transform Create(Transform parent)
        {
            return Place("HomeInterior", HousingSystem.InteriorOrigin, parent);
        }
        private static Transform Place(string name, Vector3 position, Transform parent)
        {
            var prefab = Resources.Load<GameObject>("World/" + name);
            if (prefab == null) throw new System.InvalidOperationException("Missing authored house prefab: " + name);
            return Object.Instantiate(prefab, position, Quaternion.identity, parent).transform;
        }
    }
}
