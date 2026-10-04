using UnityEngine;
namespace BreathIsland
{
    public static class InteriorCameraFraming
    {
        public static void Pose(RoomLayoutDefinition layout, Vector3 player, bool overview, out Vector3 position, out Vector3 focus)
        {
            if(overview)
            {
                float scale=Mathf.Max(layout.width,layout.depth)/8f;
                position=HousingSystem.InteriorOrigin+new Vector3(layout.width/2f,13*scale,-3*scale);
                focus=HousingSystem.InteriorOrigin+new Vector3(layout.width/2f,0,layout.depth/2f);
                return;
            }
            var local=player-HousingSystem.InteriorOrigin;
            focus=HousingSystem.InteriorOrigin+new Vector3(Mathf.Clamp(local.x,4,Mathf.Max(4,layout.width-4)),0,Mathf.Clamp(local.z+2,3,Mathf.Max(3,layout.depth-3)));
            position=focus+new Vector3(0,10,-7);
        }
    }
}
