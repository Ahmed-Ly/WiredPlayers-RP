using GTANetworkAPI;

namespace WiredPlayers.model
{
    public class DeathModel
    {
        public Client player { get; internal set; }
        public NetHandle entityKiller { get; internal set; }
        public uint weapon { get; internal set; }

        public DeathModel() { }

        public DeathModel(Client player, NetHandle entityKiller, uint weapon)
        {
            this.player = player;
            this.entityKiller = entityKiller;
            this.weapon = weapon;
        }
    }
}
