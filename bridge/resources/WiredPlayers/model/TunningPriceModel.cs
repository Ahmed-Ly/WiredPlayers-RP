namespace WiredPlayers.model
{
    public class TunningPriceModel
    {
        public int slot { get; internal set; }
        public int products { get; internal set; }

        public TunningPriceModel() { }

        public TunningPriceModel(int slot, int products)
        {
            this.slot = slot;
            this.products = products;
        }
    }
}
