using UnityEngine;
namespace BreathIsland
{
    public sealed class GroundCover : MonoBehaviour
    {
        private void Awake()
        {
            var prefab=Resources.Load<GameObject>("World/GroundCover");
            if(prefab!=null)Instantiate(prefab,transform);
            foreach(var renderer in transform.GetComponentsInChildren<Renderer>())
            {
                string material=renderer.name=="Island Grass"?"GrassGround":renderer.name=="Beach"?"SandGround":renderer.name=="Island Rock"?"RockGround":null;
                if(material!=null)renderer.sharedMaterial=Resources.Load<Material>("World/Coastal/"+material);
            }
        }
    }
}
