using System;
using System.Collections.Generic;
using System.Text;

namespace Neuro_Link.Engine
{
    internal class GameState
    {
        public string CurrentSceneId { get; set; } = "intro";
        public Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();
    }
}
