using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class BusinessModel
    {
        public int id { get; internal set; }
        public int type { get; internal set; }
        public String ipl { get; internal set; }
        public String name { get; internal set; }
        public Vector3 position { get; internal set; }
        public uint dimension { get; internal set; }
        public String owner { get; internal set; }
        public int funds { get; internal set; }
        public int products { get; internal set; }
        public float multiplier { get; internal set; }
        public bool locked { get; internal set; }
        public TextLabel businessLabel { get; internal set; }

        public BusinessModel() { }
    }
}
