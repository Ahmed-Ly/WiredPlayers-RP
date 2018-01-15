using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class FurnitureModel
    {
        public int id { get; internal set; }
        public uint hash { get; internal set; }
        public uint house { get; internal set; }
        public Vector3 position { get; internal set; }
        public Vector3 rotation { get; internal set; }
        public NetHandle handle { get; internal set; }

        public FurnitureModel() { }
    }
}
