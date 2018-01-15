using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class FactionWarningModel
    {
        public int faction { get; internal set; }
        public int playerId { get; internal set; }
        public String place { get; internal set; }
        public Vector3 position { get; internal set; }
        public int takenBy { get; internal set; }
        public String hour { get; internal set; }

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
