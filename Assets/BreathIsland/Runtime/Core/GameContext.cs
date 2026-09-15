using UnityEngine;

namespace BreathIsland
{
    public sealed class GameContext
    {
        private readonly SaveService saveService;
        private readonly WorldHandles world;
        private readonly System.Action<string> showMessage;

        public GameContext(GameSaveData save, SaveService saveService, WorldHandles world, DefinitionCatalog content, System.Action<string> showMessage)
        {
            Save = save;
            this.saveService = saveService;
            this.world = world;
            Content = content;
            this.showMessage = showMessage;
            Positions = new PlayerPositionSystem(world);
        }

        public GameSaveData Save { get; private set; }
        public DefinitionCatalog Content { get; }
        public WorldHandles World => world;
        public PlayerPositionSystem Positions { get; }
        public bool Korean { get; set; } = true;

        public string Text(string key) => GameText.Get(key, Korean);
        public void Message(string text) => showMessage(text);

        public void SaveTo(int slot = -1)
        {
            CapturePosition();
            saveService.Save(Save, slot);
        }

        public void LoadFrom(int slot = -1)
        {
            Save = saveService.Load(slot);
            RestorePosition();
        }

        public void CapturePosition()
        {
            Vector3 position = world.player.transform.position;
            Save.player.x = position.x;
            Save.player.y = position.y;
            Save.player.z = position.z;
        }

        public void RestorePosition()
        {
            Positions.Restore(Save.player);
        }
    }
}
