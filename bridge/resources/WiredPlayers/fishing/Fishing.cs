using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.faction;
using WiredPlayers.database;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.fishing
{
    public class Fishing : Script
    {
        private static Dictionary<int, Timer> fishingTimerList = new Dictionary<int, Timer>();

        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (fishingTimerList.TryGetValue(player.Value, out Timer fishingTimer) == true)
            {
                // Remove the timer
                fishingTimer.Dispose();
                fishingTimerList.Remove(player.Value);
            }
        }

        private void OnFishingPrewarnTimer(object playerObject)
        {
            Client player = (Client)playerObject;
            
            if (fishingTimerList.TryGetValue(player.Value, out Timer fishingTimer) == true)
            {
                // Remove the timer
                fishingTimer.Dispose();
                fishingTimerList.Remove(player.Value);
            }

            // Start the minigame
            NAPI.ClientEvent.TriggerClientEvent(player, "fishingBaitTaken");

            // Send the message and play fishing animation
            NAPI.Player.PlayPlayerAnimation(player, (int)Constants.AnimationFlags.Loop, "amb@world_human_stand_fishing@idle_a", "idle_c");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_SOMETHING_BAITED);
        }

        private int GetPlayerFishingLevel(Client player)
        {
            // Get player points
            int fishingPoints = Job.GetJobPoints(player, Constants.JOB_FISHERMAN);

            // Calculamos el nivel
            if (fishingPoints > 600) return 5;
            if (fishingPoints > 300) return 4;
            if (fishingPoints > 150) return 3;
            if (fishingPoints > 50) return 2;

            return 1;
        }

        [RemoteEvent("startFishingTimer")]
        public void StartFishingTimerEvent(Client player)
        {
            Random random = new Random();

            // Timer for the game to start
            Timer fishingTimer = new Timer(OnFishingPrewarnTimer, player, random.Next(1250, 2500), Timeout.Infinite);
            fishingTimerList.Add(player.Value, fishingTimer);

            // Confirmation message
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_FISHING_ROD_THROWN);
        }

        [RemoteEvent("fishingCanceled")]
        public void FishingCanceledEvent(Client player)
        {
            if (fishingTimerList.TryGetValue(player.Value, out Timer fishingTimer) == true)
            {
                fishingTimer.Dispose();
                fishingTimerList.Remove(player.Value);
            }

            // Cancel the fishing
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Player.FreezePlayer(player, false);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_FISHING);
            
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FISHING_CANCELED);
        }

        [RemoteEvent("fishingSuccess")]
        public void FishingSuccessEvent(Client player)
        {
            // Calculate failure chance
            bool failed = false;
            Random random = new Random();
            int successChance = random.Next(100);

            // Getting player's level
            int fishingLevel = GetPlayerFishingLevel(player);

            switch (fishingLevel)
            {
                case 1:
                    failed = successChance >= 70;
                    break;
                case 2:
                    failed = successChance >= 80;
                    break;
                default:
                    failed = successChance >= 90;
                    fishingLevel = 3;
                    break;
            }

            if (!failed)
            {
                // Get player earnings
                int fishWeight = random.Next(fishingLevel * 100, fishingLevel * 750);
                int playerDatabaseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                ItemModel fishItem = Globals.GetPlayerItemModelFromHash(playerDatabaseId, Constants.ITEM_HASH_FISH);

                if (fishItem == null)
                {
                    fishItem = new ItemModel();
                    fishItem.amount = fishWeight;
                    fishItem.hash = Constants.ITEM_HASH_FISH;
                    fishItem.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                    fishItem.ownerIdentifier = playerDatabaseId;
                    fishItem.position = new Vector3(0.0f, 0.0f, 0.0f);
                    fishItem.dimension = 0;

                    // Add the fish item to database
                    fishItem.id = Database.AddNewItem(fishItem);
                    Globals.itemList.Add(fishItem);
                }
                else
                {
                    fishItem.amount += fishWeight;
                    Database.UpdateItem(fishItem);
                }

                // Send the message to the player
                String message = String.Format(Messages.INF_FISHED_WEIGHT, fishWeight);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_GARBAGE_FISHED);
            }

            // Add one skill point to the player
            int fishingPoints = Job.GetJobPoints(player, Constants.JOB_FISHERMAN);
            Job.SetJobPoints(player, Constants.JOB_FISHERMAN, fishingPoints + 1);

            // Cancel fishing
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Player.FreezePlayer(player, false);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_FISHING);
        }

        [RemoteEvent("fishingFailed")]
        public void FishingFailedEvent(Client player)
        {
            // Cancel fishing
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Player.FreezePlayer(player, false);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_FISHING);
            
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FISHING_FAILED);
        }

        [Command(Commands.COMMAND_FISH)]
        public void FishCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_FISHING) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_FISHING);
            }
            else if (NAPI.Player.GetPlayerVehicleSeat(player) == (int)VehicleSeat.Driver)
            {
                NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                VehicleHash vehicleModel = (VehicleHash)NAPI.Entity.GetEntityModel(vehicle);
                if (vehicleModel == VehicleHash.Marquis || vehicleModel == VehicleHash.Tug)
                {
                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_FISHERMAN)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FISHERMAN);
                    }
                    else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_FISHABLE) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_FISHING_ZONE);
                    }
                    else
                    {

                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_FISHING_BOAT);
                }
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                int fishingRodId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                ItemModel fishingRod = Globals.GetItemModelFromId(fishingRodId);

                if (fishingRod != null && fishingRod.hash == Constants.ITEM_HASH_FISHING_ROD)
                {
                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    ItemModel bait = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BAIT);
                    if (bait != null && bait.amount > 0)
                    {
                        foreach (Vector3 fishingPosition in Constants.FISHING_POSITION_LIST)
                        {
                            // Set player looking to the sea
                            NAPI.Entity.SetEntityRotation(player, new Vector3(0.0f, 0.0f, 142.532f));
                            NAPI.Player.FreezePlayer(player, true);
                            
                            if (bait.amount == 1)
                            {
                                Globals.itemList.Remove(bait);
                                Database.RemoveItem(bait.id);
                            }
                            else
                            {
                                bait.amount--;
                                Database.UpdateItem(bait);
                            }

                            // Start fishing minigame
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_FISHING, true);
                            NAPI.Player.PlayPlayerAnimation(player, (int)Constants.AnimationFlags.Loop, "amb@world_human_stand_fishing@base", "base");
                            NAPI.ClientEvent.TriggerClientEvent(player, "startPlayerFishing");
                            return;
                        }

                        // Player's not in the fishing zone
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_FISHING_ZONE);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_BAITS);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_FISHING_ROD);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ROD_BOAT);
            }
        }
    }
}
