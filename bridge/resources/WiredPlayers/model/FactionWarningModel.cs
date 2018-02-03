using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class FactionWarningModel
    {
        public int faction { get; set; }
        public int playerId { get; set; }
        public String place { get; set; }
        public Vector3 position { get; set; }
        public int takenBy { get; set; }
        public String hour { get; set; }

        public FactionWarningModel(int faction, int playerId, String place, Vector3 position, int takenBy, String hour)
        {
            this.faction = faction;
            this.playerId = playerId;
            this.place = place;
            this.position = position;
            this.takenBy = takenBy;
            this.hour = hour;
        }
    }
}
