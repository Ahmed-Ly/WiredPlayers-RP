using System;

namespace WiredPlayers.model
{
    public class BusinessTattooModel
    {
        public int slot { get; internal set; }
        public String name { get; internal set; }
        public String library { get; internal set; }
        public String maleHash { get; internal set; }
        public String femaleHash { get; internal set; }
        public int price { get; internal set; }

        public BusinessTattooModel() { }

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
