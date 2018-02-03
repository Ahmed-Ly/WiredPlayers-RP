using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class HouseIplModel
    {
        public String ipl { get; set; }
        public Vector3 position { get; set; }

        public HouseIplModel(String ipl, Vector3 position)
        {
            this.ipl = ipl;
            this.position = position;
        }
    }
}
