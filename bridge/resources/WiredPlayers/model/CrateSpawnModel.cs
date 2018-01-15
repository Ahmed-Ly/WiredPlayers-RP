using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class CrateSpawnModel
    {
        public int spawnPoint { get; internal set; }
        public Vector3 position { get; internal set; }

        public CrateSpawnModel() { }

        public CrateSpawnModel(int spawnPoint, Vector3 position)
        {
            this.spawnPoint = spawnPoint;
            this.position = position;
        }
    }
}
