using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

namespace WiredPlayers.fastfood
{
    public class FastFood : Script
    {
        private static Dictionary<int, Timer> fastFoodTimerList = new Dictionary<int, Timer>();

        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (fastFoodTimerList.TryGetValue(player.Value, out Timer fastFoodTimer) == true)
            {
                // Destroy the timer
                fastFoodTimer.Dispose();
                fastFoodTimerList.Remove(player.Value);
            }
        }

        private int GetFastFoodOrderAmount(Client player)
        {
            int amount = 0;
            int orderId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
            foreach (FastFoodOrderModel order in Globals.fastFoodOrderList)
            {
                if (order.id == orderId)
                {
                    amount += order.pizzas * Constants.PRICE_PIZZA;
                    amount += order.hamburgers * Constants.PRICE_HAMBURGER;
                    amount += order.sandwitches * Constants.PRICE_SANDWICH;
                    break;
                }
            }
            return amount;
        }

        private FastFoodOrderModel GetFastfoodOrderFromId(int orderId)
        {
            FastFoodOrderModel order = null;

            foreach (FastFoodOrderModel orderModel in Globals.fastFoodOrderList)
            {
                if (orderModel.id == orderId)
                {
                    order = orderModel;
                    break;
                }
            }

            return order;
        }

        private void RespawnFastfoodVehicle(Vehicle vehicle)
        {
            NAPI.Vehicle.RepairVehicle(vehicle);
            NAPI.Entity.SetEntityPosition(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION));
            NAPI.Entity.SetEntityRotation(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ROTATION));
        }

        private void OnFastFoodTimer(object playerObject)
        {
            Client player = (Client)playerObject;
            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);

            // Vehicle respawn
            RespawnFastfoodVehicle(vehicle);

            // Cancel the order
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_WON);

            // Delete map blip
            NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDeliverFinished");

            // Remove timer from the list
            Timer fastFoodTimer = fastFoodTimerList[player.Value];
            if (fastFoodTimer != null)
            {
                fastFoodTimer.Dispose();
                fastFoodTimerList.Remove(player.Value);
            }

            // Send the message to the player
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_JOB_VEHICLE_ABANDONED);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seat)
        {
            if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.JOB_FASTFOOD + Constants.MAX_FACTION_VEHICLES)
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DELIVER_ORDER) == false && NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == false)
                {
                    NAPI.Player.WarpPlayerOutOfVehicle(player);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_DELIVERING_ORDER);
                }
                else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE) != vehicle)
                {
                    NAPI.Player.WarpPlayerOutOfVehicle(player);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_YOUR_JOB_VEHICLE);
                }
                else
                {
                    if (fastFoodTimerList.TryGetValue(player.Value, out Timer fastFoodTimer) == true)
                    {
                        fastFoodTimer.Dispose();
                        fastFoodTimerList.Remove(player.Value);
                    }
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == false)
                    {
                        int orderId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
                        FastFoodOrderModel order = GetFastfoodOrderFromId(orderId);
                        Checkpoint playerFastFoodCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, order.position, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));

                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT, playerFastFoodCheckpoint);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_VEHICLE, vehicle);

                        NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDestinationCheckPoint", order.position);
                    }
                }
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.JOB_FASTFOOD + Constants.MAX_FACTION_VEHICLES && NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == true)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == vehicle)
                {
                    String warn = String.Format(Messages.INF_JOB_VEHICLE_LEFT, 60);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + warn);

                    // Timer with the time left to get into the vehicle
                    Timer fastFoodTimer = new Timer(OnFastFoodTimer, player, 60000, Timeout.Infinite);
                    fastFoodTimerList.Add(player.Value, fastFoodTimer);
                }
            }
        }

        [ServerEvent(Event.PlayerEnterCheckpoint)]
        public void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) == Constants.JOB_FASTFOOD)
            {
                // Get the player's deliver checkpoint
                Checkpoint playerDeliverColShape = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);

                if (playerDeliverColShape == checkpoint)
                {
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DELIVER_START) == true)
                    {
                        if (NAPI.Player.IsPlayerInAnyVehicle(player) == false)
                        {
                            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                            Vector3 vehiclePosition = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            NAPI.Entity.SetEntityPosition(playerDeliverColShape, vehiclePosition);
                            int elapsed = Globals.GetTotalSeconds() - NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_START);
                            int extra = (int)Math.Round((NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_TIME) - elapsed) / 2.0f);
                            int amount = GetFastFoodOrderAmount(player) + extra;

                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DELIVER_START);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_WON, amount > 0 ? amount : 25);

                            NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDeliverBack", vehiclePosition);

                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_DELIVER_COMPLETED);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_DELIVER_IN_VEHICLE);
                        }
                    }
                    else
                    {
                        Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                        if (NAPI.Player.GetPlayerVehicle(player) == vehicle && NAPI.Player.GetPlayerVehicleSeat(player) == (int)VehicleSeat.Driver)
                        {
                            int won = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_WON);
                            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                            int orderId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
                            String message = String.Format(Messages.INF_JOB_WON, won);
                            Globals.fastFoodOrderList.RemoveAll(order => order.id == orderId);

                            NAPI.Entity.DeleteEntity(playerDeliverColShape);
                            NAPI.Player.WarpPlayerOutOfVehicle(player);

                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money + won);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);

                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_WON);

                            NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDeliverFinished");

                            // We get the motorcycle to its spawn point
                            RespawnFastfoodVehicle(vehicle);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_YOUR_JOB_VEHICLE);
                        }
                    }
                }
            }
        }

        [RemoteEvent("takeFastFoodOrder")]
        public void TakeFastFoodOrderEvent(Client player, int orderId)
        {
            foreach (FastFoodOrderModel order in Globals.fastFoodOrderList)
            {
                if (order.id == orderId)
                {
                    if (order.taken)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ORDER_TAKEN);
                    }
                    else
                    {
                        // Get the time to reach the destination
                        int start = Globals.GetTotalSeconds();
                        int time = (int)Math.Round(player.Position.DistanceTo(order.position) / 9.5f);

                        // We take the order
                        order.taken = true;

                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DELIVER_ORDER, orderId);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DELIVER_START, start);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DELIVER_TIME, time);

                        // Information message sent to the player
                        String orderMessage = String.Format(Messages.INF_DELIVER_ORDER, time);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + orderMessage);
                    }
                    return;
                }
            }

            // Order has been deleted
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ORDER_TIMEOUT);
        }

        [Command(Messages.COM_ORDERS)]
        public void OrdersCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_FASTFOOD)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_FASTFOOD);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DELIVER_ORDER) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ORDER_DELIVERING);
            }
            else
            {
                // Get the deliverable orders
                List<FastFoodOrderModel> fastFoodOrders = Globals.fastFoodOrderList.Where(o => !o.taken).ToList();

                if (fastFoodOrders.Count > 0)
                {

                    List<float> distancesList = new List<float>();

                    foreach (FastFoodOrderModel order in fastFoodOrders)
                    {
                        float distance = player.Position.DistanceTo(order.position);
                        distancesList.Add(distance);
                    }

                    NAPI.ClientEvent.TriggerClientEvent(player, "showFastfoodOrders", NAPI.Util.ToJson(fastFoodOrders), NAPI.Util.ToJson(distancesList));
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ORDER_NONE);
                }
            }
        }
    }
}
