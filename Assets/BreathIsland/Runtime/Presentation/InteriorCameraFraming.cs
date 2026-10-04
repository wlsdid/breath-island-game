using UnityEngine;
namespace BreathIsland
{
    public static class InteriorCameraFraming
    {
        public static void Pose(RoomLayoutDefinition layout, Vector3 player, bool overview, out Vector3 position, out Vector3 focus,HomeStructureState structure=null)
        {
            var size=structure?.customized==true?HomeFootprint.Size(structure):new Vector2(layout.width,layout.depth);
            if(overview)
            {
                float scale=Mathf.Max(size.x,size.y)/8f;
                position=HousingSystem.InteriorOrigin+new Vector3(size.x/2f,13*scale,-3*scale);
                focus=HousingSystem.InteriorOrigin+new Vector3(size.x/2f,0,size.y/2f);
                return;
            }
            var local=player-HousingSystem.InteriorOrigin;
            focus=HousingSystem.InteriorOrigin+new Vector3(Mathf.Clamp(local.x,4,Mathf.Max(4,size.x-4)),0,Mathf.Clamp(local.z+2,3,Mathf.Max(3,size.y-3)));
            position=focus+new Vector3(0,10,-7);
        }
    }
}
