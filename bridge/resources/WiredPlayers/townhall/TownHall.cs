using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.drivingschool;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace WiredPlayers.TownHall
{
    public class TownHall : Script
    {
        private TextLabel townHallTextLabel;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            townHallTextLabel = NAPI.TextLabel.CreateTextLabel("/" + Messages.COM_TOWNHALL, new Vector3(-139.2177f, -631.8386f, 168.86f), 10.0f, 0.5f, 4, new Color(255, 255, 153), false, 0);
            NAPI.TextLabel.CreateTextLabel(Messages.GEN_TOWNHALL_HELP, new Vector3(-139.2177f, -631.8386f, 168.76f), 10.0f, 0.5f, 4, new Color(255, 255, 255), false, 0);
        }

        [RemoteEvent("documentOptionSelected")]
        public void DocumentOptionSelectedEvent(Client player, int tramitation)
        {
            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

            switch (tramitation)
            {
                case Constants.TRAMITATE_IDENTIFICATION:
                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_DOCUMENTATION) > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAS_IDENTIFICATION);
                    }
                    else if (money < Constants.PRICE_IDENTIFICATION)
                    {
                        String message = String.Format(Messages.ERR_PLAYER_NOT_IDENTIFICATION_MONEY, Constants.PRICE_IDENTIFICATION);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                    }
                    else
                    {
                        String message = String.Format(Messages.INF_PLAYER_HAS_INDENTIFICATION, Constants.PRICE_IDENTIFICATION);
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_IDENTIFICATION);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DOCUMENTATION, Globals.GetTotalSeconds());
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                        Database.LogPayment(player.Name, Messages.GEN_FACTION_TOWNHALL, Messages.GEN_IDENTIFICATION, Constants.PRICE_IDENTIFICATION);
                    }
                    break;
                case Constants.TRAMITATE_MEDICAL_INSURANCE:
                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE) > Globals.GetTotalSeconds())
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAS_MEDICAL_INSURANCE);
                    }
                    else if (money < Constants.PRICE_MEDICAL_INSURANCE)
                    {
                        String message = String.Format(Messages.ERR_PLAYER_NOT_MEDICAL_INSURANCE_MONEY, Constants.PRICE_MEDICAL_INSURANCE);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                    }
                    else
                    {
                        String message = String.Format(Messages.INF_PLAYER_HAS_MEDICAL_INSURANCE, Constants.PRICE_MEDICAL_INSURANCE);
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_MEDICAL_INSURANCE);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE, Globals.GetTotalSeconds() + 1209600);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                        Database.LogPayment(player.Name, Messages.GEN_FACTION_TOWNHALL, Messages.GEN_MEDICAL_INSURANCE, Constants.PRICE_MEDICAL_INSURANCE);
                    }
                    break;
                case Constants.TRAMITATE_TAXI_LICENSE:
                    if (DrivingSchool.GetPlayerLicenseStatus(player, Constants.LICENSE_TAXI) > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAS_TAXI_LICENSE);
                    }
                    else if (money < Constants.PRICE_TAXI_LICENSE)
                    {
                        String message = String.Format(Messages.ERR_PLAYER_NOT_TAXI_LICENSE_MONEY, Constants.PRICE_TAXI_LICENSE);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                    }
                    else
                    {
                        String message = String.Format(Messages.INF_PLAYER_HAS_TAXI_LICENSE, Constants.PRICE_TAXI_LICENSE);
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - Constants.PRICE_TAXI_LICENSE);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                        DrivingSchool.SetPlayerLicense(player, Constants.LICENSE_TAXI, 1);
                        Database.LogPayment(player.Name, Messages.GEN_FACTION_TOWNHALL, Messages.GEN_TAXI_LICENSE, Constants.PRICE_TAXI_LICENSE);
                    }
                    break;
                case Constants.TRAMITATE_FINE_LIST:
                    List<FineModel> fineList = Database.LoadPlayerFines(player.Name);
                    if (fineList.Count > 0)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerFineList", NAPI.Util.ToJson(fineList));
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_NO_FINES);
                    }
                    break;
            }
        }

        [RemoteEvent("payPlayerFines")]
        public void PayPlayerFinesEvent(Client player, String finesJson)
        {
            List<FineModel> fineList = Database.LoadPlayerFines(player.Name);
            List<FineModel> removedFines = JsonConvert.DeserializeObject<List<FineModel>>(finesJson);
            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
            int finesProcessed = 0;
            int amount = 0;

            // Get the money amount for all the fines
            foreach (FineModel fine in removedFines)
            {
                amount += fine.amount;
                finesProcessed++;
            }

            if (amount == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FINES);
            }
            else if (amount > money)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FINE_MONEY);
            }
            else
            {
                // Remove money from player
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - amount);

                // Delete paid fines
                Database.RemoveFines(removedFines);
                Database.LogPayment(player.Name, Messages.GEN_FACTION_TOWNHALL, Messages.GEN_FINES_PAYMENT, amount);

                // Check if all fines were paid
                if (finesProcessed == fineList.Count)
                {
                    // Volvemos a la página anterior
                    NAPI.ClientEvent.TriggerClientEvent(player, "backTownHallIndex");
                }

                String message = String.Format(Messages.INF_PLAYER_FINES_PAID, amount);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
            }
        }

        [Command(Messages.COM_TOWNHALL)]
        public void TownHallCommand(Client player)
        {
            if (player.Position.DistanceTo(townHallTextLabel.Position) < 2.0f)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "showTownHallMenu");
            }
            else
            {
                // Player is not in the town hall
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_TOWNHALL);
            }
        }
    }
}