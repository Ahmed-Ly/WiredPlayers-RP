using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.vehicles;
using System.Collections.Generic;
using System;

namespace WiredPlayers.carshop
{
    public class CarShop : Script
    {
        private TextLabel carShopTextLabel;
        private TextLabel motorbikeShopTextLabel;
        private TextLabel shipShopTextLabel;

        private int GetClosestCarShop(Client player, float distance = 2.0f)
        {
            int carShop = -1;
            if (player.Position.DistanceTo(carShopTextLabel.Position) < distance)
            {
                carShop = 0;
            }
            else if (player.Position.DistanceTo(motorbikeShopTextLabel.Position) < distance)
            {
                carShop = 1;
            }
            else if (player.Position.DistanceTo(shipShopTextLabel.Position) < distance)
            {
                carShop = 2;
            }
            return carShop;
        }

        private List<CarShopVehicleModel> GetVehicleListInCarShop(int carShop)
        {
            List<CarShopVehicleModel> vehicleList = new List<CarShopVehicleModel>();
            foreach (CarShopVehicleModel vehicle in Constants.CARSHOP_VEHICLE_LIST)
            {
                if (vehicle.carShop == carShop)
                {
                    vehicleList.Add(vehicle);
                }
            }
            return vehicleList;
        }

        private int GetVehiclePrice(VehicleHash vehicleHash)
        {
            int price = 0;
            foreach (CarShopVehicleModel vehicle in Constants.CARSHOP_VEHICLE_LIST)
            {
                if (vehicle.hash == vehicleHash)
                {
                    price = vehicle.price;
                    break;
                }
            }
            return price;
        }

        private String GetVehicleModel(VehicleHash vehicleHash)
        {
            String model = String.Empty;
            foreach (CarShopVehicleModel vehicle in Constants.CARSHOP_VEHICLE_LIST)
            {
                if (vehicle.hash == vehicleHash)
                {
                    model = vehicle.model;
                    break;
                }
            }
            return model;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            // Car dealer creation
            carShopTextLabel = NAPI.TextLabel.CreateTextLabel("/" + Messages.COM_CATALOG, new Vector3(-56.88f, -1097.12f, 26.52f), 10.0f, 0.5f, 4, new Color(255, 255, 153));
            TextLabel carShopSubTextLabel = NAPI.TextLabel.CreateTextLabel(Messages.GEN_CATALOG_HELP, new Vector3(-56.88f, -1097.12f, 26.42f), 10.0f, 0.5f, 4, new Color(255, 255, 255));
            Blip carShopBlip = NAPI.Blip.CreateBlip(new Vector3(-56.88f, -1097.12f, 26.52f));
            NAPI.Blip.SetBlipName(carShopBlip, Messages.GEN_CAR_DEALER);
            NAPI.Blip.SetBlipSprite(carShopBlip, 225);

            // Motorcycle dealer creation
            motorbikeShopTextLabel = NAPI.TextLabel.CreateTextLabel("/" + Messages.COM_CATALOG, new Vector3(286.76f, -1148.36f, 29.29f), 10.0f, 0.5f, 4, new Color(255, 255, 153));
            TextLabel motorbikeShopSubTextLabel = NAPI.TextLabel.CreateTextLabel(Messages.GEN_CATALOG_HELP, new Vector3(286.76f, -1148.36f, 29.19f), 10.0f, 0.5f, 4, new Color(255, 255, 255));
            Blip motorbikeShopBlip = NAPI.Blip.CreateBlip(new Vector3(286.76f, -1148.36f, 29.29f));
            NAPI.Blip.SetBlipName(motorbikeShopBlip, Messages.GEN_MOTORCYCLE_DEALER);
            NAPI.Blip.SetBlipSprite(motorbikeShopBlip, 226);

            // Boat dealer creation
            shipShopTextLabel = NAPI.TextLabel.CreateTextLabel("/" + Messages.COM_CATALOG, new Vector3(-711.6249f, -1299.427f, 5.41f), 10.0f, 0.5f, 4, new Color(255, 255, 153));
            TextLabel shipShopSubTextLabel = NAPI.TextLabel.CreateTextLabel(Messages.GEN_CATALOG_HELP, new Vector3(-711.6249f, -1299.427f, 5.31f), 10.0f, 0.5f, 4, new Color(255, 255, 255));
            Blip shipShopBlip = NAPI.Blip.CreateBlip(new Vector3(-711.6249f, -1299.427f, 5.41f));
            NAPI.Blip.SetBlipName(shipShopBlip, Messages.GEN_BOAT_DEALER);
            NAPI.Blip.SetBlipSprite(shipShopBlip, 455);
        }

        [ServerEvent(Event.PlayerEnterCheckpoint)]
        public void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE) && NAPI.Data.HasEntityData(player, EntityData.PLAYER_TESTING_VEHICLE) == true)
            {
                if (NAPI.Player.IsPlayerInAnyVehicle(player) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE) == checkpoint)
                {
                    Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_TESTING_VEHICLE);
                    if (NAPI.Player.GetPlayerVehicle(player) == vehicle)
                    {
                        // We destroy the vehicle and the checkpoint
                        Checkpoint testCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);
                        NAPI.Player.WarpPlayerOutOfVehicle(player);
                        NAPI.Entity.DeleteEntity(testCheckpoint);
                        NAPI.Entity.DeleteEntity(vehicle);

                        // Variable cleaning
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_TESTING_VEHICLE);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE);

                        // Deleting checkpoint
                        NAPI.ClientEvent.TriggerClientEvent(player, "deleteCarshopCheckpoint");
                    }
                }
            }
        }

        [RemoteEvent("purchaseVehicle")]
        public void PurchaseVehicleEvent(Client player, String hash, String firstColor, String secondColor)
        {
            int carShop = GetClosestCarShop(player);
            VehicleHash vehicleHash = (VehicleHash)UInt32.Parse(hash);
            int vehiclePrice = GetVehiclePrice(vehicleHash);
            if (vehiclePrice > 0 && NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK) >= vehiclePrice)
            {
                switch (carShop)
                {
                    case 0:
                        for (int i = 0; i < Constants.CARSHOP_SPAWNS.Count; i++)
                        {
                            bool spawnOccupied = false;
                            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                            {
                                Vector3 vehPos = NAPI.Entity.GetEntityPosition(veh);
                                if (Constants.CARSHOP_SPAWNS[i].DistanceTo(vehPos) < 2.5f)
                                {
                                    spawnOccupied = true;
                                    break;
                                }
                            }
                            if (!spawnOccupied)
                            {
                                // Basic data for vehicle creation
                                VehicleModel vehicleModel = new VehicleModel();
                                vehicleModel.model = GetVehicleModel(vehicleHash);
                                vehicleModel.plate = String.Empty;
                                vehicleModel.position = Constants.CARSHOP_SPAWNS[i];
                                vehicleModel.rotation = new Vector3(0.0, 0.0, 0.0);
                                vehicleModel.owner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_NAME);
                                vehicleModel.colorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM;
                                vehicleModel.firstColor = firstColor;
                                vehicleModel.secondColor = secondColor;
                                vehicleModel.pearlescent = 0;
                                vehicleModel.price = vehiclePrice;
                                vehicleModel.parking = 0;
                                vehicleModel.parked = 0;
                                vehicleModel.engine = 0;
                                vehicleModel.locked = 0;
                                vehicleModel.gas = 50.0f;
                                vehicleModel.kms = 0.0f;

                                // Creating the purchased vehicle
                                Vehicles.CreateVehicle(player, vehicleModel, false);
                                return;
                            }
                        }
                        break;
                    case 1:
                        for (int i = 0; i < Constants.BIKESHOP_SPAWNS.Count; i++)
                        {
                            bool spawnOccupied = false;
                            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                            {
                                Vector3 vehPos = NAPI.Entity.GetEntityPosition(veh);
                                if (Constants.BIKESHOP_SPAWNS[i].DistanceTo(vehPos) < 2.5f)
                                {
                                    spawnOccupied = true;
                                    break;
                                }
                            }
                            if (!spawnOccupied)
                            {
                                // Basic data for vehicle creation
                                VehicleModel vehicleModel = new VehicleModel();
                                vehicleModel.model = GetVehicleModel(vehicleHash);
                                vehicleModel.plate = String.Empty;
                                vehicleModel.position = Constants.BIKESHOP_SPAWNS[i];
                                vehicleModel.rotation = new Vector3(0.0, 0.0, 0.0);
                                vehicleModel.owner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_NAME);
                                vehicleModel.colorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM;
                                vehicleModel.firstColor = firstColor;
                                vehicleModel.secondColor = secondColor;
                                vehicleModel.pearlescent = 0;
                                vehicleModel.price = vehiclePrice;
                                vehicleModel.parking = 0;
                                vehicleModel.parked = 0;
                                vehicleModel.engine = 0;
                                vehicleModel.locked = 0;
                                vehicleModel.gas = 50.0f;
                                vehicleModel.kms = 0.0f;

                                // Creating the purchased vehicle
                                Vehicles.CreateVehicle(player, vehicleModel, false);
                                return;
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < Constants.SHIP_SPAWNS.Count; i++)
                        {
                            bool spawnOccupied = false;
                            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                            {
                                Vector3 vehPos = NAPI.Entity.GetEntityPosition(veh);
                                if (Constants.SHIP_SPAWNS[i].DistanceTo(vehPos) < 2.5f)
                                {
                                    spawnOccupied = true;
                                    break;
                                }
                            }
                            if (!spawnOccupied)
                            {
                                // Basic data for vehicle creation
                                VehicleModel vehicleModel = new VehicleModel();
                                vehicleModel.model = GetVehicleModel(vehicleHash);
                                vehicleModel.plate = String.Empty;
                                vehicleModel.position = Constants.SHIP_SPAWNS[i];
                                vehicleModel.rotation = new Vector3(0.0, 0.0, 0.0);
                                vehicleModel.owner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_NAME);
                                vehicleModel.colorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM;
                                vehicleModel.firstColor = firstColor;
                                vehicleModel.secondColor = secondColor;
                                vehicleModel.pearlescent = 0;
                                vehicleModel.price = vehiclePrice;
                                vehicleModel.parking = 0;
                                vehicleModel.parked = 0;
                                vehicleModel.engine = 0;
                                vehicleModel.locked = 0;
                                vehicleModel.gas = 50.0f;
                                vehicleModel.kms = 0.0f;

                                // Creating the purchased vehicle
                                Vehicles.CreateVehicle(player, vehicleModel, false);
                                return;
                            }
                        }
                        break;
                }

                // Parking places are occupied
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CARSHOP_SPAWN_OCCUPIED);
            }
            else
            {
                String message = String.Format(Messages.ERR_CARSHOP_NO_MONEY, vehiclePrice);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
            }
        }

        [RemoteEvent("testVehicle")]
        public void TestVehicleEvent(Client player, String hash)
        {
            Vehicle vehicle = null;
            Checkpoint testFinishCheckpoint = null;
            VehicleHash vehicleModel = (VehicleHash)UInt32.Parse(hash);

            switch (GetClosestCarShop(player))
            {
                case 0:
                    vehicle = NAPI.Vehicle.CreateVehicle(vehicleModel, new Vector3(-51.54087f, -1076.941f, 26.94754f), 75.0f, new Color(0, 0, 0), new Color(0, 0, 0));
                    testFinishCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, new Vector3(-28.933f, -1085.566f, 25.565f), new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                    break;
                case 1:
                    vehicle = NAPI.Vehicle.CreateVehicle(vehicleModel, new Vector3(307.0036f, -1162.707f, 29.29191f), 180.0f, new Color(0, 0, 0), new Color(0, 0, 0));
                    testFinishCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, new Vector3(267.412f, -1159.755f, 28.263f), new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                    break;
                case 2:
                    vehicle = NAPI.Vehicle.CreateVehicle(vehicleModel, new Vector3(-717.3467f, -1319.792f, -0.42f), 180.0f, new Color(0, 0, 0), new Color(0, 0, 0));
                    testFinishCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, new Vector3(-711.267f, -1351.501f, -1.359f), new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                    break;
            }

            // Vehicle variable initialization
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_KMS, 0.0f);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_GAS, 50.0f);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_TESTING, true);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_TESTING_VEHICLE, vehicle);
            NAPI.Player.SetPlayerIntoVehicle(player, vehicle, (int)VehicleSeat.Driver);
            NAPI.Vehicle.SetVehicleEngineStatus(vehicle, true);

            // Adding the checkpoint
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRIVING_COLSHAPE, testFinishCheckpoint);
            NAPI.ClientEvent.TriggerClientEvent(player, "showCarshopCheckpoint", testFinishCheckpoint.Position);

            // Confirmation message sent to the player
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_TEST_VEHICLE);
        }

        [Command(Messages.COM_CATALOG)]
        public void CatalogoCommand(Client player)
        {
            int carShop = GetClosestCarShop(player);
            if (carShop > -1)
            {
                // We get the vehicle list
                List<CarShopVehicleModel> carList = GetVehicleListInCarShop(carShop);

                // Getting the speed for each vehicle in the list
                foreach (CarShopVehicleModel carShopVehicle in carList)
                {
                    VehicleHash vehicleHash = NAPI.Util.VehicleNameToModel(carShopVehicle.model);
                    carShopVehicle.speed = (int)Math.Round(NAPI.Vehicle.GetVehicleMaxSpeed(vehicleHash) * 3.6f);
                }

                // We show the catalog
                NAPI.ClientEvent.TriggerClientEvent(player, "showVehicleCatalog", NAPI.Util.ToJson(carList), carShop);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_CARSHOP);
            }
        }
    }
}
