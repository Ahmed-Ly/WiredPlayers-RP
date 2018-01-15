using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class ParkingModel
    {
        public int id { get; internal set; }
        public int type { get; internal set; }
        public int houseId { get; internal set; }
        public Vector3 position { get; internal set; }
        public int capacity { get; internal set; }
        public TextLabel parkingLabel { get; internal set; }

        public ParkingModel() { }
    }
}
