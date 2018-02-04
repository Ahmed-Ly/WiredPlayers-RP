using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.house;
using WiredPlayers.model;
using WiredPlayers.parking;
using System;

namespace WiredPlayers.avatar
{
    public class Avatar
    {
        [Command("jugador")]
        public void JugadorCommand(Client player)
        {
            String sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? "Masculino" : "Femenino";
            String age = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_AGE) + " años";
            String money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY) + "$";
            String bank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK) + "$";
            String job = "Sin trabajo";
            String faction = "Sin facción";
            String rank = "Sin rango";
            String houses = String.Empty;
            String ownedVehicles = String.Empty;
            String lentVehicles = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS);
            TimeSpan played = TimeSpan.FromMinutes(NAPI.Data.GetEntityData(player, EntityData.PLAYER_PLAYED));

            // Miramos si tiene un trabajo
            foreach (JobModel jobModel in Constants.JOB_LIST)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) == jobModel.job)
                {
                    job = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? jobModel.descriptionMale : jobModel.descriptionFemale;
                    break;
                }
            }

            // Miramos si tiene una facción
            foreach (FactionModel factionModel in Constants.FACTION_RANK_LIST)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == factionModel.faction && NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) == factionModel.rank)
                {
                    switch (factionModel.faction)
                    {
                        case Constants.FACTION_POLICE:
                            faction = "Policía";
                            break;
                        case Constants.FACTION_EMERGENCY:
                            faction = "Emergencias";
                            break;
                        case Constants.FACTION_NEWS:
                            faction = "Weazel News";
                            break;
                        case Constants.FACTION_TOWNHALL:
                            faction = "Ayuntamiento";
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            faction = "Servicio de transportes";
                            break;
                        default:
                            faction = "Sin facción";
                            break;
                    }

                    // Establecemos el rango
                    rank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? factionModel.descriptionMale : factionModel.descriptionFemale;
                    break;
                }
            }

            // Miramos si tiene alguna casa alquilada
            if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_RENT_HOUSE) > 0)
            {
                houses += " " + NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_RENT_HOUSE);
            }

            // Miramos si tiene alguna casa en propiedad
            foreach (HouseModel house in House.houseList)
            {
                if (house.owner == player.Name)
                {
                    houses += " " + house.id;
                }
            }

            // Miramos si tiene algún vehículo en propiedad
            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER) == player.Name)
                {
                    ownedVehicles += " " + NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                }
            }

            // Miramos entre los vehículos aparcados
            foreach (ParkedCarModel parkedVehicle in Parking.parkedCars)
            {
                if (parkedVehicle.vehicle.owner == player.Name)
                {
                    ownedVehicles += " " + parkedVehicle.vehicle.id;
                }
            }

            // Mostramos la información
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Datos básicos:");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Nombre: " + player.Name + "; Sexo: " + sex + "; Edad: " + age + "; Dinero: " + money + "; Banco: " + bank);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + " ");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Datos de empleo:");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Trabajo: " + job + "; Facción: " + faction + "; Rango: " + rank);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + " ");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Propiedades:");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Casas: " + houses);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Vehículos propios: " + ownedVehicles);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Vehículos cedidos: " + lentVehicles);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + " ");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Otros datos:");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Tiempo jugado: " + (int)played.TotalHours + "h " + played.Minutes + "m");
        }
    }
}
