using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class GunModel
    {
        public WeaponHash weapon { get; set; }
        public String ammunition { get; set; }
        public int capacity { get; set; }

        public GunModel(WeaponHash weapon, String ammunition, int capacity)
        {
            this.weapon = weapon;
            this.ammunition = ammunition;
            this.capacity = capacity;
        }
    }
}
