using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class PlayerModel
    {
        public int id { get; internal set; }
        public String realName { get; internal set; }
        public int adminRank { get; internal set; }
        public String adminName { get; internal set; }
        public Vector3 position { get; internal set; }
        public Vector3 rotation { get; internal set; }
        public int money { get; internal set; }
        public int bank { get; internal set; }
        public int health { get; internal set; }
        public int armor { get; internal set; }
        public int age { get; internal set; }
        public int sex { get; internal set; }
        public int faction { get; internal set; }
        public int job { get; internal set; }
        public int rank { get; internal set; }
        public int duty { get; internal set; }
        public int phone { get; internal set; }
        public int radio { get; internal set; }
        public int killed { get; internal set; }
        public String jailed { get; internal set; }
        public String carKeys { get; internal set; }
        public int documentation { get; internal set; }
        public String licenses { get; internal set; }
        public int insurance { get; internal set; }
        public int weaponLicense { get; internal set; }
        public int status { get; internal set; }
        public int played { get; internal set; }
        public int houseRent { get; internal set; }
        public int houseEntered { get; internal set; }
        public int businessEntered { get; internal set; }
        public int employeeCooldown { get; internal set; }
        public int jobCooldown { get; internal set; }
        public int jobDeliver { get; internal set; }
        public String jobPoints { get; internal set; }

        public PlayerModel() { }
    }
}
