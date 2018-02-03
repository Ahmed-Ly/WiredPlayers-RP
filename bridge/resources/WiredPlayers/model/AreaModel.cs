using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class AreaModel
    {
        public String action { get; set; }
        public ColShape area { get; set; }

        public AreaModel(String action, ColShape area)
        {
            this.action = action;
            this.area = area;
        }
    }
}