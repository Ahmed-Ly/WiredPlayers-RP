using System;

namespace WiredPlayers.model
{
    public class BloodModel
    {
        public int id { get; internal set; }
        public int doctor { get; internal set; }
        public int patient { get; internal set; }
        public String type { get; internal set; }
        public bool used { get; internal set; }

        public BloodModel() { }
    }
}
