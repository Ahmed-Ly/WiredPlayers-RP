using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class GarbageModel
    {
        public int route { get; internal set; }
        public int checkPoint { get; internal set; }
        public Vector3 position { get; internal set; }

        public GarbageModel() { }

        public GarbageModel(int route, int checkPoint, Vector3 position)
        {
            this.route = route;
            this.checkPoint = checkPoint;
            this.position = position;
        }
    }
}