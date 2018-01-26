using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class DeathModel
    {
        public Client player { get; internal set; }
        public Client killer { get; internal set; }
        public uint weapon { get; internal set; }

        public DeathModel() { }

        public DeathModel(Client player, Client killer, uint weapon)
        {
            this.player = player;
            this.killer = killer;
            this.weapon = weapon;
        }
    }
}
