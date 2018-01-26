using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

namespace WiredPlayers.garbage
{
    public class Garbage : Script
    {
        private static Dictionary<int, Timer> garbageTimerList = new Dictionary<int, Timer>();

        public Garbage()
        {
            Event.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Event.OnPlayerExitVehicle += OnPlayerExitVehicle;
            Event.OnPlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
            Event.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seat)
        {
            if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.JOB_GARBAGE + Constants.MAX_FACTION_VEHICLES)
            {
                if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
                {
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_ROUTE) == false && NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == false)
                    {
                        NAPI.Player.WarpPlayerOutOfVehicle(player);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_ROUTE);
                    }
                    else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE) != vehicle)
                    {
                        NAPI.Player.WarpPlayerOutOfVehicle(player);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_YOUR_JOB_VEHICLE);
                    }
                    else
                    {
                        int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                        if (garbageTimerList.TryGetValue(playerId, out Timer garbageTimer) == true)
                        {
                            garbageTimer.Dispose();
                            garbageTimerList.Remove(playerId);
                        }

                        // Miramos si empieza una ruta o vuelve al camión
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == false)
                        {
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_PARTNER, player);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_VEHICLE, vehicle);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_WAITING_PARTNER);
                        }
                        else
                        {
                            // Continuamos la ruta
                            Client partner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                            int garbageRoute = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_ROUTE);
                            int checkPoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);
                            Vector3 garbagePosition = GetGarbageCheckPointPosition(garbageRoute, checkPoint);
                            NAPI.ClientEvent.TriggerClientEvent(player, "showGarbageCheckPoint", garbagePosition);
                            NAPI.ClientEvent.TriggerClientEvent(partner, "showGarbageCheckPoint", garbagePosition);
                        }
                    }
                }
                else
                {
                    foreach (Client driver in NAPI.Vehicle.GetVehicleOccupants(vehicle))
                    {
                        if (NAPI.Data.HasEntityData(driver, EntityData.PLAYER_JOB_PARTNER) && NAPI.Player.GetPlayerVehicleSeat(driver) == Constants.VEHICLE_SEAT_DRIVER)
                        {
                            Client partner = NAPI.Data.GetEntityData(driver, EntityData.PLAYER_JOB_PARTNER);
                            if (partner == driver)
                            {
                                if(NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 1)
                                {
                                    // Unimos los dos jugadores como compañeros
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_PARTNER, driver);
                                    NAPI.Data.SetEntityData(driver, EntityData.PLAYER_JOB_PARTNER, player);

                                    // Asignamos la ruta al copiloto
                                    int garbageRoute = NAPI.Data.GetEntityData(driver, EntityData.PLAYER_JOB_ROUTE);
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_ROUTE, garbageRoute);
                                    NAPI.Data.SetEntityData(driver, EntityData.PLAYER_JOB_CHECKPOINT, 0);
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT, 0);

                                    // Creamos el primer punto
                                    Vector3 currentGarbagePosition = GetGarbageCheckPointPosition(garbageRoute, 0);
                                    Vector3 nextGarbagePosition = GetGarbageCheckPointPosition(garbageRoute, 1);
                                    Checkpoint garbageCheckpoint = NAPI.Checkpoint.CreateCheckpoint(0, currentGarbagePosition, nextGarbagePosition, 2.5f, new Color(198, 40, 40, 200));
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE, garbageCheckpoint);
                                    NAPI.ClientEvent.TriggerClientEvent(driver, "showGarbageCheckPoint", currentGarbagePosition);
                                    NAPI.ClientEvent.TriggerClientEvent(player, "showGarbageCheckPoint", currentGarbagePosition);

                                    // Añadimos el objeto basura
                                    GTANetworkAPI.Object trashBag = NAPI.Object.CreateObject(628215202, currentGarbagePosition, new Vector3(0.0f, 0.0f, 0.0f));
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_GARBAGE_BAG, trashBag);
                                }
                                else
                                {
                                    NAPI.Player.WarpPlayerOutOfVehicle(player);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
                                }
                            }
                            return;
                        }
                    }

                    // Si no hay nadie conduciendo, echamos al jugador
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WAIT_GARBAGE_DRIVER);
                    NAPI.Player.WarpPlayerOutOfVehicle(player);
                }
            }
        }

        private void OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_VEHICLE) && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == Constants.JOB_GARBAGE + Constants.MAX_FACTION_VEHICLES)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == vehicle && NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
                {
                    Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                    String warn = String.Format(Messages.INF_JOB_VEHICLE_LEFT, 45);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + warn);
                    NAPI.ClientEvent.TriggerClientEvent(player, "deleteGarbageCheckPoint");
                    NAPI.ClientEvent.TriggerClientEvent(target, "deleteGarbageCheckPoint");

                    // Creamos el timer para volver a subirse
                    Timer garbageTimer = new Timer(OnGarbageTimer, player, 45000, Timeout.Infinite);
                    garbageTimerList.Add(playerId, garbageTimer);
                }
            }
        }

        private void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_COLSHAPE) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) == Constants.JOB_GARBAGE)
            {
                // Obtenemos el checkpoint de basura
                Checkpoint garbageCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE);

                if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER && garbageCheckpoint == checkpoint)
                {
                    NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE) == vehicle)
                    {
                        // Finalizar la ruta de basurero
                        FinishGarbageRoute(player);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE_JOB);
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

                if (garbageTimerList.TryGetValue(playerId, out Timer garbageTimer) == true)
                {
                    // Eliminamos el timer
                    garbageTimer.Dispose();
                    garbageTimerList.Remove(playerId);
                }
            }
        }

        private void RespawnGarbageVehicle(Vehicle vehicle)
        {
            NAPI.Vehicle.RepairVehicle(vehicle);
            NAPI.Entity.SetEntityPosition(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_POSITION));
            NAPI.Entity.SetEntityRotation(vehicle, NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ROTATION));
        }

        private void OnGarbageTimer(object playerObject)
        {
            try
            {
                Client player = (Client)playerObject;
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);

                // Respawneamos el vehículo
                RespawnGarbageVehicle(vehicle);

                // Cancelamos la ruta
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);
                NAPI.Data.ResetEntityData(target, EntityData.PLAYER_JOB_CHECKPOINT);

                // Borramos el timer de la lista
                Timer garbageTimer = garbageTimerList[playerId];
                if (garbageTimer != null)
                {
                    garbageTimer.Dispose();
                    garbageTimerList.Remove(playerId);
                }

                // Avisamos a los jugadores
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_JOB_VEHICLE_ABANDONED);
                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ERROR + Messages.ERR_JOB_VEHICLE_ABANDONED);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnGarbageTimer] " + ex.Message);
            }
        }

        private void OnGarbageCollectedTimer(object playerObject)
        {
            try
            {
                Client player = (Client)playerObject;
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                Client driver = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);

                // Recogemos la bolsa de basura
                GTANetworkAPI.Object trashBag = NAPI.Data.GetEntityData(player, EntityData.PLAYER_GARBAGE_BAG);
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Entity.DeleteEntity(trashBag);

                // Obtenemos el total de checkpoints
                int route = NAPI.Data.GetEntityData(driver, EntityData.PLAYER_JOB_ROUTE);
                int checkPoint = NAPI.Data.GetEntityData(driver, EntityData.PLAYER_JOB_CHECKPOINT) + 1;
                int totalCheckPoints = Constants.GARBAGE_LIST.Where(x => x.route == route).Count();

                // Obtenemos el checkpoint actual
                Checkpoint garbageCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE);

                if (checkPoint < totalCheckPoints)
                {
                    Vector3 currentGarbagePosition = GetGarbageCheckPointPosition(route, checkPoint);
                    Vector3 nextGarbagePosition = GetGarbageCheckPointPosition(route, checkPoint + 1);

                    // Avanzamos al siguiente checkpoint
                    NAPI.Entity.SetEntityPosition(garbageCheckpoint, currentGarbagePosition);
                    NAPI.Checkpoint.SetCheckpointDirection(garbageCheckpoint, nextGarbagePosition);
                    NAPI.Data.SetEntityData(driver, EntityData.PLAYER_JOB_CHECKPOINT, checkPoint);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT, checkPoint);
                    NAPI.ClientEvent.TriggerClientEvent(driver, "showGarbageCheckPoint", currentGarbagePosition);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showGarbageCheckPoint", currentGarbagePosition);

                    // Añadimos el objeto basura
                    trashBag = NAPI.Object.CreateObject(628215202, currentGarbagePosition, new Vector3(0.0f, 0.0f, 0.0f));
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_GARBAGE_BAG, trashBag);
                }
                else
                {
                    Vector3 garbagePosition = new Vector3(-339.0206f, -1560.117f, 25.23038f);
                    NAPI.Entity.SetEntityModel(garbageCheckpoint, 4);
                    NAPI.Entity.SetEntityPosition(garbageCheckpoint, garbagePosition);
                    NAPI.Chat.SendChatMessageToPlayer(driver, Constants.COLOR_INFO + Messages.INF_ROUTE_FINISHED);
                    NAPI.ClientEvent.TriggerClientEvent(driver, "showGarbageCheckPoint", garbagePosition);
                    NAPI.ClientEvent.TriggerClientEvent(player, "deleteGarbageCheckPoint");
                }

                // Borramos el timer de la lista
                Timer garbageTimer = garbageTimerList[playerId];
                if (garbageTimer != null)
                {
                    garbageTimer.Dispose();
                    garbageTimerList.Remove(playerId);
                }

                // Mandamos el mensaje de que se ha recogido la bolsa de basura
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_GARBAGE_COLLECTED);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnGarbageCollectedTimer] " + ex.Message);
            }
        }

        private Vector3 GetGarbageCheckPointPosition(int route, int checkPoint)
        {
            Vector3 position = new Vector3();
            foreach (GarbageModel garbage in Constants.GARBAGE_LIST)
            {
                if (garbage.route == route && garbage.checkPoint == checkPoint)
                {
                    position = garbage.position;
                    break;
                }
            }
            return position;
        }

        private void FinishGarbageRoute(Client driver, bool canceled = false)
        {
            Vehicle vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(NAPI.Player.GetPlayerVehicle(driver));
            Client partner = NAPI.Data.GetEntityData(driver, EntityData.PLAYER_JOB_PARTNER);
            
            // Respawneamos el vehículo
            RespawnGarbageVehicle(vehicle);

            // Destruímos el check anterior
            Checkpoint garbageCheckpoint = NAPI.Data.GetEntityData(driver, EntityData.PLAYER_JOB_COLSHAPE);
            NAPI.ClientEvent.TriggerClientEvent(driver, "deleteGarbageCheckPoint");
            NAPI.Entity.DeleteEntity(garbageCheckpoint);

            // Limpiamos las variables
            NAPI.Data.ResetEntityData(driver, EntityData.PLAYER_JOB_PARTNER);
            NAPI.Data.ResetEntityData(partner, EntityData.PLAYER_JOB_PARTNER);
            NAPI.Data.ResetEntityData(driver, EntityData.PLAYER_JOB_COLSHAPE);
            NAPI.Data.ResetEntityData(partner, EntityData.PLAYER_GARBAGE_BAG);
            NAPI.Data.ResetEntityData(driver, EntityData.PLAYER_JOB_ROUTE);
            NAPI.Data.ResetEntityData(partner, EntityData.PLAYER_JOB_ROUTE);
            NAPI.Data.ResetEntityData(driver, EntityData.PLAYER_JOB_CHECKPOINT);
            NAPI.Data.ResetEntityData(partner, EntityData.PLAYER_JOB_CHECKPOINT);
            NAPI.Data.ResetEntityData(driver, EntityData.PLAYER_JOB_VEHICLE);
            NAPI.Data.ResetEntityData(partner, EntityData.PLAYER_JOB_VEHICLE);
            NAPI.Data.ResetEntityData(partner, EntityData.PLAYER_ANIMATION);

            if(!canceled)
            {
                // Pagamos a los jugadores lo ganado
                int driverMoney = NAPI.Data.GetEntitySharedData(driver, EntityData.PLAYER_MONEY);
                int partnerMoney = NAPI.Data.GetEntitySharedData(partner, EntityData.PLAYER_MONEY);
                NAPI.Data.SetEntitySharedData(driver, EntityData.PLAYER_MONEY, driverMoney + Constants.MONEY_GARBAGE_ROUTE);
                NAPI.Data.SetEntitySharedData(partner, EntityData.PLAYER_MONEY, partnerMoney + Constants.MONEY_GARBAGE_ROUTE);

                // Mandamos el aviso de dinero recibido
                String message = String.Format(Messages.INF_GARBAGE_EARNINGS, Constants.MONEY_GARBAGE_ROUTE);
                NAPI.Chat.SendChatMessageToPlayer(driver, Constants.COLOR_INFO + message);
                NAPI.Chat.SendChatMessageToPlayer(partner, Constants.COLOR_INFO + message);
            }

            // Sacamos a los ocupantes del vehículo
            NAPI.Player.WarpPlayerOutOfVehicle(driver);
            NAPI.Player.WarpPlayerOutOfVehicle(partner);
        }

        [Command("basurero", Messages.GEN_GARBAGE_JOB_COMMAND)]
        public void BasuraCommand(Client player, String action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_GARBAGE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_GARBAGE);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else
            {
                switch (action.ToLower())
                {
                    case "ruta":
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_ROUTE) == true)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Messages.ERR_ALREADY_IN_ROUTE);
                        }
                        else
                        {
                            Random random = new Random();
                            int garbageRoute = random.Next(Constants.MAX_GARBAGE_ROUTES);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_ROUTE, garbageRoute);
                            switch (garbageRoute)
                            {
                                case 0:
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Se te ha asignado la ruta norte.");
                                    break;
                                case 1:
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Se te ha asignado la ruta sur.");
                                    break;
                                case 2:
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Se te ha asignado la ruta este.");
                                    break;
                                case 3:
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Se te ha asignado la ruta oeste.");
                                    break;
                            }
                        }
                        break;
                    case "recoger":
                        if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_ANY)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_GARBAGE_IN_VEHICLE);
                        }
                        else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_COLSHAPE) == false)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_GARBAGE_NEAR);
                        }
                        else
                        {
                            Checkpoint garbageCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE);
                            if (player.Position.DistanceTo(garbageCheckpoint.Position) < 3.5f)
                            {
                                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                                if (garbageTimerList.TryGetValue(playerId, out Timer garbageTimer) == false)
                                {
                                    NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.AllowPlayerControl), "anim@move_m@trash", "pickup");
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, true);

                                    // Creamos el timer para recoger la basura
                                    garbageTimer = new Timer(OnGarbageCollectedTimer, player, 15000, Timeout.Infinite);
                                    garbageTimerList.Add(playerId, garbageTimer);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_GARBAGE);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_GARBAGE_NEAR);
                            }
                        }
                        break;
                    case "cancelar":
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_PARTNER) == true)
                        {
                            Client partner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                            if(partner != player)
                            {
                                // Tiene un compañero asignado
                                GTANetworkAPI.Object trashBag = null;
                                Checkpoint garbageCheckpoint = null;

                                if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
                                {
                                    // Ha cancelado el conductor
                                    trashBag = NAPI.Data.GetEntityData(player, EntityData.PLAYER_GARBAGE_BAG);
                                    garbageCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE);
                                }
                                else
                                {
                                    // Ha cancelado el acompañante
                                    trashBag = NAPI.Data.GetEntityData(partner, EntityData.PLAYER_GARBAGE_BAG);
                                    garbageCheckpoint = NAPI.Data.GetEntityData(partner, EntityData.PLAYER_JOB_COLSHAPE);
                                }

                                // Borramos la bolsa de basura
                                NAPI.Entity.DeleteEntity(trashBag);

                                // Destruímos el check anterior
                                NAPI.Entity.SetEntityModel(garbageCheckpoint, 4);
                                NAPI.Entity.SetEntityPosition(garbageCheckpoint, new Vector3(-339.0206f, -1560.117f, 25.23038f));

                                if (NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
                                {
                                    // Ha cancelado el conductor
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ROUTE_FINISHED);
                                    NAPI.ClientEvent.TriggerClientEvent(partner, "deleteGarbageCheckPoint");
                                }
                                else
                                {
                                    // Ha cancelado el acompañante
                                    trashBag = NAPI.Data.GetEntityData(partner, EntityData.PLAYER_GARBAGE_BAG);
                                    NAPI.ClientEvent.TriggerClientEvent(player, "deleteGarbageCheckPoint");
                                }
                            }
                            else
                            {
                                // No tiene compañero, enviamos el mensaje
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ROUTE_CANCELED);
                            }

                            // Eliminamos la búsqueda de compañero
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                        }
                        else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_ROUTE) == true)
                        {
                            // Cancelamos la ruta
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_GARBAGE_ROUTE_CANCELED);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_ROUTE);
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_GARBAGE_JOB_COMMAND);
                        break;
                }
            }
        }
    }
}