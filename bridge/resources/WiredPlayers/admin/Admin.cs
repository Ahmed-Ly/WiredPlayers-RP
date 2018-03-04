using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.vehicles;
using WiredPlayers.database;
using WiredPlayers.emergency;
using WiredPlayers.business;
using WiredPlayers.parking;
using WiredPlayers.house;
using WiredPlayers.weapons;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WiredPlayers.admin
{
    public class Admin : Script
    {
        public static List<PermissionModel> permissionList;

        private bool HasUserCommandPermission(Client player, String command, String option = "")
        {
            bool hasPermission = false;
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

            foreach (PermissionModel permission in permissionList)
            {
                if (permission.playerId == playerId && command == permission.command)
                {
                    // We check whether it's a command option or just the command
                    if (option == String.Empty || option == permission.option)
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }

            return hasPermission;
        }

        private void SendHouseInfo(Client player, HouseModel house)
        {
            String title = String.Format(Messages.GEN_HOUSE_CHECK_TITLE, house.id);
            NAPI.Chat.SendChatMessageToPlayer(player, title);
            NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_NAME + house.name);
            NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_IPL + house.ipl);
            NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_OWNER + house.owner);
            NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_PRICE + house.price);
            NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_STATUS + house.status);
        }

        [Command(Messages.COM_SKIN, Messages.GEN_SKIN_COMMAND)]
        public void SkinCommand(Client player, String pedModel)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                NAPI.Player.SetPlayerSkin(player, pedHash);
            }
        }

        [Command(Messages.COM_ADMIN, Messages.GEN_ADMIN_COMMAND, GreedyArg = true)]
        public void AdminCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // Check the message length
                String secondMessage = String.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // Message needs to be printed in two lines
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                // We send the message to all the players in the server
                NAPI.Chat.SendChatMessageToAll(secondMessage.Length > 0 ? Constants.COLOR_ADMIN_INFO + Messages.GEN_ADMIN_NOTICE + message + "..." : Constants.COLOR_ADMIN_INFO + Messages.GEN_ADMIN_NOTICE + message);
                if (secondMessage.Length > 0)
                {
                    NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + secondMessage);
                }
            }
        }

        [Command(Messages.COM_COORD, Messages.GEN_COORD_COMMAND)]
        public void CoordCommand(Client player, float posX, float posY, float posZ)
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

        [Command(Messages.COM_TP, Messages.GEN_TP_COMMAND, GreedyArg = true)]
        public void TpCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // We get the player from the input string
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    String message = String.Format(Messages.ADM_GOTO_PLAYER, target.Name);

                    // We get interior variables from the target player
                    int targetBusiness = NAPI.Data.GetEntityData(target, EntityData.PLAYER_BUSINESS_ENTERED);
                    int targetHouse = NAPI.Data.GetEntityData(target, EntityData.PLAYER_HOUSE_ENTERED);
                    uint targetDimension = NAPI.Entity.GetEntityDimension(target);
                    Vector3 position = NAPI.Entity.GetEntityPosition(target);

                    // Change player's position and dimension
                    NAPI.Entity.SetEntityPosition(player, position);
                    NAPI.Entity.SetEntityDimension(player, targetDimension);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, targetBusiness);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, targetHouse);

                    // Confirmation message sent to the command executor
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Messages.COM_BRING, Messages.GEN_BRING_COMMAND, GreedyArg = true)]
        public void BringCommand(Client player, String targetString)
        {

            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // We get the player from the input string
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    String message = String.Format(Messages.ADM_BRING_PLAYER, player.SocialClubName);

                    // We get interior variables from the player
                    int playerBusiness = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                    int playerHouse = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                    uint playerDimension = NAPI.Entity.GetEntityDimension(player);
                    Vector3 position = NAPI.Entity.GetEntityPosition(player);

                    // Change target's position and dimension
                    NAPI.Entity.SetEntityPosition(target, position);
                    NAPI.Entity.SetEntityDimension(target, playerDimension);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_BUSINESS_ENTERED, playerBusiness);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_HOUSE_ENTERED, playerHouse);

                    // Confirmation message sent to the command executor
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + message);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Messages.COM_GUN, Messages.GEN_GUN_COMMAND)]
        public void GunCommand(Client player, String targetString, String weaponName, int ammo)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
            {
                // We get the player from the input string
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
                        // Give the weapon to the player
                        Weapons.GivePlayerNewWeapon(target, weapon, ammo, false);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Messages.COM_VEHICLE, Messages.GEN_VEHICLE_COMMAND, GreedyArg = true)]
        public void VehicleCommand(Client player, String args)
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
                        case Messages.ARG_INFO:
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
                                    String title = String.Format(Messages.GEN_VEHICLE_CHECK_TITLE, vehicleId);
                                    String model = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_MODEL);
                                    String owner = NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_OWNER);
                                    NAPI.Chat.SendChatMessageToPlayer(player, title);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_MODEL + model);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_OWNER + owner);
                                }
                            }
                            break;
                        case Messages.ARG_CREATE:
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                if (arguments.Length == 4)
                                {
                                    String[] firstColorArray = arguments[2].Split(',');
                                    String[] secondColorArray = arguments[3].Split(',');
                                    if (firstColorArray.Length == Constants.TOTAL_COLOR_ELEMENTS && secondColorArray.Length == Constants.TOTAL_COLOR_ELEMENTS)
                                    {
                                        // Basic data for vehicle creation
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
                        case Messages.ARG_MODIFY:
                            if (arguments.Length > 1)
                            {
                                switch (arguments[1].ToLower())
                                {
                                    case Messages.ARG_COLOR:
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
                                                            NAPI.Util.ConsoleOutput("[EXCEPTION Vehicle modify color] " + ex.Message);
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
                                    case Messages.ARG_DIMENSION:
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
                                    case Messages.ARG_FACTION:
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
                                    case Messages.ARG_POSITION:
                                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (NAPI.Player.IsPlayerInAnyVehicle(player) == true)
                                            {
                                                veh = NAPI.Player.GetPlayerVehicle(player);
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
                                    case Messages.ARG_OWNER:
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
                        case Messages.ARG_REMOVE:
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
                        case Messages.ARG_REPAIR:
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                NAPI.Vehicle.RepairVehicle(NAPI.Player.GetPlayerVehicle(player));
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_VEHICLE_REPAIRED);
                            }
                            break;
                        case Messages.ARG_LOCK:
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
                        case Messages.ARG_START:
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                if (NAPI.Player.GetPlayerVehicleSeat(player) == (int)VehicleSeat.Driver)
                                {
                                    veh = NAPI.Player.GetPlayerVehicle(player);
                                    NAPI.Vehicle.SetVehicleEngineStatus(veh, true);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
                                }
                            }
                            break;
                        case Messages.ARG_BRING:
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                if (arguments.Length == 2 && int.TryParse(arguments[1], out vehicleId) == true)
                                {
                                    veh = Vehicles.GetVehicleById(vehicleId);
                                    if (veh != null)
                                    {
                                        // Get the vehicle to the player's position
                                        NAPI.Entity.SetEntityPosition(veh, player.Position);
                                        NAPI.Data.SetEntityData(veh, EntityData.VEHICLE_POSITION, player.Position);

                                        // Send the message to the player
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
                        case Messages.ARG_TP:
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
                                            // Teleport player to the parking
                                            ParkingModel parking = Parking.GetParkingById(vehModel.parking);
                                            NAPI.Entity.SetEntityPosition(player, parking.position);

                                            // Send the message to the player
                                            String message = String.Format(Messages.ADM_VEHICLE_GOTO, vehicleId);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                    }
                                    else
                                    {
                                        // Get the player to the vehicle's position
                                        Vector3 position = NAPI.Entity.GetEntityPosition(veh);
                                        NAPI.Entity.SetEntityPosition(player, position);

                                        // Send the message to the player
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

        [Command(Messages.COM_GO, Messages.GEN_GO_COMMAND)]
        public void GoCommand(Client player, String location)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                switch (location.ToLower())
                {
                    case Messages.ARG_WORKSHOP:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1204.13f, -1489.49f, 4.34967f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_ELECTRONICS:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1148.98f, -1608.94f, 4.41592f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_POLICE:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1111.952f, -824.9194f, 19.31578f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_TOWNHALL:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1285.544f, -567.0439f, 31.71239f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_LICENSE:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-70f, -1100f, 28f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_VANILLA:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(120f, -1400f, 30f));
                        NAPI.Entity.SetEntityDimension(player, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        break;
                    case Messages.ARG_HOSPITAL:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1385.481f, -976.4036f, 9.273162f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_NEWS:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-600f, -950f, 25f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_BAHAMA:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-1400f, -590f, 30f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_MECHANIC:
                        NAPI.Entity.SetEntityPosition(player, new Vector3(492f, -1300f, 30f));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        break;
                    case Messages.ARG_GARBAGE:
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

        [Command(Messages.COM_BUSINESS, Messages.GEN_BUSINESS_COMMAND, GreedyArg = true)]
        public void BusinessCommand(Client player, String args)
        {
            if (HasUserCommandPermission(player, Messages.COM_BUSINESS) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                if (args.Trim().Length > 0)
                {
                    BusinessModel business = new BusinessModel();
                    String[] arguments = args.Split(' ');
                    String message = String.Empty;
                    switch (arguments[0].ToLower())
                    {
                        case Messages.ARG_INFO:
                            break;
                        case Messages.ARG_CREATE:
                            if (HasUserCommandPermission(player, Messages.COM_BUSINESS, Messages.ARG_CREATE) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                            {
                                if (arguments.Length == 2)
                                {
                                    // We get the business type
                                    if (Int32.TryParse(arguments[1], out int type) == true)
                                    {
                                        business.type = type;
                                        business.ipl = Business.GetBusinessTypeIpl(type);
                                        business.position = player.Position;
                                        business.dimension = player.Dimension;
                                        business.multiplier = 3.0f;
                                        business.owner = String.Empty;
                                        business.locked = false;
                                        business.name = Messages.GEN_BUSINESS;
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
                        case Messages.ARG_MODIFY:
                            if (HasUserCommandPermission(player, Messages.COM_BUSINESS, Messages.ARG_MODIFY) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                            {
                                business = Business.GetClosestBusiness(player);
                                if (business != null)
                                {
                                    if (arguments.Length > 1)
                                    {
                                        switch (arguments[1].ToLower())
                                        {
                                            case Messages.ARG_NAME:
                                                if (arguments.Length > 2)
                                                {
                                                    // We change business name
                                                    String businessName = String.Join(" ", arguments.Skip(2));
                                                    business.name = businessName;
                                                    NAPI.TextLabel.SetTextLabelText(business.businessLabel, businessName);
                                                    Database.UpdateBusiness(business);

                                                    // Confirmation message sent to the player
                                                    message = String.Format(Messages.ADM_BUSINESS_NAME_MODIFIED, businessName);
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                                }
                                                else
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BUSINESS_MODIFY_NAME_COMMAND);
                                                }
                                                break;
                                            case Messages.ARG_TYPE:
                                                if (arguments.Length == 3)
                                                {
                                                    // We get business type
                                                    if (Int32.TryParse(arguments[2], out int businessType) == true)
                                                    {
                                                        // Changing business type
                                                        business.type = businessType;
                                                        business.ipl = Business.GetBusinessTypeIpl(businessType);
                                                        Database.UpdateBusiness(business);

                                                        // Confirmation message sent to the player
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
                        case Messages.ARG_REMOVE:
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

        [Command(Messages.COM_CHARACTER, Messages.GEN_CHARACTER_COMMAND)]
        public void CharacterCommand(Client player, String action, String name = "", String surname = "", String amount = "")
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                Client target = null;

                // We check whether we have an id or a full name
                if (Int32.TryParse(name, out int targetId) == true)
                {
                    target = Globals.GetPlayerById(targetId);
                    amount = surname;
                }
                else
                {
                    target = NAPI.Player.GetPlayerFromName(name + " " + surname);
                }

                // We check whether the player is connected
                if (target != null && NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                {
                    // Getting the amount
                    if (Int32.TryParse(amount, out int value) == true)
                    {
                        String message = String.Empty;
                        switch (action.ToLower())
                        {
                            case Messages.ARG_BANK:
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                                {
                                    NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_BANK, value);
                                    message = String.Format(Messages.ADM_PLAYER_BANK_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case Messages.ARG_MONEY:
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                                {
                                    NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_MONEY, value);
                                    message = String.Format(Messages.ADM_PLAYER_MONEY_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case Messages.ARG_FACTION:
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                {
                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, value);
                                    message = String.Format(Messages.ADM_PLAYER_FACTION_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case Messages.ARG_JOB:
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                {
                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB, value);
                                    message = String.Format(Messages.ADM_PLAYER_JOB_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case Messages.ARG_RANK:
                                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                {
                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, value);
                                    message = String.Format(Messages.ADM_PLAYER_RANK_MODIFIED, value, target.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                }
                                break;
                            case Messages.ARG_DIMENSION:
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

        [Command(Messages.COM_HOUSE, Messages.GEN_HOUSE_COMMAND, GreedyArg = true)]
        public void HouseCommand(Client player, String args)
        {
            if (HasUserCommandPermission(player, Messages.COM_HOUSE) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                HouseModel house = House.GetClosestHouse(player);
                String[] arguments = args.Split(' ');
                switch (arguments[0].ToLower())
                {
                    case Messages.ARG_INFO:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                        {
                            // We get house identifier
                            if (arguments.Length == 2 && int.TryParse(arguments[1], out int houseId) == true)
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
                            else if (arguments.Length == 1)
                            {
                                SendHouseInfo(player, house);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_HOUSE_INFO_COMMAND);
                            }
                        }
                        break;
                    case Messages.ARG_CREATE:
                        if (HasUserCommandPermission(player, Messages.COM_HOUSE, Messages.ARG_CREATE) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                        {
                            String houseLabel = String.Empty;
                            house = new HouseModel();
                            house.ipl = Constants.HOUSE_IPL_LIST[0].ipl;
                            house.name = Messages.GEN_HOUSE;
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
                    case Messages.ARG_MODIFY:
                        if (arguments.Length > 2)
                        {
                            String message = String.Empty;

                            if (Int32.TryParse(arguments[2], out int value) == true)
                            {
                                // Numeric modifications
                                switch (arguments[1].ToLower())
                                {
                                    case Messages.ARG_INTERIOR:
                                        if (HasUserCommandPermission(player, Messages.COM_HOUSE, Messages.ARG_INTERIOR) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (value >= 0 && value < Constants.HOUSE_IPL_LIST.Count)
                                            {
                                                house.ipl = Constants.HOUSE_IPL_LIST[value].ipl;
                                                Database.UpdateHouse(house);

                                                // Confirmation message sent to the player
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
                                    case Messages.ARG_PRICE:
                                        if (HasUserCommandPermission(player, Messages.COM_HOUSE, Messages.ARG_PRICE) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            if (value > 0)
                                            {
                                                house.price = value;
                                                house.status = Constants.HOUSE_STATE_BUYABLE;
                                                NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                                Database.UpdateHouse(house);

                                                // Confirmation message sent to the player
                                                message = String.Format(Messages.ADM_HOUSE_PRICE_MODIFIED, value);
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_PRICE_MODIFY);
                                            }
                                        }
                                        break;
                                    case Messages.ARG_STATE:
                                        if (value >= 0 && value < 3)
                                        {
                                            house.status = value;
                                            NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                            Database.UpdateHouse(house);

                                            // Confirmation message sent to the player
                                            message = String.Format(Messages.ADM_HOUSE_STATUS_MODIFIED, value);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_STATUS_MODIFY);
                                        }
                                        break;
                                    case Messages.ARG_RENT:
                                        if (value > 0)
                                        {
                                            house.rental = value;
                                            house.status = Constants.HOUSE_STATE_RENTABLE;
                                            NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                            Database.UpdateHouse(house);

                                            // Confirmation message sent to the player
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

                                // Text based modifications
                                switch (arguments[1].ToLower())
                                {
                                    case Messages.ARG_OWNER:
                                        if (HasUserCommandPermission(player, Messages.COM_HOUSE, Messages.ARG_OWNER) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            house.owner = name.Trim();
                                            Database.UpdateHouse(house);

                                            // Confirmation message sent to the player
                                            message = String.Format(Messages.ADM_HOUSE_OWNER_MODIFIED);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        break;
                                    case Messages.ARG_NAME:
                                        if (HasUserCommandPermission(player, Messages.COM_HOUSE, Messages.ARG_NAME) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                                        {
                                            house.name = name.Trim();
                                            NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                            Database.UpdateHouse(house);

                                            // Confirmation message sent to the player
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
                    case Messages.ARG_REMOVE:
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
                    case Messages.ARG_TP:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
                        {
                            // We get the house
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

        [Command(Messages.COM_PARKING, Messages.GEN_PARKING_COMMAND, GreedyArg = true)]
        public void ParkingCommand(Client player, String args)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                String[] arguments = args.Split(' ');
                ParkingModel parking = Parking.GetClosestParking(player);
                switch (arguments[0].ToLower())
                {
                    case Messages.ARG_INFO:
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
                            
                            if (vehicles > 0)
                            {
                                // We show all the vehicles in this parking
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + vehicleList);
                            }
                            else
                            {
                                // There are no vehicles in this parking
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PARKING_EMPTY);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PARKING_NEAR);
                        }
                        break;
                    case Messages.ARG_CREATE:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
                        {
                            if (arguments.Length == 2)
                            {
                                // We get the parking type
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
                    case Messages.ARG_MODIFY:
                        if (arguments.Length == 3)
                        {
                            if (parking != null)
                            {
                                switch (arguments[1].ToLower())
                                {
                                    case Messages.ARG_HOUSE:
                                        if (parking.type == Constants.PARKING_TYPE_GARAGE)
                                        {
                                            // We link the house to this parking
                                            if (Int32.TryParse(arguments[2], out int houseId) == true)
                                            {
                                                parking.houseId = houseId;
                                                Database.UpdateParking(parking);

                                                // Confirmation message sent to the player
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
                                    case Messages.ARG_PLACES:
                                        int slots = 0;
                                        if (Int32.TryParse(arguments[2], out slots) == true)
                                        {
                                            parking.capacity = slots;
                                            Database.UpdateParking(parking);
                                            parking.parkingLabel = NAPI.TextLabel.CreateTextLabel(Parking.GetParkingLabelText(parking.type), parking.position, 20.0f, 0.75f, 4, new Color(255, 255, 255));

                                            // Confirmation message sent to the player
                                            String message = String.Format(Messages.ADM_PARKING_SLOTS_MODIFIED, slots);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);
                                        }
                                        else
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_PARKING_MODIFY_COMMAND);
                                        }
                                        break;
                                    case Messages.ARG_TYPE:
                                        int type = 0;
                                        if (Int32.TryParse(arguments[2], out type) == true)
                                        {
                                            parking.type = type;
                                            Database.UpdateParking(parking);

                                            // Confirmation message sent to the player
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
                    case Messages.ARG_REMOVE:
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

        [Command(Messages.COM_POS)]
        public void PosCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                Vector3 position = NAPI.Entity.GetEntityPosition(player);
                NAPI.Util.ConsoleOutput("{0},{1},{2}", player.Position.X, player.Position.Y, player.Position.Z);
            }
        }

        [Command(Messages.COM_REVIVE, Messages.GEN_REVIVE_COMMAND)]
        public void ReviveCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // We get the target player
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

        [Command(Messages.COM_WEATHER, Messages.GEN_WEATHER_COMMAND)]
        public void WeatherCommand(Client player, int weather)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                if (weather < 0 || weather > 13)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_WEATHER_VALUE_INVALID);
                }
                else
                {
                    NAPI.World.SetWeather(weather.ToString());

                    String message = String.Format(Messages.ADM_WEATHER_CHANGED, player.Name, weather);
                    NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + message);
                }
            }
        }

        [Command(Messages.COM_JAIL, Messages.GEN_JAIL_COMMAND, GreedyArg = true)]
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

                    // We move the player to the jail
                    NAPI.Entity.SetEntityPosition(target, new Vector3(1651.441f, 2569.83f, 45.56486f));
                    NAPI.Entity.SetEntityDimension(target, 0);

                    // We set jail type
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAILED, jailTime);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAIL_TYPE, Constants.JAIL_TYPE_OOC);

                    // Message sent to the whole server
                    String message = String.Format(Messages.ADM_PLAYER_JAILED, target.Name, jailTime, reason);
                    NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + message);

                    // We add the log in the database
                    Database.AddAdminLog(player.SocialClubName, target.Name, "jail", jailTime, reason);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_JAIL_COMMAND);
                }
            }
        }

        [Command(Messages.COM_KICK, Messages.GEN_KICK_COMMAND, GreedyArg = true)]
        public void KickCommand(Client player, String targetString, string reason)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                // We get the target player
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);
                
                NAPI.Player.KickPlayer(target, reason);

                //  Message sent to the whole server
                String message = String.Format(Messages.ADM_PLAYER_KICKED, player.Name, target.Name, reason);
                NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + message);

                // We add the log in the database
                Database.AddAdminLog(player.SocialClubName, target.Name, "kick", 0, reason);
            }
        }

        [Command(Messages.COM_KICKALL)]
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

                // Confirmation message sent to the player
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_KICKED_ALL);
            }
        }

        [Command(Messages.COM_BAN, Messages.GEN_BAN_COMMAND, GreedyArg = true)]
        public void BanCommand(Client player, String targetString, string reason)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
            {
                // We get the target player
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                NAPI.Player.BanPlayer(target, reason);

                String message = String.Format(Messages.ADM_PLAYER_BANNED, player.Name, target.Name, reason);
                NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + message);

                // We add the log in the database
                Database.AddAdminLog(player.SocialClubName, target.Name, "ban", 0, reason);
            }
        }

        [Command(Messages.COM_HEALTH, Messages.GEN_HEAL_COMMAND)]
        public void HealthCommand(Client player, String targetString, int health)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
            {
                // We get the target player
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                NAPI.Player.SetPlayerHealth(target, health);

                // We send the confirmation message to both players
                String playerMessage = String.Format(Messages.ADM_PLAYER_HEALTH, target.Name, health);
                String targetMessage = String.Format(Messages.ADM_TARGET_HEALTH, player.Name, health);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);
                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + targetMessage);
            }
        }

        [Command(Messages.COM_SAVE)]
        public void SaveCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                String message = String.Empty;

                // We print a message saying when the command starts
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_SAVE_START);

                // Saving all business
                Database.UpdateAllBusiness(Business.businessList);

                message = String.Format(Messages.ADM_SAVE_BUSINESS, Business.businessList.Count);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + message);

                // Saving all connected players
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                    {
                        PlayerModel character = new PlayerModel();

                        // Non shared data
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
                        character.rolePoints = NAPI.Data.GetEntityData(target, EntityData.PLAYER_ROLE_POINTS);
                        character.played = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PLAYED);
                        character.jailed = NAPI.Data.GetEntityData(target, EntityData.PLAYER_JAIL_TYPE) + "," + NAPI.Data.GetEntityData(target, EntityData.PLAYER_JAILED);

                        // Shared data
                        character.money = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY);
                        character.bank = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_BANK);

                        // Saving the character information into the database
                        Database.SaveCharacterInformation(character);
                    }
                }

                // All the characters saved
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_CHARACTERS_SAVED);

                // Vehicles saving
                List<VehicleModel> vehicleList = new List<VehicleModel>();

                foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
                {
                    if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == 0 && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PARKING) == 0)
                    {
                        VehicleModel vehicleModel = new VehicleModel();

                        // Getting the needed values to be stored
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

                        // We add the vehicle to the list
                        vehicleList.Add(vehicleModel);
                    }
                }

                // Saving the list into database
                Database.SaveAllVehicles(vehicleList);

                // All vehicles saved
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_VEHICLES_SAVED);

                // End of the command
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_SAVE_FINISH);
            }
        }

        [Command(Messages.COM_ADUTY)]
        public void ADutyCommand(Client player)
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

        [Command(Messages.COM_TICKETS)]
        public void TicketsCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TICKET_LIST);
                foreach (AdminTicketModel adminTicket in Globals.adminTicketList)
                {
                    Client target = Globals.GetPlayerById(adminTicket.playerId);
                    String ticket = target.Name + " (" + adminTicket.playerId + "): " + adminTicket.question;
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + ticket);
                }
            }
        }

        [Command(Messages.COM_ATICKET, Messages.GEN_ANSWER_HELP_REQUEST, GreedyArg = true)]
        public void ATicketCommand(Client player, int ticket, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                foreach (AdminTicketModel adminTicket in Globals.adminTicketList)
                {
                    if (adminTicket.playerId == ticket)
                    {
                        Client target = Globals.GetPlayerById(adminTicket.playerId);

                        // We send the answer to the player
                        String targetMessage = String.Format(Messages.INF_TICKET_ANSWER, message);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                        // We send the confirmation to the staff
                        String playerMessage = String.Format(Messages.ADM_TICKET_ANSWERED, ticket);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);

                        // Ticket removed
                        Globals.adminTicketList.Remove(adminTicket);
                        return;
                    }
                }

                // There's no ticket with that identifier
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ADMIN_TICKET_NOT_FOUND);
            }
        }

        [Command(Messages.COM_A, Messages.GEN_ADMIN_TEXT_COMMAND, GreedyArg = true)]
        public void ACommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
            {
                String secondMessage = String.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // We split the message in two lines
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_NONE)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_ADMIN_INFO + "((Staff [ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_ADMIN_INFO + "((Staff [ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                        if (secondMessage.Length > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + secondMessage + "))");
                        }
                    }
                }
            }
        }

        [Command(Messages.COM_RECON, Messages.GEN_RECON_COMMAND, GreedyArg = true)]
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

        [Command(Messages.COM_RECOFF)]
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

        [Command(Messages.COM_INFO, Messages.GEN_INFO_COMMAND)]
        public void InfoCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_SUPPORT)
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    // Get player's basic data
                    Globals.GetPlayerBasicData(player, target);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Messages.COM_POINTS, Messages.GEN_POINTS_COMMAND, GreedyArg = true)]
        public void PuntosCommand(Client player, String arguments)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_GAME_MASTER)
            {
                String[] args = arguments.Trim().Split(' ');
                if (args.Length == 3 || args.Length == 4)
                {
                    int rolePoints = 0;
                    Client target = null;

                    if (Int32.TryParse(args[1], out int targetId) == true)
                    {
                        target = Globals.GetPlayerById(targetId);
                        rolePoints = Int32.Parse(args[2]);
                    }
                    else
                    {
                        target = NAPI.Player.GetPlayerFromName(args[1] + " " + args[2]);
                        rolePoints = Int32.Parse(args[3]);
                    }

                    if (target != null && NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                    {
                        // We get player's role points
                        String playerMessage = String.Empty;
                        String targetMessage = String.Empty;
                        int targetRolePoints = NAPI.Data.GetEntityData(target, EntityData.PLAYER_ROLE_POINTS);

                        switch (args[0].ToLower())
                        {
                            case Messages.ARG_GIVE:
                                // We give role points to the player
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_ROLE_POINTS, targetRolePoints + rolePoints);

                                playerMessage = String.Format(Messages.ADM_ROLE_POINTS_GIVEN, target.Name, rolePoints);
                                targetMessage = String.Format(Messages.ADM_ROLE_POINTS_RECEIVED, player.SocialClubName, rolePoints);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + targetMessage);

                                break;
                            case Messages.ARG_REMOVE:
                                // We remove role points to the player
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_ROLE_POINTS, targetRolePoints - rolePoints);

                                playerMessage = String.Format(Messages.ADM_ROLE_POINTS_REMOVED, target.Name, rolePoints);
                                targetMessage = String.Format(Messages.ADM_ROLE_POINTS_LOST, player.SocialClubName, rolePoints);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + targetMessage);
                                break;
                            case Messages.ARG_SET:
                                // We set player's role points
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_ROLE_POINTS, rolePoints);

                                playerMessage = String.Format(Messages.ADM_ROLE_POINTS_SET, target.Name, rolePoints);
                                targetMessage = String.Format(Messages.ADM_ROLE_POINTS_ESTABLISHED, player.SocialClubName, rolePoints);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + targetMessage);
                                break;
                            default:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_POINTS_COMMAND);
                                break;
                        }
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_POINTS_COMMAND);
                }
            }
        }
    }
}
