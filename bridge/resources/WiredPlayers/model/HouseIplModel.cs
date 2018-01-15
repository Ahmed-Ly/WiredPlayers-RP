using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class HouseIplModel
    {
        public String ipl { get; internal set; }
        public Vector3 position { get; internal set; }

        public HouseIplModel() { }

        public HouseIplModel(String ipl, Vector3 position)
        {
            this.ipl = ipl;
            this.position = position;
        }
    }
}
