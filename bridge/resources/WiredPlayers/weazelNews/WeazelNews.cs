using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System;

namespace WiredPlayers.weazelNews
{
    public class WeazelNews : Script
    {
        public static List<AnnoucementModel> annoucementList;

        public static void SendNewsMessage(Client player, String message)
        {
            // Comprobación de la longitud del mensaje
            String secondMessage = String.Empty;

            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ON_AIR) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_NEWS)
            {
                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // El mensaje tiene una longitud de dos líneas
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                    {
                        // Envío del mensaje
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_NEWS + "Locutor " + player.Name + ": " + message + "..." : Constants.COLOR_NEWS + "Locutor " + player.Name + ": " + message);
                        if (secondMessage.Length > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_NEWS + secondMessage);
                        }
                    }
                }
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ON_AIR))
            {
                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // El mensaje tiene una longitud de dos líneas
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                    {
                        // Envío del mensaje
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_NEWS + "Invitado " + player.Name + ": " + message + "..." : Constants.COLOR_NEWS + "Invitado " + player.Name + ": " + message);
                        if (secondMessage.Length > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_NEWS + secondMessage);
                        }
                    }
                }
            }
            else
            {
                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // El mensaje tiene una longitud de dos líneas
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                    {
                        // Envío del mensaje
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_NEWS + "[ANUNCIO] " + message + "..." : Constants.COLOR_NEWS + "[ANUNCIO] " + message);
                        if (secondMessage.Length > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_NEWS + secondMessage);
                        }
                    }
                }
            }
        }

        [Command("cortardirecto", Messages.GEN_CUT_ON_AIR_COMMAND)]
        public void CortardirectoCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_NEWS_FACTION);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                // Miramos si está en un directo
                if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_ON_AIR) == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_ON_AIR);
                }
                else if (target == player)
                {
                    foreach (Client interviewed in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(interviewed, EntityData.PLAYER_ON_AIR) && interviewed != player)
                        {
                            NAPI.Data.ResetEntityData(interviewed, EntityData.PLAYER_ON_AIR);
                            NAPI.Data.ResetEntityData(interviewed, EntityData.PLAYER_JOB_PARTNER);
                            NAPI.Chat.SendChatMessageToPlayer(interviewed, Constants.COLOR_INFO + Messages.INF_PLAYER_ON_AIR_CUTTED);
                        }
                    }

                    // Mandamos
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ON_AIR);
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_REPORTER_ON_AIR_CUTTED);
                }
                else
                {
                    NAPI.Data.ResetEntityData(target, EntityData.PLAYER_ON_AIR);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_REPORTER_ON_AIR_CUTTED);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_PLAYER_ON_AIR_CUTTED);
                }
            }
        }

        [Command("entrevistar", Messages.GEN_OFFER_ON_AIR_COMMAND)]
        public void EntrevistarCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_NEWS_FACTION);
            }
            else
            {
                Vehicle vehicle = Globals.GetClosestVehicle(player);
                if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) != Constants.FACTION_NEWS && NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_LEFT_REAR)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_NEWS_VAN);
                }
                else
                {
                    Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_ON_AIR) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ON_AIR);
                    }
                    else
                    {
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ON_AIR, target);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_PARTNER, target);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WZ_OFFER_ONAIR);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_WZ_ACCEPT_ONAIR);
                    }
                }
            }
        }

        private int GetRemainingFounds()
        {
            int remaining = 0;

            foreach (AnnoucementModel announcement in annoucementList)
            {
                if (announcement.given)
                {
                    remaining -= announcement.amount;
                }
                else
                {
                    remaining += announcement.amount;
                }
            }
            return remaining;
        }

        [Command("premiar", Messages.GEN_NEWS_PRIZE, GreedyArg = true)]
        public void PremiarCommand(Client player, String targetString, int prize, string contest)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_NEWS_FACTION);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null && NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                {
                    int prizeAmount = GetRemainingFounds();
                    if (prizeAmount >= prize)
                    {
                        AnnoucementModel prizeModel = new AnnoucementModel();
                        int targetMoney = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY);

                        String playerMessage = String.Format(Messages.INF_PRIZE_GIVEN, prize, target.Name);
                        String targetMessage = String.Format(Messages.INF_PRIZE_RECEIVED, player.Name, prize, contest);

                        targetMoney += prize;
                        NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_MONEY, targetMoney);

                        prizeModel.amount = prize;
                        prizeModel.winner = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
                        prizeModel.annoucement = contest;
                        prizeModel.journalist = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        prizeModel.given = true;

                        prizeModel.id = Database.GivePrize(prizeModel);
                        annoucementList.Add(prizeModel);

                        // Mandamos el mensaje a los jugadores
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                        // Logeamos el dinero del premio
                        Database.LogPayment(player.Name, target.Name, "Premio WeazelNews", prize);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.ERR_FACTION_NOT_ENOUGH_MONEY);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("anunciar", Messages.GEN_NEWS_ANNOUCEMENT, GreedyArg = true)]
        public void AnunciarCommand(Client player, string message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                if (money < Constants.PRICE_ANNOUNCEMENT)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                }
                else
                {
                    AnnoucementModel annoucement = new AnnoucementModel();
                    annoucement.winner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    annoucement.amount = Constants.PRICE_ANNOUNCEMENT;
                    annoucement.annoucement = message;
                    annoucement.given = false;

                    annoucement.id = Database.SendAnnoucement(annoucement);

                    // Le quitamos el dinero al jugador
                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_ANNOUNCEMENT);

                    SendNewsMessage(player, message);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Has publicado un anuncio, anunciarse cuesta 500$.");

                    // Metemos el log en la base de datos
                    Database.LogPayment(player.Name, "Arca WN", "Anuncio WeazelNews", Constants.PRICE_ANNOUNCEMENT);
                }
            }
        }

        [Command("n", Messages.GEN_NEWS_COMMAND, GreedyArg = true)]
        public void NCommand(Client player, string message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_NEWS_FACTION);
            }
            else
            {
                Vehicle vehicle = Globals.GetClosestVehicle(player);
                if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) != Constants.FACTION_NEWS && NAPI.Player.GetPlayerVehicleSeat(player) != Constants.VEHICLE_SEAT_LEFT_REAR)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_NEWS_VAN);
                }
                else
                {
                    SendNewsMessage(player, message);
                }
            }
        }
    }
}

