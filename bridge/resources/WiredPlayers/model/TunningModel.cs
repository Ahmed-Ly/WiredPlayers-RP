namespace WiredPlayers.model
{
    public class TunningModel
    {
        public int id { get; internal set; }
        public int vehicle { get; internal set; }
        public int slot { get; internal set; }
        public int component { get; internal set; }

        public TunningModel() { }
    }
}
