using GTANetworkAPI;
using WiredPlayers.business;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.vehicles;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WiredPlayers.mechanic
{
    class Mechanic : Script
    {
        public static List<TunningModel> tunningList;

        public static void AddTunningToVehicle(Vehicle vehicle)
        {
            foreach (TunningModel tunning in tunningList)
            {
                if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID) == tunning.vehicle)
                {
                    NAPI.Vehicle.SetVehicleMod(vehicle, tunning.slot, tunning.component);
                }
            }
        }

        public static bool PlayerInValidRepairPlace(Client player)
        {
            // Check if the player is in any workshop
            foreach (BusinessModel business in Business.businessList)
            {
                if (business.type == Constants.BUSINESS_TYPE_MECHANIC && player.Position.DistanceTo(business.position) < 25.0f)
                {
                    return true;
                }
            }

            // Check if the player has a towtruck near
            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                VehicleHash vehicleHash = (VehicleHash)NAPI.Entity.GetEntityModel(vehicle);
                if (vehicleHash == VehicleHash.TowTruck || vehicleHash == VehicleHash.TowTruck2)
                {
                    return true;
                }
            }

            return false;
        }

        private TunningModel GetVehicleTunningComponent(int vehicleId, int slot, int component)
        {
            TunningModel tunning = null;
            foreach (TunningModel tunningModel in tunningList)
            {
                if (tunningModel.vehicle == vehicleId && tunningModel.slot == slot && tunningModel.component == component)
                {
                    tunning = tunningModel;
                    break;
                }
            }
            return tunning;
        }

        [RemoteEvent("repaintVehicle")]
        public void RepaintVehicleEvent(Client player, int colorType, String firstColor, String secondColor, int pearlescentColor, int vehiclePaid)
        {
            // Get player's vehicle
            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE);
            
            switch (colorType)
            {
                case 0:
                    // Predefined color
                    NAPI.Vehicle.SetVehiclePrimaryColor(vehicle, Int32.Parse(firstColor));
                    NAPI.Vehicle.SetVehicleSecondaryColor(vehicle, Int32.Parse(secondColor));
                    
                    if (pearlescentColor >= 0)
                    {
                        NAPI.Vehicle.SetVehiclePearlescentColor(vehicle, pearlescentColor);
                    }
                    break;
                case 1:
                    // Custom color
                    String[] firstColorArray = firstColor.Split(',');
                    String[] secondColorArray = secondColor.Split(',');
                    NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, Int32.Parse(firstColorArray[0]), Int32.Parse(firstColorArray[1]), Int32.Parse(firstColorArray[2]));
                    NAPI.Vehicle.SetVehicleCustomSecondaryColor(vehicle, Int32.Parse(secondColorArray[0]), Int32.Parse(secondColorArray[1]), Int32.Parse(secondColorArray[2]));
                    break;
            }
            
            if (vehiclePaid > 0)
            {
                // Check for the product amount
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                ItemModel item = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                if (item != null && item.amount >= 250)
                {
                    int vehicleFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);

                    // Search for a player with vehicle keys
                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (Vehicles.HasPlayerVehicleKeys(target, vehicle) || (vehicleFaction > 0 && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == vehicleFaction))
                        {
                            if (target.Position.DistanceTo(player.Position) < 4.0f)
                            {
                                // Vehicle repaint data
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_VEHICLE, vehicle);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_COLOR_TYPE, colorType);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_FIRST_COLOR, firstColor);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_SECOND_COLOR, secondColor);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_PEARLESCENT, pearlescentColor);
                                NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, vehiclePaid);
                                NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRODUCTS, 250);
                                
                                String playerMessage = String.Format(Messages.INF_MECHANIC_REPAINT_OFFER, target.Name, vehiclePaid);
                                String targetMessage = String.Format(Messages.INF_MECHANIC_REPAINT_ACCEPT, player.Name, vehiclePaid);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                return;
                            }
                        }
                    }

                    // There's no player with vehicle's keys near
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                }
                else
                {
                    String message = String.Format(Messages.ERR_NOT_REQUIRED_PRODUCTS, 250);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                }
            }
        }

        [RemoteEvent("cancelVehicleRepaint")]
        public void CancelVehicleRepaintEvent(Client player)
        {
            // Get player's vehicle
            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE);

            // Get previous colors
            int vehicleColorType = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE);
            String primaryVehicleColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR);
            String secondaryVehicleColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR);
            int vehiclePearlescentColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR);

            if (vehicleColorType == Constants.VEHICLE_COLOR_TYPE_PREDEFINED)
            {
                NAPI.Vehicle.SetVehiclePrimaryColor(vehicle, Int32.Parse(primaryVehicleColor));
                NAPI.Vehicle.SetVehicleSecondaryColor(vehicle, Int32.Parse(secondaryVehicleColor));
                NAPI.Vehicle.SetVehiclePearlescentColor(vehicle, vehiclePearlescentColor);
            }
            else
            {
                String[] primaryColor = primaryVehicleColor.Split(',');
                String[] secondaryColor = secondaryVehicleColor.Split(',');
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, Int32.Parse(primaryColor[0]), Int32.Parse(primaryColor[1]), Int32.Parse(primaryColor[2]));
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(vehicle, Int32.Parse(secondaryColor[0]), Int32.Parse(secondaryColor[1]), Int32.Parse(secondaryColor[2]));
            }
        }

        [RemoteEvent("calculateTunningCost")]
        public void CalculateTunningCostEvent(Client player)
        {
            int totalProducts = 0;

            // Get the vehicle
            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
            int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);

            for (int i = 0; i < 49; i++)
            {
                int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);

                if (vehicleMod > 0)
                {
                    TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);
                    if (tunningModel == null)
                    {
                        totalProducts += Constants.TUNNING_PRICE_LIST.Where(x => x.slot == i).First().products;
                    }
                }
            }

            // Send the price to the player
            String priceMessage = String.Format(Messages.INF_TUNNING_PRODUCTS, totalProducts);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + priceMessage);
        }

        [RemoteEvent("modifyVehicle")]
        public void ModifyVehicleEvent(Client player, int slot, int component)
        {
            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);

            if (component > 0)
            {
                NAPI.Vehicle.SetVehicleMod(vehicle, slot, component);
            }
            else
            {
                NAPI.Vehicle.RemoveVehicleMod(vehicle, slot);
            }
        }

        [RemoteEvent("cancelVehicleModification")]
        public void CancelVehicleModificationEvent(Client player)
        {
            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
            int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
            
            for (int i = 0; i < 49; i++)
            {
                int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);
                TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);

                if (tunningModel == null)
                {
                    NAPI.Vehicle.RemoveVehicleMod(vehicle, i);
                }
                else
                {
                    NAPI.Vehicle.SetVehicleMod(vehicle, i, vehicleMod);
                }
            }
        }

        [RemoteEvent("confirmVehicleModification")]
        public void ConfirmVehicleModificationEvent(Client player)
        {
            int totalProducts = 0;
            Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
            int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);

            // Get player's product amount
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            ItemModel item = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);
            
            for (int i = 0; i < 49; i++)
            {
                int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);

                if (vehicleMod > 0)
                {
                    TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);
                    if (tunningModel == null)
                    {
                        totalProducts += Constants.TUNNING_PRICE_LIST.Where(x => x.slot == i).First().products;
                    }
                }
            }

            if (item != null && item.amount >= totalProducts)
            {
                for (int i = 0; i < 49; i++)
                {
                    int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);

                    if (vehicleMod > 0)
                    {
                        TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);
                        if (tunningModel == null)
                        {
                            // Add component to database
                            tunningModel = new TunningModel();
                            tunningModel.slot = i;
                            tunningModel.component = vehicleMod;
                            tunningModel.vehicle = vehicleId;
                            tunningModel.id = Database.AddTunning(tunningModel);
                            tunningList.Add(tunningModel);
                        }
                    }
                }

                // Remove consumed products
                item.amount -= totalProducts;
                Database.UpdateItem(item);

                // Close tunning menu
                NAPI.ClientEvent.TriggerClientEvent(player, "closeTunningMenu");
                
                // Confirmation message
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_TUNNING);
            }
            else
            {
                String message = String.Format(Messages.ERR_NOT_REQUIRED_PRODUCTS, totalProducts);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
            }
        }

        [Command(Messages.COM_REPAIR, Messages.GEN_MECHANIC_REPAIR_COMMAND)]
        public void RepairCommand(Client player, int vehicleId, String type, int price = 0)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_MECHANIC)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_MECHANIC);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (PlayerInValidRepairPlace(player) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VALID_REPAIR_PLACE);
            }
            else
            {
                Vehicle vehicle = Vehicles.GetVehicleById(vehicleId);
                if (vehicle == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                }
                else if (NAPI.Entity.GetEntityPosition(vehicle).DistanceTo(player.Position) > 5.0f)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_WANTED_VEHICLE_FAR);
                }
                else
                {
                    int spentProducts = 0;

                    switch (type.ToLower())
                    {
                        case Messages.ARG_CHASSIS:
                            spentProducts = Constants.PRICE_VEHICLE_CHASSIS;
                            break;
                        case Messages.ARG_DOORS:
                            for (int i = 0; i < 6; i++)
                            {
                                if (NAPI.Vehicle.IsVehicleDoorBroken(vehicle, i) == true)
                                {
                                    spentProducts += Constants.PRICE_VEHICLE_DOORS;
                                }
                            }
                            break;
                        case Messages.ARG_TYRES:
                            for (int i = 0; i < 4; i++)
                            {
                                if (NAPI.Vehicle.IsVehicleTyrePopped(vehicle, i) == true)
                                {
                                    spentProducts += Constants.PRICE_VEHICLE_TYRES;
                                }
                            }
                            break;
                        case Messages.ARG_WINDOWS:
                            for (int i = 0; i < 4; i++)
                            {
                                if (NAPI.Vehicle.IsVehicleWindowBroken(vehicle, i) == true)
                                {
                                    spentProducts += Constants.PRICE_VEHICLE_WINDOWS;
                                }
                            }
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_MECHANIC_REPAIR_COMMAND);
                            return;
                    }

                    if (price > 0)
                    {
                        // Get player's products
                        int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        ItemModel item = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                        if (item != null && item.amount >= spentProducts)
                        {
                            int vehicleFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);

                            // Check a player with vehicle keys
                            foreach (Client target in NAPI.Pools.GetAllPlayers())
                            {
                                if (Vehicles.HasPlayerVehicleKeys(target, vehicle) || (vehicleFaction > 0 && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == vehicleFaction))
                                {
                                    if (target.Position.DistanceTo(player.Position) < 4.0f)
                                    {
                                        // Fill repair entity data
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAIR_VEHICLE, vehicle);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAIR_TYPE, type);
                                        NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRODUCTS, spentProducts);
                                        NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, price);
                                        
                                        String playerMessage = String.Format(Messages.INF_MECHANIC_REPAIR_OFFER, target.Name, price);
                                        String targetMessage = String.Format(Messages.INF_MECHANIC_REPAIR_ACCEPT, player.Name, price);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        return;
                                    }
                                }
                            }

                            // There's no player with the vehicle's keys near
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                        }
                        else
                        {
                            String message = String.Format(Messages.ERR_NOT_REQUIRED_PRODUCTS, spentProducts);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                        }
                    }
                    else
                    {
                        String message = String.Format(Messages.INF_REPAIR_PRICE, spentProducts);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    }
                }
            }
        }

        [Command(Messages.COM_REPAINT, Messages.GEN_MECHANIC_REPAINT_COMMAND)]
        public void RepaintCommand(Client player, int vehicleId)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_MECHANIC)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_MECHANIC);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                foreach (BusinessModel business in Business.businessList)
                {
                    if (business.type == Constants.BUSINESS_TYPE_MECHANIC && player.Position.DistanceTo(business.position) < 25.0f)
                    {
                        Vehicle vehicle = Vehicles.GetVehicleById(vehicleId);
                        if (vehicle != null)
                        {
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE, vehicle);
                            NAPI.ClientEvent.TriggerClientEvent(player, "showRepaintMenu");
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                        }
                        return;
                    }
                }

                // Player is not in any workshop
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_MECHANIC_WORKSHOP);
            }
        }

        [Command(Messages.COM_TUNNING)]
        public void TunningCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_MECHANIC)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_MECHANIC);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                foreach (BusinessModel business in Business.businessList)
                {
                    if (business.type == Constants.BUSINESS_TYPE_MECHANIC && player.Position.DistanceTo(business.position) < 25.0f)
                    {
                        NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                        if (vehicle != null)
                        {
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE, vehicle);
                            NAPI.ClientEvent.TriggerClientEvent(player, "showTunningMenu");
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
                        }
                        return;
                    }
                }

                // Player is not in any workshop
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_MECHANIC_WORKSHOP);
            }
        }
    }
}
