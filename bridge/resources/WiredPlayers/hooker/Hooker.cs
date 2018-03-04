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

        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (sexTimerList.TryGetValue(player.Value, out Timer sexTimer) == true)
            {
                sexTimer.Dispose();
                sexTimerList.Remove(player.Value);
            }
        }

        public static void OnSexServiceTimer(object playerObject)
        {
            Client player = (Client)playerObject;
            Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ALREADY_FUCKING);

            // We stop both animations
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Player.StopPlayerAnimation(target);

            // Health the player
            NAPI.Player.SetPlayerHealth(player, 100);
            
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ANIMATION);
            NAPI.Data.ResetEntityData(player, EntityData.HOOKER_TYPE_SERVICE);
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ALREADY_FUCKING);
            NAPI.Data.ResetEntityData(target, EntityData.PLAYER_ALREADY_FUCKING);
            
            if (sexTimerList.TryGetValue(player.Value, out Timer sexTimer) == true)
            {
                sexTimer.Dispose();
                sexTimerList.Remove(player.Value);
            }

            // Send finish message to both players
            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SUCCESS + Messages.SUC_HOOKER_CLIENT_SATISFIED);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_HOOKER_SERVICE_FINISHED);
        }

        [Command(Messages.COM_SERVICE, Messages.GEN_HOOKER_SERVICE_COMMAND)]
        public void ServiceCommand(Client player, String service, String targetString, int price)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_HOOKER)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_HOOKER);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ALREADY_FUCKING) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_FUCKING);
            }
            else if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.RightFront)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_PASSENGER);
            }
            else
            {
                NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (NAPI.Player.GetPlayerVehicleSeat(target) != (int)VehicleSeat.Driver)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CLIENT_NOT_VEHICLE_DRIVING);
                }
                else
                {
                    String playerMessage = String.Empty;
                    String targetMessage = String.Empty;

                    switch (service.ToLower())
                    {
                        case Messages.ARG_ORAL:
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                            NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, price);
                            NAPI.Data.SetEntityData(target, EntityData.HOOKER_TYPE_SERVICE, Constants.HOOKER_SERVICE_BASIC);

                            playerMessage = String.Format(Messages.INF_ORAL_SERVICE_OFFER, target.Name, price);
                            targetMessage = String.Format(Messages.INF_ORAL_SERVICE_RECEIVE, player.Name, price);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            break;
                        case Messages.ARG_SEX:
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                            NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, price);
                            NAPI.Data.SetEntityData(target, EntityData.HOOKER_TYPE_SERVICE, Constants.HOOKER_SERVICE_FULL);

                            playerMessage = String.Format(Messages.INF_SEX_SERVICE_OFFER, target.Name, price);
                            targetMessage = String.Format(Messages.INF_SEX_SERVICE_RECEIVE, player.Name, price);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
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


