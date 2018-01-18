using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.drivingschool;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System;

namespace WiredPlayers.TownHall
{
    class TownHall : Script
    {
        private TextLabel townHallTextLabel;

        public TownHall()
        {
            Event.OnClientEventTrigger += OnClientEvent;
            Event.OnResourceStart += OnResourceStartHandler;
        }

        private void OnResourceStartHandler()
        {
            townHallTextLabel = NAPI.TextLabel.CreateTextLabel("/ayuntamiento", new Vector3(-139.2177f, -631.8386f, 168.86f), 10.0f, 0.5f, 0, new Color(255, 255, 153), false, 0);
            NAPI.TextLabel.CreateTextLabel("Escribe el comando para ver los trámites disponibles", new Vector3(-139.2177f, -631.8386f, 168.76f), 10.0f, 0.5f, 0, new Color(255, 255, 255), false, 0);
        }

        public void OnClientEvent(Client player, String eventName, params object[] arguments)
        {
            if (eventName == "documentOptionSelected")
            {
                int tramitation = Convert.ToInt32(arguments[0]);
                switch (tramitation)
                {
                    case Constants.TRAMITATE_IDENTIFICATION:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_DOCUMENTATION) > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAS_IDENTIFICATION);
                        }
                        else
                        {
                            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                            if (money < Constants.PRICE_IDENTIFICATION)
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
                                Database.LogPayment(player.Name, "Ayuntamiento", "Documentación", Constants.PRICE_IDENTIFICATION);
                            }
                        }
                        break;
                    case Constants.TRAMITATE_MEDICAL_INSURANCE:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE) > Globals.GetTotalSeconds())
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAS_MEDICAL_INSURANCE);
                        }
                        else
                        {
                            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                            if (money < Constants.PRICE_MEDICAL_INSURANCE)
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
                                Database.LogPayment(player.Name, "Ayuntamiento", "Seguro médico", Constants.PRICE_MEDICAL_INSURANCE);
                            }
                        }
                        break;
                    case Constants.TRAMITATE_TAXI_LICENSE:
                        if (DrivingSchool.GetPlayerLicenseStatus(player, Constants.LICENSE_TAXI) > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAS_TAXI_LICENSE);
                        }
                        else
                        {
                            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                            if (money < Constants.PRICE_TAXI_LICENSE)
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
                                Database.LogPayment(player.Name, "Ayuntamiento", "Licencia de taxis", Constants.PRICE_TAXI_LICENSE);
                            }
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
            else if (eventName == "payPlayerFines")
            {
                int amount = Database.GetAllFinesAmount(player.Name);
                int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                if(amount == 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FINES);
                }
                else if (amount > money)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FINE_MONEY);
                }
                else
                {
                    String message = String.Format(Messages.INF_PLAYER_FINES_PAID, amount);
                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - amount);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    Database.LogPayment(player.Name, "Ayuntamiento", "Pago de multas", amount);
                    Database.RemoveAllFines(player.Name);
                }
            }
        }

        [Command("ayuntamiento")]
        public void AyuntamientoCommand(Client player)
        {
            if (player.Position.DistanceTo(townHallTextLabel.Position) < 2.0f)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "showTownHallDocumentsMenu");
            }
            else
            {
                // Avisamos de que no está en el ayuntamiento
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_TOWNHALL);
            }
        }
    }
}