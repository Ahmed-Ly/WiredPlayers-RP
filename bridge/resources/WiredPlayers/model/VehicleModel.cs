using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class VehicleModel
    {
        public int id { get; internal set; }
        public String model { get; internal set; }
        public String owner { get; internal set; }
        public String plate { get; internal set; }
        public Vector3 position { get; internal set; }
        public Vector3 rotation { get; internal set; }
        public int colorType { get; internal set; }
        public String firstColor { get; internal set; }
        public String secondColor { get; internal set; }
        public int pearlescent { get; internal set; }
        public uint dimension { get; internal set; }
        public int faction { get; internal set; }
        public int engine { get; internal set; }
        public int locked { get; internal set; }
        public int price { get; internal set; }
        public int parking { get; internal set; }
        public int parked { get; internal set; }
        public float gas { get; internal set; }
        public float kms { get; internal set; }

        public VehicleModel() { }
    }
}
