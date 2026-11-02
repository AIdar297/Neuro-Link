using System;
using System.Collections.Generic;
using System.Text;

namespace Neuro_Link.Engine
{
    internal class Choice
    {
        public string Text { get; set; }
        public string NextScene { get; set; }
        public string SetFlag { get; set; }
        public string RemoveFlag { get; set; }
    }
}
