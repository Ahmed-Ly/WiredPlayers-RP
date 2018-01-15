using System;

namespace WiredPlayers.model
{
    public class TattooModel
    {
        public int player { get; internal set; }
        public int slot { get; internal set; }
        public String library { get; internal set; }
        public String hash { get; internal set; }

        public TattooModel() { }
    }
}
