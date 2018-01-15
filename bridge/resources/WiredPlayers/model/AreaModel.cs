using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class AreaModel
    {
        public String action { get; internal set; }
        public ColShape area { get; internal set; }

        public AreaModel() { }

        public AreaModel(String action, ColShape area)
        {
            this.action = action;
            this.area = area;
        }
    }
}