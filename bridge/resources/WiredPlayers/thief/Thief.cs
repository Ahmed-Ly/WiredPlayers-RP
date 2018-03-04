using GTANetworkAPI;
using WiredPlayers.business;
using WiredPlayers.database;
using WiredPlayers.faction;
using WiredPlayers.globals;
using WiredPlayers.house;
using WiredPlayers.model;
using WiredPlayers.vehicles;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.thief
{
    public class Thief : Script
    {
        private static Dictionary<int, Timer> robberyTimerList = new Dictionary<int, Timer>();

        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (robberyTimerList.TryGetValue(player.Value, out Timer robberyTimer) == true)
            {
                robberyTimer.Dispose();
                robberyTimerList.Remove(player.Value);
            }
        }

        private void OnLockpickTimer(object playerObject)
        {
            Client player = (Client)playerObject;
            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LOCKPICKING);

            NAPI.Vehicle.SetVehicleLocked(vehicle, false);
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_LOCKPICKING);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ANIMATION);
            
            if (robberyTimerList.TryGetValue(player.Value, out Timer robberyTimer) == true)
            {
                robberyTimer.Dispose();
                robberyTimerList.Remove(player.Value);
            }
            
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_LOCKPICKED);
        }

        private void OnHotwireTimer(object playerObject)
        {
            Client player = (Client)playerObject;
            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOTWIRING);

            NAPI.Vehicle.SetVehicleEngineStatus(vehicle, true);
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_HOTWIRING);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ANIMATION);
            
            if (robberyTimerList.TryGetValue(player.Value, out Timer robberyTimer) == true)
            {
                robberyTimer.Dispose();
                robberyTimerList.Remove(player.Value);
            }

            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                {
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_POLICE_WARNING);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_EMERGENCY_WITH_WARN, player.Position);
                }
            }
            
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_VEH_HOTWIREED);
        }

        private void OnPlayerRob(object playerObject)
        {
            Client player = (Client)playerObject;
            int playerSqlId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            int timeElapsed = Globals.GetTotalSeconds() - NAPI.Data.GetEntityData(player, EntityData.PLAYER_ROBBERY_START);
            decimal stolenItemsDecimal = timeElapsed / Constants.ITEMS_ROBBED_PER_TIME;
            int totalStolenItems = (int)Math.Round(stolenItemsDecimal);

            // Check if the player has stolen items
            ItemModel stolenItemModel = Globals.GetPlayerItemModelFromHash(playerSqlId, Constants.ITEM_HASH_STOLEN_OBJECTS);

            if (stolenItemModel == null)
            {
                stolenItemModel = new ItemModel();

                stolenItemModel.amount = totalStolenItems;
                stolenItemModel.hash = Constants.ITEM_HASH_STOLEN_OBJECTS;
                stolenItemModel.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                stolenItemModel.ownerIdentifier = playerSqlId;
                stolenItemModel.dimension = 0;
                stolenItemModel.position = new Vector3(0.0f, 0.0f, 0.0f);
                stolenItemModel.id = Database.AddNewItem(stolenItemModel);
                Globals.itemList.Add(stolenItemModel);
            }
            else
            {
                stolenItemModel.amount += totalStolenItems;
                Database.UpdateItem(stolenItemModel);
            }

            // Allow player movement
            NAPI.Player.FreezePlayer(player, false);
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ANIMATION);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ROBBERY_START);
            
            if (robberyTimerList.TryGetValue(player.Value, out Timer robberyTimer) == true)
            {
                robberyTimer.Dispose();
                robberyTimerList.Remove(player.Value);
            }

            // Avisamos de los objetos robados
            String message = String.Format(Messages.INF_PLAYER_ROBBED, totalStolenItems);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);

            // Check if the player commited the maximum thefts allowed
            int totalThefts = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_DELIVER);
            if (Constants.MAX_THEFTS_IN_ROW == totalThefts)
            {
                // Apply a cooldown to the player
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_DELIVER, 0);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN, 60);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_ROB_PRESSURE);
            }
            else
            {
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_DELIVER, totalThefts + 1);
            }
        }

        private void GeneratePoliceRobberyWarning(Client player)
        {
            Vector3 robberyPosition = null;
            String robberyPlace = String.Empty;
            String robberyHour = DateTime.Now.ToString("h:mm:ss tt");

            // Check if he robbed into a house or business
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) > 0)
            {
                int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                HouseModel house = House.GetHouseById(houseId);
                robberyPosition = house.position;
                robberyPlace = house.name;
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) > 0)
            {
                int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                BusinessModel business = Business.GetBusinessById(businessId);
                robberyPosition = business.position;
                robberyPlace = business.name;
            }
            else
            {
                robberyPosition = NAPI.Entity.GetEntityPosition(player);
            }

            // Create the police report
            FactionWarningModel factionWarning = new FactionWarningModel(Constants.FACTION_POLICE, player.Value, robberyPlace, robberyPosition, -1, robberyHour);
            Faction.factionWarningList.Add(factionWarning);
            
            String warnMessage = String.Format(Messages.INF_EMERGENCY_WARNING, Faction.factionWarningList.Count - 1);
            
            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) == 1)
                {
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + warnMessage);
                }
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            foreach (Vector3 pawnShop in Constants.PAWN_SHOP)
            {
                // Create pawn shops
                NAPI.TextLabel.CreateTextLabel(Messages.GEN_PAWN_SHOP, pawnShop, 10.0f, 0.5f, 4, new Color(255, 255, 255), false, 0);
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) == Constants.JOB_THIEF)
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_HOTWIRING) == true)
                {
                    // Remove player's hotwire
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_HOTWIRING);
                    NAPI.Player.StopPlayerAnimation(player);
                    
                    if (robberyTimerList.TryGetValue(player.Value, out Timer robberyTimer) == true)
                    {
                        robberyTimer.Dispose();
                        robberyTimerList.Remove(player.Value);
                    }
                    
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_STOPPED_HOTWIRE);
                }
                else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ROBBERY_START) == true)
                {
                    OnPlayerRob(player);
                }
            }
        }

        [Command(Messages.COM_FORCE)]
        public void ForceCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_THIEF)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_THIEF);
                }
                else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_LOCKPICKING) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_LOCKPICKING);
                }
                else
                {
                    Vehicle vehicle = Globals.GetClosestVehicle(player);
                    if (vehicle == null)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                    }
                    else if (Vehicles.HasPlayerVehicleKeys(player, vehicle) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_LOCKPICK_OWN_VEHICLE);
                    }
                    else if (NAPI.Vehicle.GetVehicleLocked(vehicle) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEH_ALREADY_UNLOCKED);
                    }
                    else
                    {
                        // Generate police report
                        GeneratePoliceRobberyWarning(player);

                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_LOCKPICKING, vehicle);
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "missheistfbisetup1", "hassle_intro_loop_f");
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, true);

                        // Timer to finish forcing the door
                        Timer robberyTimer = new Timer(OnLockpickTimer, player, 10000, Timeout.Infinite);
                        robberyTimerList.Add(player.Value, robberyTimer);

                    }
                }
            }
        }

        [Command(Messages.COM_STEAL)]
        public void StealCommand(Client player)
        {
            if (player.Position.DistanceTo(new Vector3(-286.7586f, -849.3693f, 31.74337f)) > 1150.0f)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_THIEF_AREA);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_THIEF)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_THIEF);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ROBBERY_START) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_STEALING);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN) > 0)
            {
                int timeLeft = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN) - Globals.GetTotalSeconds();
                String message = String.Format(Messages.ERR_PLAYER_COOLDOWN_THIEF, timeLeft);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
            }
            else
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) > 0 || NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) > 0)
                {
                    int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                    HouseModel house = House.GetHouseById(houseId);
                    if (house != null && House.HasPlayerHouseKeys(player, house) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_ROB_OWN_HOUSE);
                    }
                    else
                    {
                        // Generate the police report
                        GeneratePoliceRobberyWarning(player);

                        // Start stealing items
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "misscarstealfinalecar_5_ig_3", "crouchloop");
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ROBBERY_START, Globals.GetTotalSeconds());
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_SEARCHING_VALUE_ITEMS);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, true);
                        NAPI.Player.FreezePlayer(player, true);

                        // Timer to finish the robbery
                        Timer robberyTimer = new Timer(OnPlayerRob, player, 20000, Timeout.Infinite);
                        robberyTimerList.Add(player.Value, robberyTimer);
                    }
                }
                else if (NAPI.Player.GetPlayerVehicleSeat(player) == (int)VehicleSeat.Driver)
                {
                    Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
                    if (Vehicles.HasPlayerVehicleKeys(player, vehicle) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_ROB_OWN_VEHICLE);
                    }
                    else if (NAPI.Vehicle.GetVehicleEngineStatus(vehicle) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ENGINE_ON);
                    }
                    else
                    {
                        // Generate the police report
                        GeneratePoliceRobberyWarning(player);

                        // Start stealing items
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "veh@plane@cuban@front@ds@base", "hotwire");
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ROBBERY_START, Globals.GetTotalSeconds());
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_SEARCHING_VALUE_ITEMS);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, true);

                        // Timer to finish the robbery
                        Timer robberyTimer = new Timer(OnPlayerRob, player, 35000, Timeout.Infinite);
                        robberyTimerList.Add(player.Value, robberyTimer);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_ROB);
                }
            }
        }

        [Command(Messages.COM_HOTWIRE)]
        public void HotwireCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_THIEF)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_THIEF);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_HOTWIRING) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_HOTWIRING);
            }
            else if (NAPI.Player.IsPlayerInAnyVehicle(player) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
            }
            else
            {
                Vehicle vehicle = NAPI.Player.GetPlayerVehicle(player);
                if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.Driver)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
                }
                else if (Vehicles.HasPlayerVehicleKeys(player, vehicle) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_HOTWIRE_OWN_VEHICLE);
                }
                else if (NAPI.Vehicle.GetVehicleEngineStatus(vehicle) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ENGINE_ALREADY_STARTED);
                }
                else
                {
                    int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                    Vector3 position = NAPI.Entity.GetEntityPosition(vehicle);

                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOTWIRING, vehicle);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, true);
                    NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "veh@plane@cuban@front@ds@base", "hotwire");

                    // Create timer to finish the hotwire
                    Timer robberyTimer = new Timer(OnHotwireTimer, player, 15000, Timeout.Infinite);
                    robberyTimerList.Add(player.Value, robberyTimer);
                    
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_HOTWIRE_STARTED);

                    // Add hotwire log to the database
                    Database.LogHotwire(player.Name, vehicleId, position);
                }
            }
        }

        [Command(Messages.COM_PAWN)]
        public void PawnCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_THIEF)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_THIEF);
            }
            else
            {
                foreach (Vector3 pawnShop in Constants.PAWN_SHOP)
                {
                    if (player.Position.DistanceTo(pawnShop) < 1.5f)
                    {
                        int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        ItemModel stolenItems = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_STOLEN_OBJECTS);
                        if (stolenItems != null)
                        {
                            // Calculate the earnings
                            int wonAmount = stolenItems.amount * Constants.PRICE_STOLEN;
                            String message = String.Format(Messages.INF_PLAYER_PAWNED_ITEMS, wonAmount);
                            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY) + wonAmount;

                            // Delete stolen items
                            Database.RemoveItem(stolenItems.id);
                            Globals.itemList.Remove(stolenItems);

                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_STOLEN_ITEMS);
                        }
                        return;
                    }
                }
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_PAWN_SHOW);
            }
        }
    }
}
