using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class HouseModel
    {
        public int id { get; internal set; }
        public String ipl { get; internal set; }
        public String name { get; internal set; }
        public Vector3 position { get; internal set; }
        public uint dimension { get; internal set; }
        public int price { get; internal set; }
        public String owner { get; internal set; }
        public int status { get; internal set; }
        public int tenants { get; internal set; }
        public int rental { get; internal set; }
        public bool locked { get; internal set; }
        public TextLabel houseLabel { get; internal set; }

        public HouseModel() { }
    }
}
