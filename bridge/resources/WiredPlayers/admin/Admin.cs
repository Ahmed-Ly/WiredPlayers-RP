using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.vehicles;
using WiredPlayers.database;
using WiredPlayers.emergency;
using WiredPlayers.business;
using WiredPlayers.parking;
using WiredPlayers.house;
using System.Collections.Generic;
using System.Linq;
using System;
using WiredPlayers.weapons;

namespace WiredPlayers.admin
{
    public class Admin : Script
    {
        public static List<PermissionModel> permissionList;

        public Admin() { }

        private bool HasUserCommandPermission(Client player, String command, String option = "")
        {
            bool hasPermission = false;
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

            foreach(PermissionModel permission in permissionList)
            {
                if(permission.playerId == playerId && command == permission.command)
                {
                    // Miramos si es opción o no
                    if(option == String.Empty || option == permission.option)
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }

            return hasPermission;
        }

        [Command("skin", Messages.GEN_SKIN_COMMAND)]
        public void SkinCommand(Client player, String pedModel)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                NAPI.Player.SetPlayerSkin(player, pedHash);
            }
        }

        [Command("aviso", Messages.GEN_AVISO_COMMAND, GreedyArg = true)]
        public void AvisoCommand(Client player, String message)
        {
            if(NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // Comprobación de la longitud del mensaje
                String secondMessage = String.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // El mensaje tiene una longitud de dos líneas
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                // Mandamos el mensaje a todos los jugadores
                NAPI.Chat.SendChatMessageToAll(secondMessage.Length > 0 ? Constants.COLOR_ADMIN_INFO + "[AVISO ADMINISTRATIVO] " + message + "..." : Constants.COLOR_ADMIN_INFO + "[AVISO ADMINISTRATIVO] " + message);
                if (secondMessage.Length > 0)
                {
                    NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + secondMessage);
                }
            }
        }

        [Command("coord", Messages.GEN_COORD_COMMAND)]
        public void Coord(Client player, float posX, float posY, float posZ)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                Vector3 position = new Vector3(posX, posY, posZ);
                NAPI.Entity.SetEntityPosition(player, position);
                NAPI.Entity.SetEntityDimension(player, 0);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
            }
        }

        [Command("tpa", Messages.GEN_TP_COMMAND, GreedyArg = true)]
        public void TpaCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // Obtenemos el jugador objetivo
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    String message = String.Format(Messages.ADM_GOTO_PLAYER, target.Name);

                    // Sacamos las variables del administrador
                    int targetBusiness = NAPI.Data.GetEntityData(target, EntityData.PLAYER_BUSINESS_ENTERED);
                    int targetHouse = NAPI.Data.GetEntityData(target, EntityData.PLAYER_HOUSE_ENTERED);
                    uint targetDimension = NAPI.Entity.GetEntityDimension(target);
                    Vector3 position = NAPI.Entity.GetEntityPosition(target);

                    // Editamos la posición del destino
                    NAPI.Entity.SetEntityPosition(player, position);
                    NAPI.Entity.SetEntityDimension(player, targetDimension);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, targetBusiness);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, targetHouse);

                    // Enviamos el mensaje al administrador
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("traer", Messages.GEN_BRING_COMMAND, GreedyArg = true)]
        public void TraerCommand(Client player, String targetString)
        {

            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // Obtenemos el jugador objetivo
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    String message = String.Format(Messages.ADM_BRING_PLAYER, player.SocialClubName);

                    // Sacamos las variables del administrador
                    int playerBusiness = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                    int playerHouse = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                    uint playerDimension = NAPI.Entity.GetEntityDimension(player);
                    Vector3 position = NAPI.Entity.GetEntityPosition(player);

                    // Editamos la posición del destino
                    NAPI.Entity.SetEntityPosition(target, position);
                    NAPI.Entity.SetEntityDimension(target, playerDimension);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_BUSINESS_ENTERED, playerBusiness);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_HOUSE_ENTERED, playerHouse);

                    // Enviamos el mensaje al administrador
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + message);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("gun", Messages.GEN_GUN_COMMAND)]
        public void Gun(Client player, String targetString, String weaponName, int ammo)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
            {
                // Obtenemos el jugador objetivo
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    WeaponHash weapon = NAPI.Util.WeaponNameToModel(weaponName);
                    if (weapon == 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_GUN_COMMAND);
                    }
                    else
                    {
                        // Le damos el arma al jugador
                        Weapons.GivePlayerNewWeapon(target, weapon, ammo, false);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("vehiculo", Messages.GEN_VEHICLE_COMMAND, GreedyArg = true)]
        public void VehiculoCommand(Client player, String args)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                int vehicleId = 0;
                Vehicle veh = null;
                VehicleModel vehicle = new VehicleModel();
                if (args.Trim().Length > 0)
                {
                    String[] arguments = args.Split(' ');
                    switch (arguments[0].ToLower())
                    {
                        case "info":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                veh = Globals.GetClosestVehicle(player);
                                if (veh == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                                }
                                else
                                {
                                    vehicleId = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID);
                                    String model = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_MODEL);
                                    String owner = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_OWNER);
                                    NAPI.Chat.SendChatMessageToPlayer(player, "_________Información del vehículo con ID " + vehicleId + "_________");
                                    NAPI.Chat.SendChatMessageToPlayer(player, "Modelo: " + model);
                                    NAPI.Chat.SendChatMessageToPlayer(player, "Propietario: " + owner);
                                }
                            }
                            break;
                        case "crear":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                if (arguments.Length == 4)
                                {
                                    String[] firstColorArray = arguments[2].Split(',');
                                    String[] secondColorArray = arguments[3].Split(',');
                                    if (firstColorArray.Length == Constants.TOTAL_COLOR_ELEMENTS && secondColorArray.Length == Constants.TOTAL_COLOR_ELEMENTS)
                                    {
                                        try
                                        {
                                            // Rellenamos los datos básicos del vehículo para su creación
                                            vehicle.model = arguments[1];
                                            vehicle.faction = Constants.FACTION_ADMIN;
                                            vehicle.position = NAPI.Entity.GetEntityPosition(player);
                                            vehicle.rotation = NAPI.Entity.GetEntityRotation(player);
                                            vehicle.dimension = NAPI.Entity.GetEntityDimension(player);
                                            vehicle.colorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM;
                                            vehicle.firstColor = "0,0,0";
                                            vehicle.secondColor = "0,0,0";
                                            vehicle.pearlescent = 0;
                                            vehicle.owner = String.Empty;
                                            vehicle.plate = String.Empty;
                                            vehicle.price = 0;
                                            vehicle.parking = 0;
                                            vehicle.parked = 0;
                                            vehicle.gas = 50.0f;
                                            vehicle.kms = 0.0f;
                                            Vehicles.CreateVehicle(player, vehicle, true);
                                        }
                                        catch (Exception ex)
                                        {
                                            NAPI.Util.ConsoleOutput("[EXCEPTION vehiculo crear] " + ex.Message);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_CREATE_COMMAND);
                                        }
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_CREATE_COMMAND);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_CREATE_COMMAND);
                                }
                            }
                            break;
                        case "modificar":
                            if (arguments.Length > 1)
                            {
                                switch (arguments[1].ToLower())
                                {
                                    case "color":
                                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (arguments.Length == 4)
                                            {
                                                veh = Globals.GetClosestVehicle(player);
                                                if (veh == null)
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                                                }
                                                else
                                                {
                                                    String[] firstColorArray = arguments[2].Split(',');
                                                    String[] secondColorArray = arguments[3].Split(',');
                                                    if (firstColorArray.Length == Constants.TOTAL_COLOR_ELEMENTS && secondColorArray.Length == Constants.TOTAL_COLOR_ELEMENTS)
                                                    {
                                                        try
                                                        {
                                                            /*vehicle.firstColor = new ColorModel(Int32.Parse(firstColorArray[0]), Int32.Parse(firstColorArray[1]), Int32.Parse(firstColorArray[2]));
                                                            vehicle.secondColor = new ColorModel(Int32.Parse(secondColorArray[0]), Int32.Parse(secondColorArray[1]), Int32.Parse(secondColorArray[2]));
                                                            NAPI.SetVehicleCustomPrimaryColor(veh, vehicle.firstColor.red, vehicle.firstColor.green, vehicle.firstColor.blue);
                                                            NAPI.SetVehicleCustomSecondaryColor(veh, vehicle.secondColor.red, vehicle.secondColor.green, vehicle.secondColor.blue);
                                                            NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_FIRST_COLOR, vehicle.firstColor.ToString());
                                                            NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_SECOND_COLOR, vehicle.secondColor.ToString());
                                                            Database.UpdateVehicleColor(vehicle);*/
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            NAPI.Util.ConsoleOutput("[EXCEPTION vehiculo modificar color] " + ex.Message);
                                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_COLOR_COMMAND);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_COLOR_COMMAND);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_COLOR_COMMAND);
                                            }
                                        }
                                        break;
                                    case "dimension":
                                        if (arguments.Length == 4)
                                        {
                                            if (Int32.TryParse(arguments[2], out vehicleId) == true)
                                            {
                                                veh = Vehicles.GetVehicleById(vehicleId);
                                                if (veh == null)
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                                                }
                                                else
                                                {
                                                    // Obtenemos la dimension
                                                    if (UInt32.TryParse(arguments[3], out uint dimension) == true)
                                                    {
                                                        String message = String.Format(Messages.ADM_VEHICLE_DIMENSION_MODIFIED, dimension);
                                                        NAPI.Entity.SetEntityDimension(veh, dimension);
                                                        vehicleId = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID);
                                                        NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_DIMENSION, dimension);
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                                        Database.UpdateVehicleSingleValue("dimension", Convert.ToInt32(dimension), vehicleId);
                                                    }
                                                    else
                                                    {
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_DIMENSION_COMMAND);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_DIMENSION_COMMAND);
                                            }
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_DIMENSION_COMMAND);
                                        }
                                        break;
                                    case "faccion":
                                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {

                                            if (arguments.Length == 3)
                                            {
                                                veh = Globals.GetClosestVehicle(player);
                                                if (veh == null)
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                                                }
                                                else
                                                {
                                                    // Obtenemos la facción
                                                    if (Int32.TryParse(arguments[2], out int faction) == true)
                                                    {
                                                        String message = String.Format(Messages.ADM_VEHICLE_FACTION_MODIFIED, faction);
                                                        vehicleId = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID);
                                                        NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_FACTION, faction);
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                                        Database.UpdateVehicleSingleValue("faction", faction, vehicleId);
                                                    }
                                                    else
                                                    {
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_FACTION_COMMAND);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_FACTION_COMMAND);
                                            }
                                        }
                                        break;
                                    case "posicion":
                                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (NAPI.Player.IsPlayerInAnyVehicle(player) == true)
                                            {
                                                veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(NAPI.Player.GetPlayerVehicle(player));
                                                vehicle.position = NAPI.Entity.GetEntityPosition(veh);
                                                vehicle.rotation = NAPI.Entity.GetEntityRotation(veh);
                                                vehicle.id = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID);
                                                NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_POSITION, vehicle.position);
                                                NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_ROTATION, vehicle.rotation);
                                                Database.UpdateVehiclePosition(vehicle);
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_VEHICLE_POS_UPDATED);
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
                                            }
                                        }
                                        break;
                                    case "dueño":
                                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (arguments.Length == 4)
                                            {
                                                veh = Globals.GetClosestVehicle(player);
                                                if (veh == null)
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                                                }
                                                else
                                                {
                                                    String owner = arguments[2] + " " + arguments[3];
                                                    String message = String.Format(Messages.ADM_VEHICLE_OWNER_MODIFIED, owner);
                                                    vehicleId = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID);
                                                    NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_OWNER, owner);
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                                    Database.UpdateVehicleSingleString("owner", owner, vehicleId);
                                                }
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_OWNER_COMMAND);
                                            }
                                        }
                                        break;
                                    default:
                                        NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_MODIFY_COMMAND);
                                        break;
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_MODIFY_COMMAND);
                            }
                            break;
                        case "eliminar":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                if (arguments.Length == 2 && int.TryParse(arguments[1], out vehicleId) == true)
                                {
                                    veh = Vehicles.GetVehicleById(vehicleId);
                                    if (veh != null)
                                    {
                                        NAPI.Entity.DeleteEntity(veh);
                                        Database.RemoveVehicle(vehicleId);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_DELETE_COMMAND);
                                }
                            }
                            break;
                        case "reparar":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                NAPI.Vehicle.RepairVehicle(NAPI.Player.GetPlayerVehicle(player));
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + "Has reparado el vehículo.");
                            }
                            break;
                        case "bloquear":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                veh = Globals.GetClosestVehicle(player);
                                if (veh == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                                }
                                else if (NAPI.Vehicle.GetVehicleLocked(veh) == true)
                                {
                                    NAPI.Vehicle.SetVehicleLocked(veh, false);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.SUC_VEH_UNLOCKED);
                                }
                                else
                                {
                                    NAPI.Vehicle.SetVehicleLocked(veh, true);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.SUC_VEH_LOCKED);
                                }
                            }
                            break;
                        case "arrancar":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
                                {
                                    veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(NAPI.Player.GetPlayerVehicle(player));
                                    NAPI.Vehicle.SetVehicleEngineStatus(veh, true);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
                                }
                            }
                            break;
                        case "traer":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                if (arguments.Length == 2 && int.TryParse(arguments[1], out vehicleId) == true)
                                {
                                    veh = Vehicles.GetVehicleById(vehicleId);
                                    if (veh != null)
                                    {
                                        // Traemos el vehículo a la posición
                                        NAPI.Entity.SetEntityPosition(veh, player.Position);
                                        NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_POSITION, player.Position);

                                        // Informamos al jugador
                                        String message = String.Format(Messages.ADM_VEHICLE_BRING, vehicleId);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_BRING_COMMAND);
                                }
                            }
                            break;
                        case "tpa":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                if (arguments.Length == 2 && int.TryParse(arguments[1], out vehicleId) == true)
                                {
                                    veh = Vehicles.GetVehicleById(vehicleId);
                                    if (veh == null)
                                    {
                                        VehicleModel vehModel = Vehicles.GetParkedVehicleById(vehicleId);

                                        if (vehModel == null)
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                                        }
                                        else
                                        {
                                            // Vamos a la posición del parking
                                            ParkingModel parking = Parking.GetParkingById(vehModel.parking);
                                            NAPI.Entity.SetEntityPosition(player, parking.position);

                                            // Informamos al jugador
                                            String message = String.Format(Messages.ADM_VEHICLE_GOTO, vehicleId);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                    }
                                    else
                                    {
                                        // Vamos a la posición del vehículo
                                        Vector3 position = NAPI.Entity.GetEntityPosition(veh);
                                        NAPI.Entity.SetEntityPosition(player, position);

                                        // Informamos al jugador
                                        String message = String.Format(Messages.ADM_VEHICLE_GOTO, vehicleId);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_GOTO_COMMAND);
                                }
                            }
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_COMMAND);
                            break;
                    }
                }
            }
        }

        [Command("ir", Messages.GEN_GO_COMMAND)]
        public void IrCommand(Client player, String location)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                switch (location.ToLower())
                {
                    case "taller":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1204.13f, -1489.49f, 4.34967f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "electronica":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1148.98f, -1608.94f, 4.41592f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "comisaria":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1111.952f, -824.9194f, 19.31578f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "ayuntamiento":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1285.544f, -567.0439f, 31.71239f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "autoescuela":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-70f, -1100f, 28f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "vanilla":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(120f, -1400f, 30f));
                        NAPI.Entity.SetEntityDimension(player, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        break;
                    case "hospital":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1385.481f, -976.4036f, 9.273162f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "weazel":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-600f, -950f, 25f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "bahama":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1400f, -590f, 30f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "mecanico":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(492f, -1300f, 30f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case "basurero":
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-320f, -1550f, 30f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_GO_COMMAND);
                        break;

                }
            }
        }

        [Command("negocio", Messages.GEN_BUSINESS_COMMAND, GreedyArg = true)]
        public void NegocioCommand(Client player, String args)
        {
            if (HasUserCommandPermission(player, "negocio") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                if (args.Trim().Length > 0)
                {
                    BusinessModel business = new BusinessModel();
                    String[] arguments = args.Split(' ');
                    String message = String.Empty;
                    switch (arguments[0].ToLower())
                    {
                        case "info":
                            break;
                        case "crear":
                            if (HasUserCommandPermission(player, "negocio", "crear") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                if (arguments.Length == 2)
                                {
                                    // Obtenemos el tipo de negocio
                                    if (Int32.TryParse(arguments[1], out int type) == true)
                                    {
                                        business.type = type;
                                        business.ipl = Business.GetBusinessTypeIpl(type);
                                        business.position = player.Position;
                                        business.dimension = player.Dimension;
                                        business.multiplier = 3.0f;
                                        business.owner = String.Empty;
                                        business.locked = false;
                                        business.name = "Negocio";
                                        business.id = Database.AddNewBusiness(business);
                                        business.businessLabel = NAPI.TextLabel.CreateTextLabel(business.name, business.position, 20.0f, 0.75f, 4, new Color(255, 255, 255), false, business.dimension);
                                        Business.businessList.Add(business);
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_COMMAND);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND2); 
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND3);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_COMMAND);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND2);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND3);
                                }
                            }
                            break;
                        case "modificar":
                            if (HasUserCommandPermission(player, "negocio", "modificar") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                business = Business.GetClosestBusiness(player);
                                if (business != null)
                                {
                                    if (arguments.Length > 1)
                                    {
                                        switch (arguments[1].ToLower())
                                        {
                                            case "nombre":
                                                if (arguments.Length > 2)
                                                {
                                                    // Cambiamos el nombre
                                                    String businessName = String.Join(" ", arguments.Skip(2));
                                                    business.name = businessName;
                                                    NAPI.TextLabel.SetTextLabelText(business.businessLabel, businessName);
                                                    Database.UpdateBusiness(business);

                                                    // Mandamos el mensaje
                                                    message = String.Format(Messages.ADM_BUSINESS_NAME_MODIFIED, businessName);
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                                }
                                                else
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_MODIFY_NAME_COMMAND);
                                                }
                                                break;
                                            case "tipo":
                                                if (arguments.Length == 3)
                                                {
                                                    // Obtenemos el tipo de negocio
                                                    if (Int32.TryParse(arguments[2], out int businessType) == true)
                                                    {
                                                        // Cambiamos el tipo
                                                        business.type = businessType;
                                                        business.ipl = Business.GetBusinessTypeIpl(businessType);
                                                        Database.UpdateBusiness(business);

                                                        // Mandamos el mensaje
                                                        message = String.Format(Messages.ADM_BUSINESS_TYPE_MODIFIED, businessType);
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                                    }
                                                    else
                                                    {
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_MODIFY_TYPE_COMMAND);
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND);
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND2);
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND3);
                                                    }
                                                }
                                                else
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_MODIFY_TYPE_COMMAND);
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND);
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND2);
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_CREATE_TYPES_FIRST_COMMAND3);
                                                }
                                                break;
                                            default:
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_MODIFY_COMMAND);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_MODIFY_COMMAND);
                                    }
                                }
                            }
                            break;
                        case "eliminar":
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                business = Business.GetClosestBusiness(player);
                                if (business != null)
                                {
                                    NAPI.Entity.DeleteEntity(business.businessLabel);
                                    Database.DeleteBusiness(business.id);
                                    Business.businessList.Remove(business);
                                }
                            }
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_COMMAND);
                            break;
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_COMMAND);
                }
            }
        }

        [Command("personaje", Messages.GEN_CHARACTER_COMMAND)]
        public void PersonajeCommand(Client player, String action, String name = "", String surname = "", String amount = "")
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                Client target = null;

                // Miramos si viene un id o un nombre
                if (Int32.TryParse(name, out int targetId) == true)
                {
                    target = Globals.GetPlayerById(targetId);
                    amount = surname;
                }
                else
                {
                    target = NAPI.Player.GetPlayerFromName(name + " " + surname);
                }

                // Comprobamos que el jugador se encuentre conectado
                if (target != null && NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                {
                    // Obtenemos la cantidad
                    if (Int32.TryParse(amount, out int value) == true)
                    {
                        String message = String.Empty;
                        switch (action.ToLower())
                        {
                            case "banco":
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                                {
                                    NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_BANK, value);
                                    message = String.Format(Messages.ADM_PLAYER_BANK_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case "dinero":
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                                {
                                    NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_MONEY, value);
                                    message = String.Format(Messages.ADM_PLAYER_MONEY_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case "faccion":
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                {
                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, value);
                                    message = String.Format(Messages.ADM_PLAYER_FACTION_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case "trabajo":
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                {
                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB, value);
                                    message = String.Format(Messages.ADM_PLAYER_JOB_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case "rango":
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                {
                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, value);
                                    message = String.Format(Messages.ADM_PLAYER_RANK_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case "dimension":
                                NAPI.Entity.SetEntityDimension(target, Convert.ToUInt32(value));
                                message = String.Format(Messages.ADM_PLAYER_DIMENSION_MODIFIED, value, target.Name);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                break;
                            default:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_CHARACTER_COMMAND);
                                break;
                        }
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_CHARACTER_COMMAND);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("casa", Messages.GEN_HOUSE_COMMAND, GreedyArg = true)]
        public void CasaCommand(Client player, String args)
        {
            if (HasUserCommandPermission(player, "casa") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                HouseModel house = House.GetClosestHouse(player);
                String[] arguments = args.Split(' ');
                switch (arguments[0].ToLower())
                {
                    case "info":
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                        {
                            // Obtenemos el identificador de la casa
                            if (arguments.Length == 2 && int.TryParse(arguments[1], out int houseId) == true) //Se ha metido ID.
                            {
                                house = House.GetHouseById(houseId);
                                if (house != null)
                                {
                                    SendHouseInfo(player, house);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_NOT_EXISTS);
                                }
                            }
                            else if (arguments.Length == 1) //No se ha metido,  la mas cercana
                            {
                                SendHouseInfo(player, house);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_HOUSE_INFO_COMMAND);
                            }
                        }
                        break;
                    case "crear":
                        if (HasUserCommandPermission(player, "casa", "crear") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                        {
                            String houseLabel = String.Empty;
                            house = new HouseModel();
                            house.ipl = Constants.HOUSE_IPL_LIST[0].ipl;
                            house.name = "Casa";
                            house.position = player.Position;
                            house.dimension = player.Dimension;
                            house.price = 10000;
                            house.owner = String.Empty;
                            house.status = Constants.HOUSE_STATE_BUYABLE;
                            house.tenants = 2;
                            house.rental = 0;
                            house.locked = true;
                            house.id = Database.AddHouse(house);
                            house.houseLabel = NAPI.TextLabel.CreateTextLabel(House.GetHouseLabelText(house), house.position, 20.0f, 0.75f, 4, new Color(255, 255, 255));
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_HOUSE_CREATED);
                            House.houseList.Add(house);
                        }
                        break;
                    case "modificar":
                        if (arguments.Length > 2)
                        {
                            String message = String.Empty;

                            if (Int32.TryParse(arguments[2], out int value) == true)
                            {
                                // Modificación numérica
                                switch (arguments[1].ToLower())
                                {
                                    case "interior":
                                        if (HasUserCommandPermission(player, "casa", "interior") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (value >= 0 && value < Constants.HOUSE_IPL_LIST.Count)
                                            {
                                                house.ipl = Constants.HOUSE_IPL_LIST[value].ipl;
                                                Database.UpdateHouse(house);

                                                // Mandamos el mensaje al administrador
                                                message = String.Format(Messages.ADM_HOUSE_INTERIOR_MODIFIED, value);
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                            }
                                            else
                                            {
                                                message = String.Format(Messages.ERR_HOUSE_INTERIOR_MODIFY, Constants.HOUSE_IPL_LIST.Count - 1);
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                            }
                                        }
                                        break;
                                    case "precio":
                                        if (HasUserCommandPermission(player, "casa", "precio") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (value > 0)
                                            {
                                                house.price = value;
                                                house.status = Constants.HOUSE_STATE_BUYABLE;
                                                NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                                Database.UpdateHouse(house);

                                                // Mandamos el mensaje al administrador
                                                message = String.Format(Messages.ADM_HOUSE_PRICE_MODIFIED, value);
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_PRICE_MODIFY);
                                            }
                                        }
                                        break;
                                    case "estado":
                                        if (value >= 0 && value < 3)
                                        {
                                            house.status = value;
                                            NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                            Database.UpdateHouse(house);

                                            // Mandamos el mensaje al administrador
                                            message = String.Format(Messages.ADM_HOUSE_STATUS_MODIFIED, value);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_STATUS_MODIFY);
                                        }
                                        break;
                                    case "alquiler":
                                        if (value > 0)
                                        {
                                            house.rental = value;
                                            house.status = Constants.HOUSE_STATE_RENTABLE;
                                            NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                            Database.UpdateHouse(house);

                                            // Mandamos el mensaje al administrador
                                            message = String.Format(Messages.ADM_HOUSE_RENTAL_MODIFIED, value);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_RENTAL_MODIFY);
                                        }
                                        break;
                                    default:
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_HOUSE_MODIFY_INT_COMMAND);
                                        break;
                                }
                            }
                            else
                            {
                                String name = String.Empty;
                                for (int i = 2; i < arguments.Length; i++)
                                {
                                    name += arguments[i] + " ";
                                }

                                // Modificación de texto
                                switch (arguments[1].ToLower())
                                {
                                    case "dueño":
                                        if (HasUserCommandPermission(player, "casa", "dueño") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            house.owner = name.Trim();
                                            Database.UpdateHouse(house);

                                            // Mandamos el mensaje al administrador
                                            message = String.Format(Messages.ADM_HOUSE_OWNER_MODIFIED);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        break;
                                    case "nombre":
                                        if (HasUserCommandPermission(player, "casa", "nombre") || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            house.name = name.Trim();
                                            NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                            Database.UpdateHouse(house);

                                            // Mandamos el mensaje al administrador
                                            message = String.Format(Messages.ADM_HOUSE_NAME_MODIFIED, value);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        break;
                                    default:
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_HOUSE_MODIFY_STRING_COMMAND);
                                        break;

                                }
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_HOUSE_MODIFY_COMMAND);
                        }
                        break;
                    case "eliminar":
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                        {
                            if (house != null)
                            {
                                NAPI.Entity.DeleteEntity(house.houseLabel);
                                Database.DeleteHouse(house.id);
                                House.houseList.Remove(house);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_HOUSE_DELETED);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_HOUSE_NEAR);
                            }
                        }
                        break;
                    case "tpa":
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                        {
                            // Obtenemos la casa
                            if (arguments.Length == 2 && int.TryParse(arguments[1], out int houseId) == true)
                            {
                                house = House.GetHouseById(houseId);
                                if (house != null)
                                {
                                    NAPI.Entity.SetEntityPosition(player, house.position);
                                    NAPI.Entity.SetEntityDimension(player, house.dimension);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_NOT_EXISTS);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_HOUSE_GOTO_COMMAND);
                            }
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_HOUSE_COMMAND);
                        break;
                }
            }
        }

        private void SendHouseInfo(Client player, HouseModel house)
        {
            NAPI.Chat.SendChatMessageToPlayer(player, "__________Información de la casa:__________");
            NAPI.Chat.SendChatMessageToPlayer(player, "ID: " + house.id);
            NAPI.Chat.SendChatMessageToPlayer(player, "Nombre: " + house.name);
            NAPI.Chat.SendChatMessageToPlayer(player, "IPL: " + house.ipl);
            NAPI.Chat.SendChatMessageToPlayer(player, "Propietario: " + house.owner);
            NAPI.Chat.SendChatMessageToPlayer(player, "Precio: " + house.price);
            NAPI.Chat.SendChatMessageToPlayer(player, "Estado: " + house.status);
        }

        [Command("parking", Messages.GEN_PARKING_COMMAND, GreedyArg = true)]
        public void ParkingCommand(Client player, String args)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                String[] arguments = args.Split(' ');
                ParkingModel parking = Parking.GetClosestParking(player);
                switch (arguments[0].ToLower())
                {
                    case "info":
                        if (parking != null)
                        {
                            int vehicles = 0;
                            String vehicleList = String.Empty;
                            String info = String.Format(Messages.ADM_PARKING_INFO, parking.id);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + info);
                            foreach (ParkedCarModel parkedCar in Parking.parkedCars)
                            {
                                if (parkedCar.parkingId == parking.id)
                                {
                                    vehicleList += parkedCar.vehicle.model + " LS-" + parkedCar.vehicle.id + " ";
                                    vehicles++;
                                }
                            }

                            // Mostramos la lista de vehículos o el mensaje de parking vacío
                            if (vehicles > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + vehicleList);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PARKING_EMPTY);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PARKING_NEAR);
                        }
                        break;
                    case "crear":
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                        {
                            if (arguments.Length == 2)
                            {
                                // Obtenemos el tipo de parking
                                if (Int32.TryParse(arguments[1], out int type) == true)
                                {
                                    if (type < Constants.PARKING_TYPE_PUBLIC || type > Constants.PARKING_TYPE_DEPOSIT)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_CREATE_COMMAND);
                                    }
                                    else
                                    {
                                        parking = new ParkingModel();
                                        parking.type = type;
                                        parking.position = player.Position;
                                        parking.id = Database.AddParking(parking);
                                        parking.parkingLabel = NAPI.TextLabel.CreateTextLabel(Parking.GetParkingLabelText(parking.type), parking.position, 20.0f, 0.75f, 4, new Color(255, 255, 255));
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_PARKING_CREATED);
                                        Parking.parkingList.Add(parking);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_CREATE_COMMAND);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_CREATE_COMMAND);
                            }
                        }
                        break;
                    case "modificar":
                        if (arguments.Length == 3)
                        {
                            if (parking != null)
                            {
                                switch (arguments[1].ToLower())
                                {
                                    case "casa":
                                        if (parking.type == Constants.PARKING_TYPE_GARAGE)
                                        {
                                            // Modificamos la casa adjunta
                                            if (Int32.TryParse(arguments[2], out int houseId) == true)
                                            {
                                                parking.houseId = houseId;
                                                Database.UpdateParking(parking);

                                                // Mandamos el mensaje al administrador
                                                String message = String.Format(Messages.ADM_PARKING_HOUSE_MODIFIED, houseId);
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_MODIFY_COMMAND);
                                            }
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PARKING_NOT_GARAGE);
                                        }
                                        break;
                                    case "plazas":
                                        int slots = 0;
                                        if (Int32.TryParse(arguments[2], out slots) == true)
                                        {
                                            parking.capacity = slots;
                                            Database.UpdateParking(parking);
                                            parking.parkingLabel = NAPI.TextLabel.CreateTextLabel(Parking.GetParkingLabelText(parking.type), parking.position, 20.0f, 0.75f, 4, new Color(255, 255, 255));

                                            // Mandamos el mensaje al administrador
                                            String message = String.Format(Messages.ADM_PARKING_SLOTS_MODIFIED, slots);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_MODIFY_COMMAND);
                                        }
                                        break;
                                    case "tipo":
                                        int type = 0;
                                        if (Int32.TryParse(arguments[2], out type) == true)
                                        {
                                            parking.type = type;
                                            Database.UpdateParking(parking);

                                            // Mandamos el mensaje al administrador
                                            String message = String.Format(Messages.ADM_PARKING_TYPE_MODIFIED, type);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_MODIFY_COMMAND);
                                        }
                                        break;
                                    default:
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_MODIFY_COMMAND);
                                        break;
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PARKING_NEAR);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_MODIFY_COMMAND);
                        }
                        break;
                    case "eliminar":
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                        {
                            if (parking != null)
                            {
                                NAPI.Entity.DeleteEntity(parking.parkingLabel);
                                Database.DeleteParking(parking.id);
                                Parking.parkingList.Remove(parking);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_PARKING_DELETED);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PARKING_NEAR);
                            }
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_COMMAND);
                        break;
                }
            }
        }

        [Command("pos")]
        public void Pos(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                Vector3 position = NAPI.Entity.GetEntityPosition(player);
                NAPI.Util.ConsoleOutput("{0},{1},{2}", player.Position.X, player.Position.Y, player.Position.Z);
            }
        }

        [Command("revivir", Messages.GEN_REVIVE_COMMAND)]
        public void RevivirCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // Obtenemos el jugador objetivo
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_KILLED) != 0)
                    {
                        Emergency.DestroyDeathTimer(target);
                        String playerMessage = String.Format(Messages.ADM_PLAYER_REVIVED, target.Name);
                        String targetMessage = String.Format(Messages.SUC_ADMIN_REVIVED, player.SocialClubName);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SUCCESS + targetMessage);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_DEAD);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("clima", "USO: /clima [0-12]")]
        public void Command_Weather(Client player, int weather)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                if (weather < 0 || weather > 13)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + "El clima debe estar comprendido entre 0 y 12.");
                }
                else
                {
                    NAPI.World.SetWeather(weather.ToString());
                    NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + "El clima ha sido cambiado a " + weather + " por " + player.Name);
                }
            }
        }

        [Command("jail", Messages.GEN_JAIL_COMMAND, GreedyArg = true)]
        public void JailCommand(Client player, String args)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                int jailTime = 0;
                String[] arguments = args.Trim().Split(' ');

                if (arguments.Length > 2)
                {
                    Client target = null;
                    String reason = String.Empty;

                    if (Int32.TryParse(arguments[0], out int targetId) == true)
                    {
                        target = Globals.GetPlayerById(targetId);
                        if (Int32.TryParse(arguments[1], out jailTime) == true)
                        {
                            reason = String.Join(" ", arguments.Skip(2));
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_JAIL_COMMAND);
                        }
                    }
                    else if (arguments.Length > 3)
                    {
                        target = NAPI.Player.GetPlayerFromName(arguments[0] + " " + arguments[1]);
                        if (Int32.TryParse(arguments[2], out jailTime) == true)
                        {
                            reason = String.Join(" ", arguments.Skip(3));
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_JAIL_COMMAND);
                        }
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_JAIL_COMMAND);
                        return;
                    }

                    // Movemos al jugador a la posición
                    NAPI.Entity.SetEntityPosition(target, new Vector3(1651.441f, 2569.83f, 45.56486f));
                    NAPI.Entity.SetEntityDimension(target, 0);

                    // Establecemos el tiempo de encarcelamiento
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAILED, jailTime);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAIL_TYPE, Constants.JAIL_TYPE_OOC);

                    // Mandamos el mensaje al servidor
                    String message = String.Format(Messages.ADM_PLAYER_JAILED, target.Name, jailTime, reason);
                    NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + message);

                    // Añadimos el log administrativo
                    Database.AddAdminLog(player.SocialClubName, target.Name, "jail", jailTime, reason);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_JAIL_COMMAND);
                }
            }
        }

        [Command("kick", Messages.GEN_KICK_COMMAND, GreedyArg = true)]
        public void KickCommand(Client player, String targetString, string reason)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // Obtenemos el jugador objetivo
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);
                NAPI.Player.KickPlayer(target, reason);
                NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + target.Name + " ha sido expulsado por " + player.Name + " motivo: " + reason);
            }
        }

        [Command("kickall")]
        public void KickAllCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (target != player)
                    {
                        NAPI.Player.KickPlayer(target, String.Empty);
                    }
                }

                // Mandamos el mensaje al administrador
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_KICKED_ALL);
            }
        }

        [Command("ban", Messages.GEN_BAN_COMMAND, GreedyArg = true)]
        public void Ban(Client player, String targetString, string reason)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
            {
                // Obtenemos el jugador objetivo
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);
                NAPI.Player.BanPlayer(target, reason);
                NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + target.Name + " ha sido baneado por " + player.Name + " motivo: " + reason);
            }
        }

        [Command("vida")]
        public void VidaCommand(Client player, String targetString, int health)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
            {
                // Obtenemos el jugador objetivo
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + "Has cambiado la salud de " + target.Name + " por " + health);
                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + "El miembro del staff " + player.Name + " te ha cambiado la salud por " + health);
                NAPI.Player.SetPlayerHealth(target, health);
            }
        }

        [Command("save")]
        public void SaveCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // Indicamos el mensaje
                String message = String.Empty;

                // Indicamos el comienzo
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_SAVE_START);

                // Guardamos todos los negocios
                Database.UpdateAllBusiness(Business.businessList);
                message = String.Format(Messages.ADM_SAVE_BUSINESS, Business.businessList.Count);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);

                // Guardamos todos los personajes
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                    {
                        PlayerModel character = new PlayerModel();

                        // Lista de datos no sincronizados
                        character.position = NAPI.Entity.GetEntityPosition(target);
                        character.rotation = NAPI.Entity.GetEntityRotation(target);
                        character.health = NAPI.Player.GetPlayerHealth(target);
                        character.armor = NAPI.Player.GetPlayerArmor(target);
                        character.id = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
                        character.phone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);
                        character.radio = NAPI.Data.GetEntityData(target, EntityData.PLAYER_RADIO);
                        character.killed = NAPI.Data.GetEntityData(target, EntityData.PLAYER_KILLED);
                        character.faction = NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION);
                        character.job = NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB);
                        character.rank = NAPI.Data.GetEntityData(target, EntityData.PLAYER_RANK);
                        character.duty = NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY);
                        character.carKeys = NAPI.Data.GetEntityData(target, EntityData.PLAYER_VEHICLE_KEYS);
                        character.documentation = NAPI.Data.GetEntityData(target, EntityData.PLAYER_DOCUMENTATION);
                        character.licenses = NAPI.Data.GetEntityData(target, EntityData.PLAYER_LICENSES);
                        character.insurance = NAPI.Data.GetEntityData(target, EntityData.PLAYER_MEDICAL_INSURANCE);
                        character.weaponLicense = NAPI.Data.GetEntityData(target, EntityData.PLAYER_WEAPON_LICENSE);
                        character.houseRent = NAPI.Data.GetEntityData(target, EntityData.PLAYER_RENT_HOUSE);
                        character.houseEntered = NAPI.Data.GetEntityData(target, EntityData.PLAYER_HOUSE_ENTERED);
                        character.businessEntered = NAPI.Data.GetEntityData(target, EntityData.PLAYER_BUSINESS_ENTERED);
                        character.employeeCooldown = NAPI.Data.GetEntityData(target, EntityData.PLAYER_EMPLOYEE_COOLDOWN);
                        character.jobCooldown = NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB_COOLDOWN);
                        character.jobDeliver = NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB_DELIVER);
                        character.jobPoints = NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB_POINTS);
                        character.played = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PLAYED);
                        character.jailed = NAPI.Data.GetEntityData(target, EntityData.PLAYER_JAIL_TYPE) + "," + NAPI.Data.GetEntityData(target, EntityData.PLAYER_JAILED);

                        // Lista de datos sincronizados
                        character.money = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY);
                        character.bank = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_BANK);

                        // Guardado del personaje en base de datos
                        Database.SaveCharacterInformation(character);
                    }
                }

                // Indicamos el fin
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + "Todos los personajes guardados.");

                // Guardamos los vehículos
                List<VehicleModel> vehicleList = new List<VehicleModel>();

                foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
                {
                    if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == 0 && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PARKING) == 0)
                    {
                        // Obtenemos los valores necesarios para recrear el vehículo
                        VehicleModel vehicleModel = new VehicleModel();
                        vehicleModel.id = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                        vehicleModel.model = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                        vehicleModel.position = NAPI.Entity.GetEntityPosition(vehicle);
                        vehicleModel.rotation = NAPI.Entity.GetEntityRotation(vehicle);
                        vehicleModel.dimension = NAPI.Entity.GetEntityDimension(vehicle);
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

                        // Añadimos el vehículo a la lista
                        vehicleList.Add(vehicleModel);
                    }
                }

                // Guardamos la lista de vehículos
                Database.SaveAllVehicles(vehicleList);

                // Indicamos el fin
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + "Todos los vehículos guardados.");

                // Indicamos el fin
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_SAVE_FINISH);
            }
        }

        [Command("aservicio")]
        public void AservicioCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ADMIN_ON_DUTY))
                {
                    NAPI.Entity.SetEntityInvincible(player, false);
                    NAPI.Player.ResetPlayerNametagColor(player);
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ADMIN_ON_DUTY);
                    NAPI.Notification.SendNotificationToPlayer(player, Messages.INF_PLAYER_ADMIN_FREE_TIME);
                }
                else
                {
                    NAPI.Entity.SetEntityInvincible(player, true);
                    NAPI.Player.SetPlayerNametagColor(player, 231, 133, 46);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_ADMIN_ON_DUTY, true);
                    NAPI.Notification.SendNotificationToPlayer(player, Messages.INF_PLAYER_ADMIN_ON_DUTY);
                }
            }
        }

        [Command("dudas")]
        public void DudasCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Lista de dudas");
                foreach (AdminTicketModel adminTicket in Globals.adminTicketList)
                {
                    Client target = Globals.GetPlayerById(adminTicket.playerId);
                    String ticket = target.Name + " (" + adminTicket.playerId + "): " + adminTicket.question;
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + ticket);
                }
            }
        }

        [Command("rduda", Messages.GEN_ANSWER_HELP_REQUEST, GreedyArg = true)]
        public void RdudaCommand(Client player, int ticket, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                foreach (AdminTicketModel adminTicket in Globals.adminTicketList)
                {
                    if (adminTicket.playerId == ticket)
                    {
                        Client target = Globals.GetPlayerById(adminTicket.playerId);

                        // Respondemos al jugador
                        String targetMessage = String.Format(Messages.INF_TICKET_ANSWER, message);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                        // Avisamos al administrador
                        String playerMessage = String.Format(Messages.ADM_TICKET_ANSWERED, ticket);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);

                        // Borramos la duda
                        Globals.adminTicketList.Remove(adminTicket);
                        return;
                    }
                }

                // No se ha encontrado ningún ticket con ese identificador
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ADMIN_TICKET_NOT_FOUND);
            }
        }

        [Command("a", Messages.GEN_ADMIN_TEXT_COMMAND, GreedyArg = true)]
        public void ACommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {

                // Miramos si el mensaje excede la longitud máxima
                String secondMessage = String.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // El mensaje tiene una longitud de dos líneas
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_ADMIN_INFO + "((Staff [ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_ADMIN_INFO + "((Staff [ID: " + playerId + "] " + player.Name + ": " + message + "))");
                        if (secondMessage.Length > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + secondMessage + "))");
                        }
                    }
                }
            }
        }

        [Command("recon", Messages.GEN_RECON_COMMAND, GreedyArg = true)]
        public void ReconCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
                else if (NAPI.Player.IsPlayerSpectating(target) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_SPECTATING);
                }
                else if (target == player)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CANT_SPECT_SELF);
                }
                else
                {
                    String message = String.Format(Messages.ADM_SPECTATING_PLAYER, target.Name);
                    NAPI.Player.SetPlayerToSpectator(player);
                    NAPI.Player.SetPlayerToSpectatePlayer(player, target);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                }
            }
        }

        [Command("recoff")]
        public void RecoffCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                if (NAPI.Player.IsPlayerSpectating(player) == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_SPECTATING);
                }
                else
                {
                    NAPI.Player.UnspectatePlayer(player);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_SPECT_STOPPED);
                }
            }
        }

        [Command("info", "USO: /info [Id | Nombre jugador]")]
        public void InfoCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if(target != null)
                {
                    String sex = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? "Masculino" : "Femenino";
                    String played = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PLAYED) + " minutos";
                    String age = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_AGE) + " años";
                    String money = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY) + "$";
                    String bank = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_BANK) + "$";
                    String job = "Sin trabajo";
                    String faction = "Sin facción";
                    String rank = "Sin rango";
                    String houses = String.Empty;
                    String ownedVehicles = String.Empty;
                    String lentVehicles = NAPI.Data.GetEntityData(target, EntityData.PLAYER_VEHICLE_KEYS);

                    // Miramos si tiene un trabajo
                    foreach (JobModel jobModel in Constants.JOB_LIST)
                    {
                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) == jobModel.job)
                        {
                            job = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? jobModel.descriptionMale : jobModel.descriptionFemale;
                            break;
                        }
                    }

                    // Miramos si tiene una facción
                    foreach (FactionModel factionModel in Constants.FACTION_RANK_LIST)
                    {
                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == factionModel.faction && NAPI.Data.GetEntityData(target, EntityData.PLAYER_RANK) == factionModel.rank)
                        {
                            switch (factionModel.faction)
                            {
                                case Constants.FACTION_POLICE:
                                    faction = "Policía";
                                    break;
                                case Constants.FACTION_EMERGENCY:
                                    faction = "Emergencias";
                                    break;
                                case Constants.FACTION_NEWS:
                                    faction = "Weazel News";
                                    break;
                                case Constants.FACTION_TOWNHALL:
                                    faction = "Ayuntamiento";
                                    break;
                                case Constants.FACTION_TAXI_DRIVER:
                                    faction = "Servicio de transportes";
                                    break;
                                default:
                                    faction = "Sin facción";
                                    break;
                            }

                            // Establecemos el rango
                            rank = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? factionModel.descriptionMale : factionModel.descriptionFemale;
                            break;
                        }
                    }

                    // Miramos si tiene alguna casa alquilada
                    if (NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_RENT_HOUSE) > 0)
                    {
                        houses += " " + NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_RENT_HOUSE);
                    }

                    // Miramos si tiene alguna casa en propiedad
                    foreach (HouseModel house in House.houseList)
                    {
                        if (house.owner == target.Name)
                        {
                            houses += " " + house.id;
                        }
                    }

                    // Miramos si tiene algún vehículo en propiedad
                    foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
                    {
                        if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER) == target.Name)
                        {
                            ownedVehicles += " " + NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                        }
                    }

                    // Miramos entre los vehículos aparcados
                    foreach (ParkedCarModel parkedVehicle in Parking.parkedCars)
                    {
                        if (parkedVehicle.vehicle.owner == target.Name)
                        {
                            ownedVehicles += " " + parkedVehicle.vehicle.id;
                        }
                    }

                    // Mostramos la información
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Datos básicos:");
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Nombre: " + target.Name + "; Sexo: " + sex + "; Edad: " + age + "; Dinero: " + money + "; Banco: " + bank);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + " ");
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Datos de empleo:");
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Trabajo: " + job + "; Facción: " + faction + "; Rango: " + rank);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + " ");
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Propiedades:");
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Casas: " + houses);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Vehículos propios: " + ownedVehicles);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Vehículos cedidos: " + lentVehicles);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + " ");
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Otros datos:");
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Tiempo jugado: " + played);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }
    }
}
