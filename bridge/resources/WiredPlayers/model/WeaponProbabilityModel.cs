using System;

namespace WiredPlayers.model
{
    public class WeaponProbabilityModel
    {
        public int type { get; internal set; }
        public String hash { get; internal set; }
        public int amount { get; internal set; }
        public int minChance { get; internal set; }
        public int maxChance { get; internal set; }

        public WeaponProbabilityModel() { }

        public WeaponProbabilityModel(int type, String hash, int amount, int minChance, int maxChance)
        {
            this.type = type;
            this.hash = hash;
            this.amount = amount;
            this.minChance = minChance;
            this.maxChance = maxChance;
        }
    }
}
