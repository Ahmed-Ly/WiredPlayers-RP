using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class CarShopVehicleModel
    {
        public String model { get; internal set; }
        public VehicleHash hash { get; internal set; }
        public int carShop { get; internal set; }
        public int type { get; internal set; }
        public int speed { get; internal set; }
        public int price { get; internal set; }

        public CarShopVehicleModel() { }

        public CarShopVehicleModel(String model, VehicleHash hash, int carShop, int type, int price)
        {
            this.model = model;
            this.hash = hash;
            this.carShop = carShop;
            this.type = type;
            this.price = price;
        }
    }
}
