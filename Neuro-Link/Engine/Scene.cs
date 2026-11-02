using System;
using System.Collections.Generic;
using System.Text;

namespace Neuro_Link.Engine
{
    public enum SceneType { DIALOGUE, QTE, COMBAT, HACK, ENDING }

    public class Scene
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public Dialogue Dialogue { get; set; }
        public List<Choice> Choices { get; set; }
        public SceneType Type { get; set; } = SceneType.DIALOGUE;
        public string SuccessScene { get; set; }
        public string FailScene { get; set; }
        public int EndingId { get; set; }
    }
}
