using System;

namespace WiredPlayers.model
{
    public class ScoreModel
    {
        public int playerId { get; internal set; }
        public string playerName { get; internal set; }
        public int playerPing { get; internal set; }

        public ScoreModel() { }

        public ScoreModel(int playerId, String playerName, int playerPing)
        {
            this.playerId = playerId;
            this.playerName = playerName;
            this.playerPing = playerPing;
        }
    }
}
