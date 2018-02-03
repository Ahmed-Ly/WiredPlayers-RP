using GTANetworkAPI;
using System;

namespace WiredPlayers.model
{
    public class CarShopVehicleModel
    {
        public String model { get; set; }
        public VehicleHash hash { get; set; }
        public int carShop { get; set; }
        public int type { get; set; }
        public int speed { get; set; }
        public int price { get; set; }

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
