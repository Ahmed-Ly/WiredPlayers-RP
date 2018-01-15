using System;

namespace WiredPlayers.model
{
    public class CrateContentModel
    {
        public String item { get; internal set; }
        public int amount { get; internal set; }
        public int chance { get; internal set; }

        public CrateContentModel() { }
    }
}
