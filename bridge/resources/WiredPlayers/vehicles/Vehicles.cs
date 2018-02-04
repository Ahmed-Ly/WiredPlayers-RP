using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.mechanic;
using WiredPlayers.model;
using WiredPlayers.parking;
using WiredPlayers.business;
using WiredPlayers.weapons;
using WiredPlayers.chat;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

namespace WiredPlayers.vehicles
{
    public class Vehicles : Script
    {
        private static Dictionary<int, Timer> gasTimerList = new Dictionary<int, Timer>();
        private static Dictionary<int, Timer> vehicleRespawnTimerList = new Dictionary<int, Timer>();

        public Vehicles()
        {
            Event.OnPlayerDisconnected += OnPlayerDisconnectedHandler;
            Event.OnPlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
            Event.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Event.OnPlayerExitVehicle += OnPlayerExitVehicle;
            Event.OnVehicleDeath += OnVehicleDeath;
        }
        
        public void LoadDatabaseVehicles()
        {
            List<VehicleModel> vehicleList = Database.LoadAllVehicles();
            Parking.parkedCars = new List<ParkedCarModel>();
            
            foreach (VehicleModel vehModel in vehicleList)
            {
                if (vehModel.parking == 0)
                {
                    Vehicle vehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.VehicleNameToModel(vehModel.model), vehModel.position, vehModel.rotation.Z, new Color(0, 0, 0), new Color(0, 0, 0));
                    NAPI.Vehicle.SetVehicleNumberPlate(vehicle, vehModel.plate == String.Empty ? "LS " + (1000 + vehModel.id) : vehModel.plate);
                    NAPI.Vehicle.SetVehicleEngineStatus(vehicle, vehModel.engine == 0 ? false : true);
                    NAPI.Vehicle.SetVehicleLocked(vehicle, vehModel.locked == 0 ? false : true);
                    NAPI.Entity.SetEntityDimension(vehicle, Convert.ToUInt32(vehModel.dimension));
                    
                    // Añadimos el color
                    if (vehModel.colorType == Constants.VEHICLE_COLOR_TYPE_PREDEFINED)
                    {
                        NAPI.Vehicle.SetVehiclePrimaryColor(vehicle, Int32.Parse(vehModel.firstColor));
                        NAPI.Vehicle.SetVehicleSecondaryColor(vehicle, Int32.Parse(vehModel.secondColor));
                        NAPI.Vehicle.SetVehiclePearlescentColor(vehicle, vehModel.pearlescent);
                    }
                    else
                    {
                        String[] firstColor = vehModel.firstColor.Split(',');
                        String[] secondColor = vehModel.secondColor.Split(',');
                        NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, Int32.Parse(firstColor[0]), Int32.Parse(firstColor[1]), Int32.Parse(firstColor[2]));
                        NAPI.Vehicle.SetVehicleCustomSecondaryColor(vehicle, Int32.Parse(secondColor[0]), Int32.Parse(secondColor[1]), Int32.Parse(secondColor[2]));
                    }
                    
                    // Aumentamos el motor de vehículos policiales
                    if (vehModel.faction == Constants.FACTION_POLICE)
                    {
                        NAPI.Vehicle.SetVehicleEnginePowerMultiplier(vehicle, 15.0f);
                    }

                    // Seteamos las variables
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_ID, vehModel.id);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_MODEL, vehModel.model);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_POSITION, vehModel.position);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_ROTATION, vehModel.rotation);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_DIMENSION, vehModel.dimension);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE, vehModel.colorType);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR, vehModel.firstColor);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR, vehModel.secondColor);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR, vehModel.pearlescent);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_FACTION, vehModel.faction);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PLATE, vehModel.plate);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_OWNER, vehModel.owner);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PRICE, vehModel.price);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PARKING, vehModel.parking);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PARKED, vehModel.parked);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_GAS, vehModel.gas);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_KMS, vehModel.kms);

                    // Aplicamos el tunning
                    Mechanic.AddTunningToVehicle(vehicle);
                }
                else
                {
                    // Creamos el modelo de coche aparcado
                    ParkedCarModel parkedCarModel = new ParkedCarModel();
                    parkedCarModel.parkingId = vehModel.parking;
                    parkedCarModel.vehicle = vehModel;

                    // Guardamos el coche en el parking
                    Parking.parkedCars.Add(parkedCarModel);
                }
            }
        }
        
        public static void CreateVehicle(Client player, VehicleModel vehModel, bool adminCreated)
        {
            int vehicleId = Database.AddNewVehicle(vehModel);
            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.VehicleNameToModel(vehModel.model), vehModel.position, vehModel.rotation.Z, new Color(0, 0, 0), new Color(0, 0, 0));
            NAPI.Vehicle.SetVehicleNumberPlate(vehicle, vehModel.plate == String.Empty ? "LS " + (1000 + vehicleId) : vehModel.plate);
            NAPI.Vehicle.SetVehicleEngineStatus(vehicle, vehModel.engine == 0 ? false : true);
            NAPI.Vehicle.SetVehicleLocked(vehicle, vehModel.locked == 0 ? false : true);
            NAPI.Entity.SetEntityDimension(vehicle, Convert.ToUInt32(vehModel.dimension));

            // Añadimos el color
            if (vehModel.colorType == Constants.VEHICLE_COLOR_TYPE_PREDEFINED)
            {
                NAPI.Vehicle.SetVehiclePrimaryColor(vehicle, Int32.Parse(vehModel.firstColor));
                NAPI.Vehicle.SetVehicleSecondaryColor(vehicle, Int32.Parse(vehModel.secondColor));
                NAPI.Vehicle.SetVehiclePearlescentColor(vehicle, vehModel.pearlescent);
            }
            else
            {
                String[] firstColor = vehModel.firstColor.Split(',');
                String[] secondColor = vehModel.secondColor.Split(',');
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, Int32.Parse(firstColor[0]), Int32.Parse(firstColor[1]), Int32.Parse(firstColor[2]));
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(vehicle, Int32.Parse(secondColor[0]), Int32.Parse(secondColor[1]), Int32.Parse(secondColor[2]));
            }

            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_ID, vehicleId);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_MODEL, vehModel.model);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_POSITION, vehModel.position);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_ROTATION, vehModel.rotation);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_DIMENSION, vehModel.dimension);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE, vehModel.colorType);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR, vehModel.firstColor);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR, vehModel.secondColor);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR, vehModel.pearlescent);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_FACTION, vehModel.faction);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PLATE, vehModel.plate);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_OWNER, vehModel.owner);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PRICE, vehModel.price);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PARKING, vehModel.parking);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PARKED, vehModel.parked);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_GAS, vehModel.gas);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_KMS, vehModel.kms);

            if (!adminCreated)
            {
                int moneyLeft = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK) - vehModel.price;
                String purchaseMssage = String.Format(Messages.SUC_VEHICLE_PURCHASED, vehModel.model, vehModel.price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + purchaseMssage);
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, moneyLeft);
            }

            // Aplicamos el tunning
            Mechanic.AddTunningToVehicle(vehicle);
        }

        public static bool HasPlayerVehicleKeys(Client player, Vehicle vehicle)
        {
            bool hasKeys = false;
            if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER) == player.Name)
            {
                hasKeys = true;
            }
            else
            {
                int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                String keyString = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS);
                String[] keyArray = keyString.Split(',');
                foreach (String key in keyArray)
                {
                    if (Int32.Parse(key) == vehicleId)
                    {
                        hasKeys = true;
                        break;
                    }
                }
            }
            return hasKeys;
        }

        public static bool HasPlayerVehicleKeys(Client player, VehicleModel vehicle)
        {
            bool hasKeys = false;
            if (vehicle.owner == player.Name)
            {
                hasKeys = true;
            }
            else
            {
                String keyString = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS);
                String[] keyArray = keyString.Split(',');
                foreach (String key in keyArray)
                {
                    if (Int32.Parse(key) == vehicle.id)
                    {
                        hasKeys = true;
                        break;
                    }
                }
            }
            return hasKeys;
        }

        public static Vehicle GetVehicleById(int vehicleId)
        {
            Vehicle vehicle = null;

            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
            {
                if (NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID) == vehicleId)
                {
                    vehicle = veh;
                    break;
                }
            }

            return vehicle;
        }

        public static VehicleModel GetParkedVehicleById(int vehicleId)
        {
            VehicleModel vehicle = null;
            foreach (ParkedCarModel parkedVehicle in Parking.parkedCars)
            {
                if (parkedVehicle.vehicle.id == vehicleId)
                {
                    vehicle = parkedVehicle.vehicle;
                    break;
                }
            }
            return vehicle;
        }

        private bool IsVehicleTrunkInUse(Vehicle vehicle)
        {
            bool trunkUsed = false;

            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_OPENED_TRUNK) == true)
                {
                    Vehicle openedVehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_OPENED_TRUNK);
                    if (openedVehicle == vehicle)
                    {
                        trunkUsed = true;
                        break;
                    }
                }
            }

            return trunkUsed;
        }

        private void OnPlayerDisconnectedHandler(Client player, byte type, string reason)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                if (gasTimerList.TryGetValue(player.Value, out Timer gasTimer) == true)
                {
                    // Eliminamos el timer
                    gasTimer.Dispose();
                    gasTimerList.Remove(player.Value);
                }
            }
        }

        private void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PARKED_VEHICLE) == true)
            {
                // Obtenemos el checkpoint con el localizador
                Checkpoint vehicleCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PARKED_VEHICLE);

                if(vehicleCheckpoint == checkpoint)
                {
                    // Borramos el checkpoint
                    NAPI.Entity.DeleteEntity(vehicleCheckpoint);

                    // Borramos las marcas
                    NAPI.ClientEvent.TriggerClientEvent(player, "deleteVehicleLocation");

                    // Reseteamos la variable de localización
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_PARKED_VEHICLE);
                }
            }
        }

        private void OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            // Quitamos el cinturón si lo lleva puesto
            if (NAPI.Player.GetPlayerSeatbelt(player) == true)
            {
                NAPI.Player.SetPlayerSeatbelt(player, false);
                Chat.SendMessageToNearbyPlayers(player, Messages.INF_SEATBELT_UNFASTEN, Constants.MESSAGE_ME, 20.0f);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TRUNK_WITHDRAW_ITEMS);
            }

            // Guardamos los valores del velocímetro
            NAPI.ClientEvent.TriggerClientEvent(player, "resetSpeedometer", vehicle);
        }

        private void OnPlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seat)
        {
            if (Convert.ToInt32(seat) == Constants.VEHICLE_SEAT_DRIVER)
            {
                if (NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_TESTING) == true)
                {
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_TESTING_VEHICLE) == true)
                    {
                        Vehicle testingVehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_TESTING_VEHICLE);
                        if (vehicle != testingVehicle)
                        {
                            NAPI.Player.WarpPlayerOutOfVehicle(player);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_TESTING_VEHICLE);
                            return;
                        }
                    }
                    else
                    {
                        NAPI.Player.WarpPlayerOutOfVehicle(player);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_TESTING_VEHICLE);
                        return;
                    }
                }
                else
                {
                    // Obtenemos la facción del vehículo
                    int vehFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);

                    if (vehFaction > 0)
                    {
                        // Obtenemos el trabajo y facción del jugador
                        int playerFaction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                        int playerJob = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) + Constants.MAX_FACTION_VEHICLES;

                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE && vehFaction == Constants.FACTION_ADMIN)
                        {
                            NAPI.Player.WarpPlayerOutOfVehicle(player);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ADMIN_VEHICLE);
                            return;
                        }
                        else if (vehFaction > 0 && vehFaction < Constants.MAX_FACTION_VEHICLES && playerFaction != vehFaction && vehFaction != Constants.FACTION_DRIVING_SCHOOL && vehFaction != Constants.FACTION_ADMIN)
                        {
                            NAPI.Player.WarpPlayerOutOfVehicle(player);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE_FACTION);
                            return;
                        }
                        else if (vehFaction > Constants.MAX_FACTION_VEHICLES && playerJob != vehFaction)
                        {
                            NAPI.Player.WarpPlayerOutOfVehicle(player);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE_JOB);
                            return;
                        }
                    }
                }

                // Avisamos al conductor de cómo encender el motor
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_HOW_TO_START_ENGINE);

                // Inicializamos el velocímetro
                float kms = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_KMS);
                float gas = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_GAS);
                NAPI.ClientEvent.TriggerClientEvent(player, "initializeSpeedometer", vehicle, kms, gas);
            }
        }

        private void OnVehicleDeath(Vehicle vehicle)
        {
            // Respawneamos el vehículo al de unos segundos
            int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
            Timer vehicleRespawnTimer = new Timer(OnVehicleDeathTimer, vehicle, 7500, Timeout.Infinite);
            vehicleRespawnTimerList.Add(vehicleId, vehicleRespawnTimer);
        }

        private void OnVehicleDeathTimer(object vehicleObject)
        {
            try
            {
                Vehicle vehicle = (Vehicle)vehicleObject;

                // Obtenemos los valores necesarios para recrear el vehículo
                VehicleModel vehicleModel = new VehicleModel();
                vehicleModel.id = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                vehicleModel.model = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                vehicleModel.position = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                vehicleModel.rotation = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ROTATION);
                vehicleModel.dimension = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_DIMENSION);
                vehicleModel.colorType = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE);
                vehicleModel.firstColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR);
                vehicleModel.secondColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR);
                vehicleModel.pearlescent = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR);
                vehicleModel.faction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);
                vehicleModel.plate = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PLATE);
                vehicleModel.owner = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER);
                vehicleModel.price = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PRICE);
                vehicleModel.parking = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PARKING);
                vehicleModel.parked = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PARKED);
                vehicleModel.gas = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_GAS);
                vehicleModel.kms = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_KMS);
                
                NAPI.Task.Run(() =>
                {
                    // Borramos el vehículo destruído del servidor
                    NAPI.Entity.DeleteEntity(vehicle);
                });
                
                if (vehicleModel.faction == Constants.FACTION_NONE && NAPI.Vehicle.GetVehicleOccupants(vehicle).Count > 0)
                {
                    // Buscamos el desguace
                    ParkingModel scrapyard = Parking.parkingList.Where(p => p.type == Constants.PARKING_TYPE_SCRAPYARD).FirstOrDefault();
                    if (scrapyard != null)
                    {
                        // Añadimos el vehículo al desguace
                        ParkedCarModel parkedCar = new ParkedCarModel();
                        parkedCar.parkingId = scrapyard.id;
                        parkedCar.vehicle = vehicleModel;
                        Parking.parkedCars.Add(parkedCar);
                        vehicleModel.parking = scrapyard.id;
                    }

                    // Guardamos el vehículo
                    Database.SaveVehicle(vehicleModel);
                }
                else
                {
                    NAPI.Task.Run(() =>
                    {
                        // Recreamos el vehículo
                        vehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.VehicleNameToModel(vehicleModel.model), vehicleModel.position, vehicleModel.rotation.Z, new Color(0, 0, 0), new Color(0, 0, 0));
                    });
                    
                    NAPI.Vehicle.SetVehicleNumberPlate(vehicle, vehicleModel.plate == String.Empty ? "LS " + (1000 + vehicleModel.id) : vehicleModel.plate);
                    NAPI.Entity.SetEntityDimension(vehicle, Convert.ToUInt32(vehicleModel.dimension));
                    NAPI.Vehicle.SetVehicleEngineStatus(vehicle, false);
                    NAPI.Vehicle.SetVehicleLocked(vehicle, false);

                    // Añadimos el color
                    if (vehicleModel.colorType == Constants.VEHICLE_COLOR_TYPE_PREDEFINED)
                    {
                        NAPI.Vehicle.SetVehiclePrimaryColor(vehicle, Int32.Parse(vehicleModel.firstColor));
                        NAPI.Vehicle.SetVehicleSecondaryColor(vehicle, Int32.Parse(vehicleModel.secondColor));
                        NAPI.Vehicle.SetVehiclePearlescentColor(vehicle, vehicleModel.pearlescent);
                    }
                    else
                    {
                        String[] firstColor = vehicleModel.firstColor.Split(',');
                        String[] secondColor = vehicleModel.secondColor.Split(',');
                        NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, Int32.Parse(firstColor[0]), Int32.Parse(firstColor[1]), Int32.Parse(firstColor[2]));
                        NAPI.Vehicle.SetVehicleCustomSecondaryColor(vehicle, Int32.Parse(secondColor[0]), Int32.Parse(secondColor[1]), Int32.Parse(secondColor[2]));
                    }

                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_ID, vehicleModel.id);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_MODEL, vehicleModel.model);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_POSITION, vehicleModel.position);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_ROTATION, vehicleModel.rotation);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_DIMENSION, vehicleModel.dimension);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE, vehicleModel.colorType);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR, vehicleModel.firstColor);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR, vehicleModel.secondColor);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR, vehicleModel.pearlescent);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_FACTION, vehicleModel.faction);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PLATE, vehicleModel.plate);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_OWNER, vehicleModel.owner);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PRICE, vehicleModel.price);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PARKING, vehicleModel.parking);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PARKED, vehicleModel.parked);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_GAS, vehicleModel.gas);
                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_KMS, vehicleModel.kms);

                    // Añadimos el tunning
                    Mechanic.AddTunningToVehicle(vehicle);
                }

                // Borramos el timer de la lista
                Timer vehicleRespawnTimer = vehicleRespawnTimerList[vehicleModel.id];
                if (vehicleRespawnTimer != null)
                {
                    vehicleRespawnTimer.Dispose();
                    vehicleRespawnTimerList.Remove(vehicleModel.id);
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnVehicleDeathTimer] " + ex.Message);
            }
        }

        private void OnVehicleRefueled(object vehicleObject)
        {
            try
            {
                Vehicle vehicle = (Vehicle)vehicleObject;
                Client player = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_REFUELING);

                // Reseteamos las variables
                NAPI.Data.ResetEntityData(vehicle, EntityData.VEHICLE_REFUELING);
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REFUELING);

                // Borramos el timer de la lista
                if (gasTimerList.TryGetValue(player.Value, out Timer gasTimer) == true)
                {
                    // Eliminamos el timer
                    gasTimer.Dispose();
                    gasTimerList.Remove(player.Value);
                }

                // Avisamos al jugador
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_REFUELED);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnVehicleRefueled] " + ex.Message);
            }
        }

        [RemoteEvent("stopPlayerCar")]
        public void StopPlayerCarEvent(Client player, params object[] arguments)
        {
            // Paramos el motor del vehículo
            Vehicle vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(NAPI.Player.GetPlayerVehicle(player));
            NAPI.Vehicle.SetVehicleEngineStatus(vehicle, false);
        }

        [RemoteEvent("engineOnEventKey")]
        public void EngineOnEventKeyEvent(Client player, params object[] arguments)
        {
            // Obtenemos los datos del vehículo
            Vehicle vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(NAPI.Player.GetPlayerVehicle(player));

            if (NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_TESTING) == false)
            {
                // Obtenemos la facción y trabajo del jugador y el vehículo
                int playerFaction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                int playerJob = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) + Constants.MAX_FACTION_VEHICLES;

                int vehicleFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);

                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ALREADY_FUCKING) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CANT_TOOGLE_ENGINE_WHILE_FUCKING);
                }
                else if (NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_REFUELING) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_START_REFUELING);
                }
                else if (NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_WEAPON_UNPACKING) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_START_WEAPON_UNPACKING);
                }
                else if (!HasPlayerVehicleKeys(player, vehicle) && vehicleFaction == Constants.FACTION_NONE)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                }
                else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE && vehicleFaction == Constants.FACTION_ADMIN)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ADMIN_VEHICLE);
                }
                else if (vehicleFaction > 0 && vehicleFaction < Constants.MAX_FACTION_VEHICLES && playerFaction != vehicleFaction && vehicleFaction != Constants.FACTION_DRIVING_SCHOOL && vehicleFaction != Constants.FACTION_ADMIN)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE_FACTION);
                }
                else if (vehicleFaction > Constants.MAX_FACTION_VEHICLES && playerJob != vehicleFaction)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE_JOB);
                }
                else if (NAPI.Vehicle.GetVehicleEngineStatus(vehicle) == false)
                {
                    NAPI.Vehicle.SetVehicleEngineStatus(vehicle, true);
                    NAPI.Notification.SendNotificationToPlayer(player, Messages.INF_VEHICLE_TURNED_ON);
                }
                else
                {
                    NAPI.Vehicle.SetVehicleEngineStatus(vehicle, false);
                    NAPI.Notification.SendNotificationToPlayer(player, Messages.INF_VEHICLE_TURNED_OFF);
                }
            }
        }

        [RemoteEvent("saveVehicleConsumes")]
        public void SaveVehicleConsumesEvent(Client player, params object[] arguments)
        {
            // Actualizamos los kilómetros y gasolina
            Vehicle vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>((NetHandle)arguments[0]);
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_KMS, float.Parse(arguments[1].ToString()));
            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_GAS, float.Parse(arguments[2].ToString()));
        }

        [Command("cinturon")]
        public void CinturonCommand(Client player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
            }
            else if (NAPI.Player.GetPlayerSeatbelt(player) == true)
            {
                NAPI.Player.SetPlayerSeatbelt(player, false);
                Chat.SendMessageToNearbyPlayers(player, Messages.INF_SEATBELT_UNFASTEN, Constants.MESSAGE_ME, 20.0f);
            }
            else
            {
                NAPI.Player.SetPlayerSeatbelt(player, true);
                Chat.SendMessageToNearbyPlayers(player, Messages.INF_SEATBELT_FASTEN, Constants.MESSAGE_ME, 20.0f);
            }
        }

        [Command("bloqueo")]
        public void BloqueoCmd(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                Vehicle vehicle = Globals.GetClosestVehicle(player);
                if (vehicle == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                }
                else if (HasPlayerVehicleKeys(player, vehicle) == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                }
                else
                {
                    String vehicleModel = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                    VehicleHash vehicleHash = NAPI.Util.VehicleNameToModel(vehicleModel);
                    if (NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_CYCLES || NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_MOTORCYCLES || NAPI.Vehicle.GetVehicleClass(vehicleHash) == Constants.VEHICLE_CLASS_BOATS)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_LOCKABLE);
                    }
                    else if (NAPI.Vehicle.GetVehicleLocked(vehicle) == true)
                    {
                        NAPI.Vehicle.SetVehicleLocked(vehicle, false);
                        Chat.SendMessageToNearbyPlayers(player, Messages.SUC_VEH_UNLOCKED, Constants.MESSAGE_ME, 20.0f);
                    }
                    else
                    {
                        NAPI.Vehicle.SetVehicleLocked(vehicle, true);
                        Chat.SendMessageToNearbyPlayers(player, Messages.SUC_VEH_LOCKED, Constants.MESSAGE_ME, 20.0f);
                    }
                }
            }
        }

        [Command("capo")]
        public void CapoCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                Vehicle vehicle = Globals.GetClosestVehicle(player, 3.75f);
                if (vehicle != null)
                {
                    if (HasPlayerVehicleKeys(player, vehicle) == false && NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_MECHANIC)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                    }
                    else if (NAPI.Vehicle.GetVehicleDoorState(vehicle, Constants.VEHICLE_HOOD) == false)
                    {
                        NAPI.Vehicle.SetVehicleDoorState(vehicle, Constants.VEHICLE_HOOD, true);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_HOOD_OPENED);
                    }
                    else
                    {
                        NAPI.Vehicle.SetVehicleDoorState(vehicle, Constants.VEHICLE_HOOD, false);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_HOOD_CLOSED);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                }
            }
        }

        [Command("maletero", Messages.GEN_TRUNK_COMMAND)]
        public void MaleteroCommand(Client player, String action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                // Obtenemos el vehículo más cercano
                Vehicle vehicle = Globals.GetClosestVehicle(player, 3.75f);

                if (vehicle != null)
                {
                    // Inicializamos el inventario
                    List<InventoryModel> inventory = null;

                    switch (action.ToLower())
                    {
                        case "abrir":
                            if (!HasPlayerVehicleKeys(player, vehicle) && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_NONE)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                            }
                            else if (NAPI.Vehicle.GetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK) == true)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_TRUNK_OPENED);
                            }
                            else
                            {
                                NAPI.Vehicle.SetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK, true);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TRUNK_OPENED);
                            }
                            break;
                        case "cerrar":
                            if (!HasPlayerVehicleKeys(player, vehicle) && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.FACTION_NONE)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                            }
                            else if (NAPI.Vehicle.GetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK) == false)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_TRUNK_CLOSED);
                            }
                            else
                            {
                                NAPI.Vehicle.SetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK, false);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TRUNK_CLOSED);
                            }
                            break;
                        case "guardar":
                            if (NAPI.Vehicle.GetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK) == false)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_TRUNK_CLOSED);
                            }
                            else if (IsVehicleTrunkInUse(vehicle) == true)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_TRUNK_IN_USE);
                            }
                            else
                            {
                                // Miramos si tiene algún objeto en la mano
                                if (NAPI.Data.HasEntitySharedData(player, EntityData.PLAYER_WEAPON_CRATE) == true)
                                {
                                    int vehicleCrates = 0;

                                    // Obtenemos el objeto en la mano
                                    int weaponCrateIndex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_WEAPON_CRATE);
                                    WeaponCrateModel weaponCrate = Weapons.weaponCrateList.ElementAt(weaponCrateIndex);

                                    // Metemos el objeto en el maletero
                                    weaponCrate.carriedEntity = Constants.ITEM_ENTITY_VEHICLE;
                                    weaponCrate.carriedIdentifier = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);

                                    // Eliminamos la caja del jugador
                                    NAPI.Player.StopPlayerAnimation(player);
                                    NAPI.Entity.DetachEntity(weaponCrate.crateObject);
                                    NAPI.Entity.DeleteEntity(weaponCrate.crateObject);
                                    NAPI.Data.ResetEntitySharedData(player, EntityData.PLAYER_WEAPON_CRATE);

                                    // Comprobamos si el vehículo tiene alguna caja en su interior
                                    foreach (WeaponCrateModel crates in Weapons.weaponCrateList)
                                    {
                                        if (crates.carriedEntity == Constants.ITEM_ENTITY_VEHICLE && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID) == crates.carriedIdentifier)
                                        {
                                            vehicleCrates++;
                                        }
                                    }

                                    if (vehicleCrates == 1)
                                    {
                                        // Miramos si el vehículo tiene conductor
                                        foreach (Client target in NAPI.Vehicle.GetVehicleOccupants(vehicle))
                                        {
                                            if (NAPI.Player.GetPlayerVehicleSeat(target) == Constants.VEHICLE_SEAT_DRIVER)
                                            {
                                                Vector3 weaponPosition = new Vector3(-2085.543f, 2600.857f, -0.4712417f);
                                                Checkpoint weaponCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, weaponPosition, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_CHECKPOINT, weaponCheckpoint);
                                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_WEAPON_POSITION_MARK);
                                                NAPI.ClientEvent.TriggerClientEvent(target, "showWeaponCheckpoint", weaponPosition);
                                            }
                                        }
                                    }

                                    // Mandamos el mensaje al jugador
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TRUNK_STORED_ITEMS);
                                }
                                else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
                                {
                                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                                    ItemModel rightHand = Globals.GetItemInEntity(playerId, Constants.ITEM_ENTITY_RIGHT_HAND);
                                    rightHand.ownerIdentifier = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                                    rightHand.ownerEntity = Constants.ITEM_ENTITY_VEHICLE;

                                    // Si es un arma, se la quitamos de la mano
                                    if (Int32.TryParse(rightHand.hash, out int itemHash) == false)
                                    {
                                        WeaponHash weapon = NAPI.Util.WeaponNameToModel(rightHand.hash);
                                        NAPI.Player.RemovePlayerWeapon(player, weapon);
                                    }

                                    // Actualizamos el objeto en base de datos
                                    Database.UpdateItem(rightHand);

                                    // Mandamos el mensaje al jugador
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TRUNK_STORED_ITEMS);
                                }
                                else
                                {
                                    // Cargamos el inventario del jugador
                                    inventory = Globals.GetPlayerInventoryAndWeapons(player);
                                    if (inventory.Count > 0)
                                    {
                                        NAPI.Vehicle.SetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK, true);
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_OPENED_TRUNK, vehicle);
                                        NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_VEHICLE_PLAYER);
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_ITEMS_INVENTORY);
                                    }
                                }
                            }
                            break;
                        case "sacar":
                            if (NAPI.Vehicle.GetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK) == false)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_TRUNK_CLOSED);
                            }
                            else if (IsVehicleTrunkInUse(vehicle) == true)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_TRUNK_IN_USE);
                            }
                            else
                            {
                                // Cargamos el inventario del maletero
                                inventory = Globals.GetVehicleTrunkInventory(vehicle);
                                if (inventory.Count > 0)
                                {
                                    NAPI.Vehicle.SetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK, true);
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_OPENED_TRUNK, vehicle);
                                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_VEHICLE_TRUNK);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_ITEMS_TRUNK);
                                }
                            }
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_TRUNK_COMMAND);
                            break;
                    }
                }
                else
                {
                    // No hay ningún vehículo cerca
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                }
            }
        }

        [Command("llaves", Messages.GEN_KEYS_COMMAND, GreedyArg = true)]
        public void LlavesCommand(Client player, String action, int vehicleId, String targetString = "")
        {
            // Inicializamos la variable del vehículo
            Vehicle vehicle = null;

            // Obtenemos las llaves cedidas
            String playerKeys = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS);
            String[] playerKeysArray = playerKeys.Split(',');

            switch (action.ToLower())
            {
                case "ver":
                    // Miramos que tenga la llave
                    foreach (String key in playerKeysArray)
                    {
                        if (Int32.Parse(key) == vehicleId)
                        {
                            // Obtenemos el vehículo
                            vehicle = GetVehicleById(vehicleId);

                            if (vehicle != null)
                            {
                                String model = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                                String owner = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + String.Format(Messages.INF_VEHICLE_KEYS_INFO, vehicleId, model, owner));
                            }
                            else
                            {
                                // Miramos si está aparcado
                                VehicleModel vehicleModel = GetParkedVehicleById(vehicleId);

                                if (vehicleModel != null)
                                {
                                    String message = String.Format(Messages.INF_VEHICLE_KEYS_INFO, vehicleId, vehicleModel.model, vehicleModel.owner);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                                }
                            }
                            return;
                        }
                    }

                    // El jugador no tiene esas llaves
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                    break;
                case "ceder":

                    vehicle = GetVehicleById(vehicleId);

                    if (vehicle == null)
                    {
                        // Miramos si está aparcado
                        VehicleModel vehicleModel = GetParkedVehicleById(vehicleId);

                        if (vehicle == null)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                            return;
                        }
                    }

                    if (!HasPlayerVehicleKeys(player, vehicle))
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                    }
                    else
                    {
                        if (targetString.Length > 0)
                        {
                            Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                            if (target != null && target.Position.DistanceTo(player.Position) < 5.0f)
                            {
                                String targetKeys = NAPI.Data.GetEntityData(target, EntityData.PLAYER_VEHICLE_KEYS);
                                String[] targetKeysArray = targetKeys.Split(',');
                                for (int i = 0; i < targetKeysArray.Length; i++)
                                {
                                    if (Int32.Parse(targetKeysArray[i]) == 0)
                                    {
                                        targetKeysArray[i] = vehicleId.ToString();
                                        String playerMessage = String.Format(Messages.INF_VEHICLE_KEYS_GIVEN, target.Name);
                                        String targetMessage = String.Format(Messages.INF_VEHICLE_KEYS_RECEIVED, player.Name);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_VEHICLE_KEYS, String.Join(",", targetKeysArray));
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        return;
                                    }
                                }
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_KEYS_FULL);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_KEYS_COMMAND);
                        }
                    }
                    break;
                case "tirar":
                    for (int i = 0; i < playerKeysArray.Length; i++)
                    {
                        if (playerKeysArray[i] == vehicleId.ToString())
                        {
                            playerKeysArray[i] = "0";
                            Array.Sort(playerKeysArray);
                            Array.Reverse(playerKeysArray);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS, String.Join(",", playerKeysArray));
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_KEYS_THROWN);
                            return;
                        }
                    }

                    // Mandamos un mensaje de que no se ha encontrado
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                    break;
                default:
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_KEYS_COMMAND);
                    break;
            }
        }

        [Command("localizar", Messages.GEN_LOCATE_COMMAND)]
        public void LocalizarCommand(Client player, int vehicleId)
        {
            Vehicle vehicle = GetVehicleById(vehicleId);

            if (vehicle == null)
            {
                // Miramos si está aparcado
                VehicleModel vehModel = GetParkedVehicleById(vehicleId);

                if (vehModel != null)
                {
                    if (HasPlayerVehicleKeys(player, vehModel) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                    }
                    else
                    {
                        // Obtenemos el parking
                        ParkingModel parking = Parking.GetParkingById(vehModel.parking);

                        // Creamos la zona
                        Checkpoint locationCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, parking.position, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_PARKED_VEHICLE, locationCheckpoint);

                        // Marcamos la posición del parking
                        NAPI.ClientEvent.TriggerClientEvent(player, "locateVehicle", parking.position);

                        // Avisamos al jugador
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_PARKED);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                }
            }
            else
            {
                // El vehículo no está en un parking
                if (HasPlayerVehicleKeys(player, vehicle) == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                }
                else
                {
                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    {
                        if (NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID) == vehicleId)
                        {
                            // Marcamos la posición del vehículo
                            Vector3 vehiclePosition = NAPI.Entity.GetEntityPosition(veh);
                            NAPI.ClientEvent.TriggerClientEvent(player, "locateVehicle", vehiclePosition);

                            // Creamos la zona

                            Checkpoint locationCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, vehiclePosition, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PARKED_VEHICLE, locationCheckpoint);

                            // Avisamos al jugador
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_PARKED);
                            break;
                        }
                    }
                }
            }
        }

        [Command("repostar", Messages.GEN_FUEL_COMMAND)]
        public void RepostarCommand(Client player, int amount)
        {
            foreach (BusinessModel business in Business.businessList)
            {
                if (business.type == Constants.BUSINESS_TYPE_GAS_STATION && player.Position.DistanceTo(business.position) < 20.5f)
                {
                    // Obtenemos el vehículo más cercano
                    Vehicle vehicle = Globals.GetClosestVehicle(player);

                    // Obtenemos la facción y trabajo del jugador
                    int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                    int job = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);

                    if (vehicle == null)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                    }
                    else if (NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_REFUELING) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_REFUELING);
                    }
                    else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_REFUELING) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_REFUELING);
                    }
                    else if (NAPI.Vehicle.GetVehicleEngineStatus(vehicle) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ENGINE_ON);
                    }
                    else
                    {
                        int vehicleFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);
                        if (HasPlayerVehicleKeys(player, vehicle) || vehicleFaction == faction || vehicleFaction + 100 == job)
                        {
                            // Calculamos lo que falta del depósito
                            float gasRefueled = 0.0f;
                            float currentGas = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_GAS);
                            float gasLeft = 50.0f - currentGas;
                            int maxMoney = (int)Math.Round(gasLeft * Constants.PRICE_GAS * business.multiplier);
                            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

                            if (amount == 0 || amount > maxMoney)
                            {
                                amount = maxMoney;
                                gasRefueled = gasLeft;
                            }
                            else if (amount > 0)
                            {
                                gasRefueled = amount / (Constants.PRICE_GAS * business.multiplier);
                            }

                            if (amount > 0 && playerMoney >= amount)
                            {
                                // Rellenamos el vehículo y cobramos
                                NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_GAS, currentGas + gasRefueled);
                                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - amount);

                                // Marcamos jugador y vehículo repostando
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_REFUELING, vehicle);
                                NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_REFUELING, player);

                                // Empezamos el repostaje
                                Timer gasTimer = new Timer(OnVehicleRefueled, vehicle, (int)Math.Round(gasLeft * 1000), Timeout.Infinite);
                                gasTimerList.Add(player.Value, gasTimer);

                                // Mandamos el mensaje al jugador
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_REFUELING);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                        }
                    }

                    return;
                }
            }

            // Mandamos el mensaje al jugador
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_FUEL_STATION_NEAR);
        }

        [Command("rellenar")]
        public void RellenarCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                ItemModel item = Globals.GetItemModelFromId(itemId);

                if (item.hash == Constants.ITEM_HASH_JERRYCAN)
                {
                    Vehicle vehicle = Globals.GetClosestVehicle(player);
                    if (vehicle != null)
                    {
                        if (HasPlayerVehicleKeys(player, vehicle) == true || NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) == Constants.JOB_MECHANIC)
                        {
                            float gas = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_GAS);
                            NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_GAS, gas + Constants.GAS_CAN_LITRES > 50.0f ? 50.0f : gas + Constants.GAS_CAN_LITRES);

                            // Quitamos la lata de gasolina de la mano del jugador
                            NAPI.Entity.DetachEntity(item.objectHandle);
                            NAPI.Entity.DeleteEntity(item.objectHandle);

                            // Eliminamos la lata de gasolina
                            Database.RemoveItem(item.id);
                            Globals.itemList.Remove(item);

                            // Quitamos el objeto de la mano derecha
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_RIGHT_HAND);

                            // Mandamos el mensaje al jugador
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_REFILLED);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_CAR_KEYS);
                        }
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_JERRYCAN);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_EMPTY);
            }
        }

        [Command("desguazar")]
        public void DesguazarCommand(Client player)
        {
            if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
            {
                ParkingModel parking = Parking.GetClosestParking(player, 3.5f);
                if (parking != null && parking.type == Constants.PARKING_TYPE_SCRAPYARD)
                {
                    NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                    if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER) == player.Name)
                    {
                        // Obtenemos datos básicos del vehículo
                        int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                        int vehiclePrice = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PRICE);
                        float vehicleKms = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_KMS);

                        // Obtenemos el dinero del jugador
                        int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

                        // Obtenemos los precios del desguace
                        int vehicleMaxValue = (int)Math.Round(vehiclePrice * 0.5f);
                        int vehicleMinValue = (int)Math.Round(vehiclePrice * 0.1f);
                        float vehicleReduction = vehicleKms / Constants.REDUCTION_PER_KMS / 1000;
                        int amountGiven = vehicleMaxValue - (int)Math.Round(vehicleReduction / 100 * vehicleMaxValue);

                        // Miramos que al menos dé el mínimo
                        if (amountGiven < vehicleMinValue)
                        {
                            amountGiven = vehicleMinValue;
                        }

                        // Damos el dinero al jugador
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney + amountGiven);

                        // Borramos el vehículo
                        NAPI.Player.WarpPlayerOutOfVehicle(player);
                        Database.RemoveVehicle(vehicleId);
                        NAPI.Entity.DeleteEntity(vehicle);

                        // Mandamos el mensaje al jugador
                        String message = String.Format(Messages.SUC_VEHICLE_SCRAPYARD, amountGiven);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + message);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_VEH_OWNER);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_SCRAPYARD_NEAR);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
            }
        }
    }
}