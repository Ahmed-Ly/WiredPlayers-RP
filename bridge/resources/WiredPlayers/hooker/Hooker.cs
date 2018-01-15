using GTANetworkAPI;
using WiredPlayers.globals;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.hooker
{
    public class Hooker : Script
    {
        public static Dictionary<int, Timer> sexTimerList = new Dictionary<int, Timer>();

        public Hooker()
        {
            Event.OnPlayerDisconnected += onPlayerDisconnected;
        }

        private void onPlayerDisconnected(Client player, byte type, string reason)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                Timer sexTimer = null;
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                if (sexTimerList.TryGetValue(playerId, out sexTimer) == true)
                {
                    sexTimer.Dispose();
                    sexTimerList.Remove(playerId);
                }
            }
        }

        public static void OnSexServiceTimer(object playerObject)
        {
            try
            {
                Client player = (Client)playerObject;
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ALREADY_FUCKING);

                // Paramos las animaciones
                NAPI.Player.StopPlayerAnimation(player);
                NAPI.Player.StopPlayerAnimation(target);

                // Restablecemos la salud del cliente
                NAPI.Player.SetPlayerHealth(player, 100);

                // Reseteamos las variables
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ANIMATION);
                NAPI.Data.ResetEntityData(player, EntityData.HOOKER_TYPE_SERVICE);
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ALREADY_FUCKING);
                NAPI.Data.ResetEntityData(target, EntityData.PLAYER_ALREADY_FUCKING);

                // Borramos el timer de la lista
                Timer sexTimer = sexTimerList[playerId];
                if (sexTimer != null)
                {
                    sexTimer.Dispose();
                    sexTimerList.Remove(playerId);
                }

                // Enviamos el mensaje
                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SUCCESS + Messages.SUC_HOOKER_CLIENT_SATISFIED);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_HOOKER_SERVICE_FINISHED);
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnSexServiceTimer] " + ex.Message);
            }
        }

        [Command("servicio", Messages.GEN_HOOKER_SERVICE_COMMAND)]
        public void ofrecerservicioCommand(Client player, String service, String targetString, int price)
        {
            NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_HOOKER)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_HOOKER);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ALREADY_FUCKING) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_FUCKING);
            }
            else if (NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_PASSENGER)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_PASSENGER);
            }
            else
            {
                int targetId = 0;
                Client target = Int32.TryParse(targetString, out targetId) ? Globals.getPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);
                
                if (NAPI.Player.GetPlayerVehicleSeat(target) != Constants.VEHICLE_SEAT_DRIVER)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CLIENT_NOT_VEHICLE_DRIVING);
                }
                else
                {
                    switch (service.ToLower())
                    {
                        case "oral":
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                            NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, price);
                            NAPI.Data.SetEntityData(target, EntityData.HOOKER_TYPE_SERVICE, Constants.HOOKER_SERVICE_BASIC);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Has ofrecido un servicio oral a un cliente a cambio de " + price + "$, el tendrá que aceptar.");
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + "Una prostituta te ha ofrecido un servicio oral a cambio de " + price + "$. Escribe /aceptar servicio o /cancelar servicio.");
                            break;
                        case "sexo":
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                            NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, price);
                            NAPI.Data.SetEntityData(target, EntityData.HOOKER_TYPE_SERVICE, Constants.HOOKER_SERVICE_FULL);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Has ofrecido sexo completo a un cliente a cambio de " + price + "$, el tendrá que aceptar.");
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + "Una prostituta te ha ofrecido sexo completo a cambio de " + price + "$. Escribe /aceptar servicio o /cancelar servicio.");
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_HOOKER_SERVICE_COMMAND);
                            break;
                    }
                }
            }
        }
    }
}


