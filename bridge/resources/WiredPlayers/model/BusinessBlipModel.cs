namespace WiredPlayers.model
{
    public class BusinessBlipModel
    {
        public int id { get; internal set; }
        public int blip { get; internal set; }

        public BusinessBlipModel() { }

        public BusinessBlipModel(int id, int blip)
        {
            this.id = id;
            this.blip = blip;
        }
    }
}
