using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class ReinforcesModel
    {
        public int playerId { get; internal set; }
        public Vector3 position { get; internal set; }

        public ReinforcesModel() { }

        public ReinforcesModel(int playerId, Vector3 position)
        {
            this.playerId = playerId;
            this.position = position;
        }
    }
}
