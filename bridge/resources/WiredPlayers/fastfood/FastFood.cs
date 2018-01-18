using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.house;
using WiredPlayers.model;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.fastfood
{
    public class FastFood : Script
    {
        private static Dictionary<int, Timer> fastFoodTimerList = new Dictionary<int, Timer>();

        public FastFood()
        {
            Event.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Event.OnPlayerExitVehicle += OnPlayerExitVehicle;
            Event.OnClientEventTrigger += OnClientEventTrigger;
            Event.OnEntityEnterCheckpoint += OnEntityEnterCheckpoint;
            Event.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle, sbyte seat)
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
                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                    if (fastFoodTimerList.TryGetValue(playerId, out Timer fastFoodTimer) == true)
                    {
                        fastFoodTimer.Dispose();
                        fastFoodTimerList.Remove(playerId);
                    }
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == false)
                    {
                        HouseModel orderHouse = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ORDER_DESTINATION);
                        Checkpoint playerFastFoodCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, orderHouse.position, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT, playerFastFoodCheckpoint);
                        NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDestinationCheckPoint", orderHouse.position);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_VEHICLE, vehicle);
                    }
                }
            }
        }

        private void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.JOB_FASTFOOD + Constants.MAX_FACTION_VEHICLES && NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == true)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == vehicle)
                {
                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                    String warn = String.Format(Messages.INF_JOB_VEHICLE_LEFT, 60);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + warn);
                    
                    // Creamos el timer para volver a subirse
                    Timer fastFoodTimer = new Timer(OnFastFoodTimer, player, 60000, Timeout.Infinite);
                    fastFoodTimerList.Add(playerId, fastFoodTimer);
                }
            }
        }

        private void OnClientEventTrigger(Client player, String eventName, params object[] arguments)
        {
            if (eventName == "takeFastFoodOrder")
            {
                int orderId = Int32.Parse((String)arguments[0]);
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
                            order.taken = true;
                            int start = Globals.GetTotalSeconds();
                            HouseModel house = GetPlayerFastFoodDeliveryDestination();
                            int time = (int)Math.Round(player.Position.DistanceTo(house.position) / 9.5f);
                            String orderMessage = String.Format(Messages.INF_DELIVER_ORDER, time);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + orderMessage);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DELIVER_ORDER, orderId);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ORDER_DESTINATION, house);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DELIVER_START, start);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DELIVER_TIME, time);
                            //NAPI.SetWorldSharedData(EntityData.FASTFOOD_LIST, NAPI.Util.ToJson(Globals.fastFoodOrderList));
                        }
                        return;
                    }
                }
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ORDER_TIMEOUT);
            }
        }

        private void OnEntityEnterCheckpoint(Checkpoint checkpoint, NetHandle entity)
        {
            if (NAPI.Entity.GetEntityType(entity) == EntityType.Player && NAPI.Data.GetEntityData(entity, EntityData.PLAYER_JOB) == Constants.JOB_FASTFOOD)
            {
                Client player = NAPI.Player.GetPlayerFromHandle(entity);
                Checkpoint playerDeliverColShape = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);

                if (playerDeliverColShape == checkpoint)
                {
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DELIVER_START) == true)
                    {
                        if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_NONE)
                        {
                            NetHandle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                            Vector3 vehiclePosition = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION);
                            NAPI.Entity.SetEntityPosition(playerDeliverColShape, vehiclePosition);
                            int elapsed = Globals.GetTotalSeconds() - NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_START);
                            int extra = (int)Math.Round((NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_TIME) - elapsed) / 2.0f);
                            int amount = GetFastFoodOrderAmount(player) + extra;
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DELIVER_START);
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ORDER_DESTINATION);
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
                        NetHandle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                        if (NAPI.Player.GetPlayerVehicle(player) == vehicle && NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
                        {
                            int won = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_WON);
                            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                            int orderId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
                            String message = String.Format(Messages.INF_JOB_WON, won);
                            Globals.fastFoodOrderList.RemoveAll(order => order.id == orderId);
                            NAPI.Entity.DeleteEntity(playerDeliverColShape);
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money + won);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                            NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDeliverFinished");
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_WON);
                            NAPI.Player.WarpPlayerOutOfVehicle(player);

                            // Devolvemos la moto a su sitio
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

        private void OnPlayerDisconnected(Client player, byte type, string reason)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                // Miramos si tiene el timer activo
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);

                if (fastFoodTimerList.TryGetValue(playerId, out Timer fastFoodTimer) == true)
                {
                    // Eliminamos el timer
                    fastFoodTimer.Dispose();
                    fastFoodTimerList.Remove(playerId);
                }
            }
        }

        private HouseModel GetPlayerFastFoodDeliveryDestination()
        {
            HouseModel house = null;
            Random random = new Random();
            int current = 0;
            int element = random.Next(House.houseList.Count);
            foreach (HouseModel houseModel in House.houseList)
            {
                if (current == element)
                {
                    house = houseModel;
                    break;
                }
                current++;
            }
            return house;
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

        private void RespawnFastfoodVehicle(NetHandle vehicle)
        {
            NAPI.Vehicle.RepairVehicle(vehicle);
            NAPI.Entity.SetEntityPosition(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION));
            NAPI.Entity.SetEntityRotation(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ROTATION));
        }

        private void OnFastFoodTimer(object playerObject)
        {
            try
            {
                Client player = (Client)playerObject;
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                NetHandle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);

                // Respawneamos el vehículo
                RespawnFastfoodVehicle(vehicle);

                // Cancelamos el pedido
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_WON);

                // Quitamos los checkpoints
                NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDeliverFinished");

                // Borramos el timer de la lista
                Timer fastFoodTimer = fastFoodTimerList[playerId];
                if (fastFoodTimer != null)
                {
                    fastFoodTimer.Dispose();
                    fastFoodTimerList.Remove(playerId);
                }

                // Avisamos al jugador
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_JOB_VEHICLE_ABANDONED);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnFastFoodTimer] " + ex.Message);
            }
        }

        [Command("pedidos")]
        public void PedidosCommand(Client player)
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
                NAPI.ClientEvent.TriggerClientEvent(player, "mostrarRepartosComidaRapida");
            }
        }
    }
}
