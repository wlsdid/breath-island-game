using UnityEngine;

namespace BreathIsland
{
    // Restores valid saves without moving them between screens; repairs only fallen positions.
    public sealed class PlayerPositionSystem
    {
        private static readonly Vector3 Shore = new(0f, 1.2f, 8f);
        private readonly WorldHandles world;
        public PlayerPositionSystem(WorldHandles world) => this.world = world;

        public void Restore(PlayerProgress player)
        {
            var position = new Vector3(player.x, player.y, player.z);
            if (position.y < -5f) position = Shore;
            else position.y = Mathf.Max(1.2f, position.y);
            if (world.village != null) position = world.village.RestoreSafePosition(position);
            world.motor.Teleport(position);
        }

        public void Tick()
        {
            if (world.player.transform.position.y < -5f) ReturnToShore();
        }

        public void ReturnToShore() => world.motor.Teleport(Shore);
    }
}
