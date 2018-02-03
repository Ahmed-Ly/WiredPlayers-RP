using System;

namespace WiredPlayers.model
{
    public class BusinessTattooModel
    {
        public int slot { get; set; }
        public String name { get; set; }
        public String library { get; set; }
        public String maleHash { get; set; }
        public String femaleHash { get; set; }
        public int price { get; set; }

        public BusinessTattooModel(int slot, String name, String library, String maleHash, String femaleHash, int price)
        {
            this.slot = slot;
            this.name = name;
            this.library = library;
            this.maleHash = maleHash;
            this.femaleHash = femaleHash;
            this.price = price;
        }
    }
}
