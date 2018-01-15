using System;

namespace WiredPlayers.model
{
    public class InventoryModel
    {
        public int id { get; internal set; }
        public String hash { get; internal set; }
        public String description { get; internal set; }
        public int type { get; internal set; }
        public int amount { get; internal set; }

        public InventoryModel() { }
    }
}
