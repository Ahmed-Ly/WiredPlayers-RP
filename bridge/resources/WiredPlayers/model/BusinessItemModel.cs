using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class BusinessItemModel
    {
        public String description { get; internal set; }
        public String hash { get; internal set; }
        public int type { get; internal set; }
        public int products { get; internal set; }
        public float weight { get; internal set; }
        public int health { get; internal set; }
        public int uses { get; internal set; }
        public Vector3 position { get; internal set; }
        public Vector3 rotation { get; internal set; }
        public int business { get; internal set; }
        public float alcoholLevel { get; internal set; }

        public BusinessItemModel() { }

        public BusinessItemModel(String description, String hash, int type, int products, float weight, int health, int uses, Vector3 position, Vector3 rotation, int business, float alcoholLevel)
        {
            this.description = description;
            this.hash = hash;
            this.type = type;
            this.products = products;
            this.weight = weight;
            this.health = health;
            this.uses = uses;
            this.position = position;
            this.rotation = rotation;
            this.business = business;
            this.alcoholLevel = alcoholLevel;
        }
    }
}
