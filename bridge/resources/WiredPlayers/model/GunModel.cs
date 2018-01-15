using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class GunModel
    {
        public WeaponHash weapon { get; internal set; }
        public String ammunition { get; internal set; }
        public int capacity { get; internal set; }

        public GunModel() { }

        public GunModel(WeaponHash weapon, String ammunition, int capacity)
        {
            this.weapon = weapon;
            this.ammunition = ammunition;
            this.capacity = capacity;
        }
    }
}
