using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class ItemModel
    {
        public int id { get; internal set; }
        public String hash { get; internal set; }
        public String ownerEntity { get; internal set; }
        public int ownerIdentifier { get; internal set; }
        public int amount { get; internal set; }
        public Vector3 position { get; internal set; }
        public uint dimension { get; internal set; }
        public NetHandle objectHandle { get; internal set; }

        public ItemModel() { }

        public ItemModel Copy()
        {
            ItemModel itemModel = new ItemModel();
            itemModel.id = id;
            itemModel.hash = hash;
            itemModel.ownerEntity = ownerEntity;
            itemModel.ownerIdentifier = ownerIdentifier;
            itemModel.amount = amount;
            itemModel.position = position;
            itemModel.dimension = dimension;
            itemModel.objectHandle = objectHandle;
            return itemModel;
        }
    }
}
