using System;

namespace WiredPlayers.model
{
    public class AnnoucementModel
    {
        public int id { get; internal set; }
        public int winner { get; internal set; }
        public int journalist { get; internal set; }
        public int amount { get; internal set; }
        public String annoucement { get; internal set; }
        public bool given { get; internal set; }

        public AnnoucementModel() { }
    }
}
