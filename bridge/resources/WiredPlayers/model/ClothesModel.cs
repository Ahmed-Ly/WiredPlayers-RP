namespace WiredPlayers.model
{
    public class ClothesModel
    {
        public int id { get; internal set; }
        public int player { get; internal set; }
        public int type { get; internal set; }
        public int slot { get; internal set; }
        public int drawable { get; internal set; }
        public int texture { get; internal set; }
        public bool dressed { get; internal set; }

        public ClothesModel() { }
    }
}
