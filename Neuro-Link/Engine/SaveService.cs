using Neuro_Link.Engine;
using System.IO;
using System.Text.Json;

namespace Neuro_Link.Engine
{
    public static class SaveService
    {
        private const string Path = "savegame.json";
        public static void Save(GameState state)
        {
            File.WriteAllText(Path, JsonSerializer.Serialize(state));
        }
        public static GameState Load()
        {
            if (!File.Exists(Path)) return null;
            return JsonSerializer.Deserialize<GameState>(File.ReadAllText(Path));
        }
    }
}