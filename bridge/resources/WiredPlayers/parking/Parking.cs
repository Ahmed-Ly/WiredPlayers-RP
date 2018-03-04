using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.vehicles;
using WiredPlayers.mechanic;
using WiredPlayers.house;
using System.Collections.Generic;
using System;

namespace WiredPlayers.parking
{
    public class Parking : Script
    {
        public static List<ParkingModel> parkingList;
        public static List<ParkedCarModel> parkedCars;

        public void LoadDatabaseParkings()
        {
            parkingList = Database.LoadAllParkings();
            foreach (ParkingModel parking in parkingList)
            {
                String parkingLabelText = GetParkingLabelText(parking.type);
                parking.parkingLabel = NAPI.TextLabel.CreateTextLabel(parkingLabelText, parking.position, 30.0f, 0.75f, 4, new Color(255, 255, 255));
            }
        }

        public static ParkingModel GetClosestParking(Client player, float distance = 1.5f)
        {
            ParkingModel parking = null;
            foreach (ParkingModel parkingModel in parkingList)
            {
                if (parkingModel.position.DistanceTo(player.Position) < distance)
                {
                    distance = parkingModel.position.DistanceTo(player.Position);
                    parking = parkingModel;
                }
            }
            return parking;
        }

        public static int GetParkedCarAmount(ParkingModel parking)
        {
            int totalVehicles = 0;
            foreach (ParkedCarModel parkedCar in parkedCars)
            {
                if (parkedCar.parkingId == parking.id)
                {
                    totalVehicles++;
                }
            }
            return totalVehicles;
        }

        public static String GetParkingLabelText(int type)
        {
            String labelText = String.Empty;
            switch (type)
            {
                case Constants.PARKING_TYPE_PUBLIC:
                    labelText = Messages.GEN_PUBLIC_PARKING;
                    break;
                case Constants.PARKING_TYPE_GARAGE:
                    labelText = Messages.GEN_GARAGE;
                    break;
                case Constants.PARKING_TYPE_SCRAPYARD:
                    labelText = Messages.GEN_SCRAPYARD;
                    break;
                case Constants.PARKING_TYPE_DEPOSIT:
                    labelText = Messages.GEN_POLICE_DEPOT;
                    break;
            }
            return labelText;
        }

        public static ParkingModel GetParkingById(int parkingId)
        {
            ParkingModel parking = null;
            foreach (ParkingModel parkingModel in parkingList)
            {
                if (parkingModel.id == parkingId)
                {
                    parking = parkingModel;
                    break;
                }
            }
            return parking;
        }

        private static ParkedCarModel GetParkedVehicle(int vehicleId)
        {
            ParkedCarModel vehicle = null;
            foreach (ParkedCarModel parkedCar in parkedCars)
            {
                if (parkedCar.vehicle.id == vehicleId)
                {
                    vehicle = parkedCar;
                    break;
                }
            }
            return vehicle;
        }

        private void PlayerParkVehicle(Client player, ParkingModel parking)
        {
            NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);

            // Get vehicle's colors
            Color primaryColor = NAPI.Vehicle.GetVehicleCustomPrimaryColor(vehicle);
            Color secondaryColor = NAPI.Vehicle.GetVehicleCustomSecondaryColor(vehicle);

            // Get vehicle data
            VehicleModel vehicleModel = new VehicleModel();
            vehicleModel.rotation = NAPI.Entity.GetEntityRotation(vehicle);
            vehicleModel.id = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
            vehicleModel.model = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
            vehicleModel.colorType = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE);
            vehicleModel.firstColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR);
            vehicleModel.secondColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR);
            vehicleModel.pearlescent = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR);
            vehicleModel.faction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);
            vehicleModel.plate = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PLATE);
            vehicleModel.owner = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER);
            vehicleModel.price = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PRICE);
            vehicleModel.gas = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_GAS);
            vehicleModel.kms = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_KMS);

            // Update parking values
            vehicleModel.position = parking.position;
            vehicleModel.dimension = Convert.ToUInt32(parking.id);
            vehicleModel.parking = parking.id;
            vehicleModel.parked = 0;

            // Link vehicle to the parking
            ParkedCarModel parkedCarModel = new ParkedCarModel();
            parkedCarModel.vehicle = vehicleModel;
            parkedCarModel.parkingId = parking.id;
            parkedCars.Add(parkedCarModel);

            // Save the vehicle and delete it from the game
            NAPI.Player.WarpPlayerOutOfVehicle(player);
            NAPI.Entity.DeleteEntity(vehicle);
            Database.SaveVehicle(vehicleModel);
        }

        [Command(Messages.COM_PARK)]
        public void ParkCommand(Client player)
        {
            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.Driver)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
            }
            else if (NAPI.Data.GetEntityData(NAPI.Player.GetPlayerVehicle(player), EntityData.VEHICLE_FACTION) != Constants.FACTION_NONE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_FACTION_PARK);
            }
            else
            {
                Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
                if (Vehicles.HasPlayerVehicleKeys(player, vehicle) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                }
                else
                {
                    foreach (ParkingModel parking in parkingList)
                    {
                        if (player.Position.DistanceTo(parking.position) < 3.5f)
                        {
                            switch (parking.type)
                            {
                                case Constants.PARKING_TYPE_PUBLIC:
                                    String message = String.Format(Messages.INF_PARKING_COST, Constants.PRICE_PARKING_PUBLIC);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                                    PlayerParkVehicle(player, parking);
                                    break;
                                case Constants.PARKING_TYPE_GARAGE:
                                    HouseModel house = House.GetHouseById(parking.houseId);
                                    if (house == null || House.HasPlayerHouseKeys(player, house) == false)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_GARAGE_ACCESS);
                                    }
                                    else if (GetParkedCarAmount(parking) == parking.capacity)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PARKING_FULL);
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_GARAGE_PARKED);
                                        PlayerParkVehicle(player, parking);
                                    }
                                    break;
                                case Constants.PARKING_TYPE_DEPOSIT:
                                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_DEPOSIT_PARKED);
                                        PlayerParkVehicle(player, parking);
                                    }
                                    break;
                                default:
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PARKING_ALLOWED);
                                    break;
                            }
                            return;
                        }
                    }

                    // There's no parking near
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PARKING_NEAR);
                }
            }
        }

        [Command(Messages.COM_UNPARK, Messages.GEN_UNPARK_COMMAND)]
        public void UnparkCommand(Client player, int vehicleId)
        {
            VehicleModel vehicle = Vehicles.GetParkedVehicleById(vehicleId);

            if (vehicle == null)
            {
                // There's no vehicle with that identifier
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
            }
            else if (Vehicles.HasPlayerVehicleKeys(player, vehicle) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
            }
            else
            {
                foreach (ParkingModel parking in parkingList)
                {
                    if (player.Position.DistanceTo(parking.position) < 2.5f)
                    {
                        // Check whether the vehicle is in this parking
                        if (parking.id == vehicle.parking)
                        {
                            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                            
                            switch (parking.type)
                            {
                                case Constants.PARKING_TYPE_PUBLIC:
                                    break;
                                case Constants.PARKING_TYPE_SCRAPYARD:
                                    break;
                                case Constants.PARKING_TYPE_DEPOSIT:
                                    // Remove player's money
                                    if (playerMoney >= Constants.PRICE_PARKING_DEPOSIT)
                                    {
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - Constants.PRICE_PARKING_DEPOSIT);
                                        
                                        String message = String.Format(Messages.INF_UNPARK_MONEY, Constants.PRICE_PARKING_DEPOSIT);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                                    }
                                    else
                                    {
                                        String message = String.Format(Messages.ERR_PARKING_NOT_MONEY, Constants.PRICE_PARKING_DEPOSIT);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                        return;
                                    }
                                    break;
                            }

                            // Get parked vehicle model
                            ParkedCarModel parkedCar = GetParkedVehicle(vehicleId);

                            // Recreate the vehicle
                            Vehicle newVehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.VehicleNameToModel(vehicle.model), parking.position, vehicle.rotation.Z, new Color(0, 0, 0), new Color(0, 0, 0));
                            NAPI.Vehicle.SetVehicleNumberPlate(newVehicle, vehicle.plate == String.Empty ? "LS " + (1000 + vehicle.id) : vehicle.plate);
                            NAPI.Vehicle.SetVehicleEngineStatus(newVehicle, false);
                            NAPI.Vehicle.SetVehicleLocked(newVehicle, false);
                            
                            if (vehicle.colorType == Constants.VEHICLE_COLOR_TYPE_PREDEFINED)
                            {
                                NAPI.Vehicle.SetVehiclePrimaryColor(newVehicle, Int32.Parse(vehicle.firstColor));
                                NAPI.Vehicle.SetVehicleSecondaryColor(newVehicle, Int32.Parse(vehicle.secondColor));
                                NAPI.Vehicle.SetVehiclePearlescentColor(newVehicle, vehicle.pearlescent);
                            }
                            else
                            {
                                String[] firstColor = vehicle.firstColor.Split(',');
                                String[] secondColor = vehicle.secondColor.Split(',');
                                NAPI.Vehicle.SetVehicleCustomPrimaryColor(newVehicle, Int32.Parse(firstColor[0]), Int32.Parse(firstColor[1]), Int32.Parse(firstColor[2]));
                                NAPI.Vehicle.SetVehicleCustomSecondaryColor(newVehicle, Int32.Parse(secondColor[0]), Int32.Parse(secondColor[1]), Int32.Parse(secondColor[2]));
                            }

                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_ID, vehicle.id);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_MODEL, vehicle.model);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_POSITION, parking.position);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_ROTATION, vehicle.rotation);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_COLOR_TYPE, vehicle.colorType);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_FIRST_COLOR, vehicle.firstColor);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_SECOND_COLOR, vehicle.secondColor);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_PEARLESCENT_COLOR, vehicle.pearlescent);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_FACTION, vehicle.faction);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_PLATE, vehicle.plate);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_OWNER, vehicle.owner);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_PRICE, vehicle.price);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_GAS, vehicle.gas);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_KMS, vehicle.kms);

                            // Update parking values
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_DIMENSION, 0);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_PARKING, 0);
                            NAPI.Data.SetEntityData(newVehicle, EntityData.VEHICLE_PARKED, 0);

                            // Add tunning
                            Mechanic.AddTunningToVehicle(newVehicle);

                            // Unlink from the parking
                            parkedCars.Remove(parkedCar);

                            return;
                        }

                        // The vehicle is not in this parking
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_THIS_PARKING);
                        return;
                    }
                }

                // Player's not in any parking
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PARKING_NEAR);
            }
        }
    }
}
