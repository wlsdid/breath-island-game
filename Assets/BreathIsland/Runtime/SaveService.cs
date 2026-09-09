using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BreathIsland
{
    public sealed class SaveService
    {
        private readonly string directory;
        public SaveService(string root = null)
        {
            directory = string.IsNullOrWhiteSpace(root) ? Path.Combine(Application.persistentDataPath, "Saves") : root;
            Directory.CreateDirectory(directory);
        }

        public string PathForSlot(int slot) => Path.Combine(directory, slot < 0 ? "autosave.json" : $"save_{Mathf.Clamp(slot, 0, 2)}.json");

        public void Save(GameSaveData data, int slot = -1)
        {
            data.version = GameSaveData.CurrentVersion;
            data.savedAtUtcTicks = DateTime.UtcNow.Ticks;
            string destination = PathForSlot(slot);
            string temporary = destination + ".tmp";
            string backup = destination + ".bak";
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true), new UTF8Encoding(false));
            try
            {
                if (File.Exists(destination)) File.Replace(temporary, destination, backup, true);
                else File.Move(temporary, destination);
            }
            catch
            {
                if (File.Exists(destination)) File.Copy(destination, backup, true);
                File.Copy(temporary, destination, true);
                File.Delete(temporary);
            }
        }

        public GameSaveData Load(int slot = -1)
        {
            string path = PathForSlot(slot);
            if (!File.Exists(path)) return GameSaveData.CreateDefault();
            try
            {
                var data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path, Encoding.UTF8));
                return Migrate(data);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Save load failed: {exception.Message}");
                string backup = path + ".bak";
                if (File.Exists(backup))
                {
                    try { return Migrate(JsonUtility.FromJson<GameSaveData>(File.ReadAllText(backup, Encoding.UTF8))); }
                    catch { /* return a safe new game */ }
                }
                return GameSaveData.CreateDefault();
            }
        }

        private static GameSaveData Migrate(GameSaveData data)
        {
            if (data == null) return GameSaveData.CreateDefault();
            data.player ??= new PlayerProgress();
            data.inventory ??= new InventoryState();
            data.relationships ??= new RelationshipState();
            data.facilities ??= new FacilityState();
            data.world ??= new WorldState();
            data.inventory.discoveredFish ??= new System.Collections.Generic.List<string>();
            data.inventory.displayedFish ??= new System.Collections.Generic.List<string>();
            data.version = GameSaveData.CurrentVersion;
            return data;
        }
    }
}
