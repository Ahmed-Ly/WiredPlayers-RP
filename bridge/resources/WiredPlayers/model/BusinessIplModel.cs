using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class BusinessIplModel
    {
        public int type { get; internal set; }
        public String ipl { get; internal set; }
        public Vector3 position { get; internal set; }

        public BusinessIplModel() { }

        public BusinessIplModel(int type, String ipl, Vector3 position)
        {
            this.type = type;
            this.ipl = ipl;
            this.position = position;
        }
    }
}
