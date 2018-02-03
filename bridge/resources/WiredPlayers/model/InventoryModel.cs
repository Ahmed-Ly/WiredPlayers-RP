using System;

namespace WiredPlayers.model
{
    public class InventoryModel
    {
        public int id { get; set; }
        public String hash { get; set; }
        public String description { get; set; }
        public int type { get; set; }
        public int amount { get; set; }
    }
}
