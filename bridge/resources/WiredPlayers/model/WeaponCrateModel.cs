using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class WeaponCrateModel
    {
        public String contentItem { get; internal set; }
        public int contentAmount { get; internal set; }
        public Vector3 position { get; internal set; }
        public String carriedEntity { get; internal set; }
        public int carriedIdentifier { get; internal set; }
        public GTANetworkAPI.Object crateObject { get; internal set; }

        public WeaponCrateModel() { }
    }
}
