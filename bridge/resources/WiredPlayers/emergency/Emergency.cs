using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.globals;
using WiredPlayers.database;
using WiredPlayers.house;
using WiredPlayers.business;
using WiredPlayers.faction;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.emergency
{
    public class Emergency : Script
    {
        public static List<BloodModel> bloodList;
        private static Dictionary<int, Timer> deathTimerList = new Dictionary<int, Timer>();

        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            // Eliminamos el temporizador de muerte
            DestroyDeathTimer(player);
        }

        public void OnDeathTimer(object death)
        {
            Client player = ((DeathModel)death).player;
            Client killer = ((DeathModel)death).killer;
            uint weapon = ((DeathModel)death).weapon;
            int totalSeconds = Globals.GetTotalSeconds();

            if (killer.Value == Constants.ENVIRONMENT_KILL)
            {
                // Miramos si estaba muerto
                int databaseKiller = NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED);

                if (databaseKiller == 0)
                {
                    // Ponemos al entorno como causante real
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_KILLED, Constants.ENVIRONMENT_KILL);
                }
            }
            else
            {
                int killerId = NAPI.Data.GetEntityData(killer, EntityData.PLAYER_SQL_ID);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_KILLED, killerId);
            }

            // Borramos el timer de la lista
            Timer deathTimer = deathTimerList[player.Value];
            if (deathTimer != null)
            {
                deathTimer.Dispose();
                deathTimerList.Remove(player.Value);
            }

            NAPI.Entity.SetEntityInvincible(player, true);
            NAPI.Data.SetEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN, totalSeconds + 240);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_EMERGENCY_WARN);
        }

        private int GetRemainingBlood()
        {
            int remaining = 0;
            foreach (BloodModel blood in bloodList)
            {
                if (blood.used)
                {
                    remaining--;
                }
                else
                {
                    remaining++;
                }
            }
            return remaining;
        }

        public static void DestroyDeathTimer(Client player)
        {
            if (deathTimerList.TryGetValue(player.Value, out Timer deathTimer) == true)
            {
                deathTimer.Dispose();
                deathTimerList.Remove(player.Value);
            }

            NAPI.Entity.SetEntityInvincible(player, false);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_KILLED, 0);
            NAPI.Data.ResetEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN);
        }

        private void TeleportPlayerToHospital(Client player)
        {
            Vector3 hospital = new Vector3(-1385.481f, -976.4036f, 9.273162f);
            NAPI.Data.ResetEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
            NAPI.Entity.SetEntityPosition(player, hospital);
            NAPI.Entity.SetEntityDimension(player, 0);
            NAPI.Entity.SetEntityInvincible(player, false);
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Client player, Client killer, uint weapon)
        {
            DeathModel death = new DeathModel(player, killer, weapon);

            // Creamos las variables para dar el aviso
            Vector3 deathPosition = null;
            String deathPlace = String.Empty;
            String deathHour = DateTime.Now.ToString("h:mm:ss tt");

            // Miramos el lugar donde ha muerto
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) > 0)
            {
                int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                HouseModel house = House.GetHouseById(houseId);
                deathPosition = house.position;
                deathPlace = house.name;
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) > 0)
            {
                int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                BusinessModel business = Business.GetBusinessById(businessId);
                deathPosition = business.position;
                deathPlace = business.name;
            }
            else
            {
                deathPosition = NAPI.Entity.GetEntityPosition(player);
            }

            // Creamos el aviso y lo añadimos a la lista
            FactionWarningModel factionWarning = new FactionWarningModel(Constants.FACTION_EMERGENCY, player.Value, deathPlace, deathPosition, -1, deathHour);
            Faction.factionWarningList.Add(factionWarning);

            // Creamos el mensaje de aviso
            String warnMessage = String.Format(Messages.INF_EMERGENCY_WARNING, Faction.factionWarningList.Count - 1);

            // Damos el aviso a todos los médicos de servicio
            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) > 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + warnMessage);
                }
            }

            // Creamos el timer para poner el estado de muerto
            Timer deathTimer = new Timer(OnDeathTimer, death, 2500, Timeout.Infinite);
            deathTimerList.Add(player.Value, deathTimer);
        }

        [Command("curar", Messages.GEN_HEAL_COMMAND)]
        public void CurarCommand(Client player, String targetString)
        {
            Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

            if (target != null && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
            {
                if (NAPI.Player.GetPlayerHealth(target) < 100)
                {
                    String playerMessage = String.Format(Messages.INF_MEDIC_HEALED_PLAYER, target.Name);
                    String targetMessage = String.Format(Messages.INF_PLAYER_HEALED_MEDIC, player.Name);

                    // Curamos al personaje
                    NAPI.Player.SetPlayerHealth(target, 100);

                    foreach (Client targetPlayer in NAPI.Pools.GetAllPlayers())
                    {
                        if (targetPlayer.Position.DistanceTo(player.Position) < 20.0f)
                        {
                            String message = String.Format(Messages.INF_MEDIC_REANIMATED, player.Name, target.Name);
                            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, Constants.COLOR_CHAT_ME + message);
                        }
                    }

                    // Enviamos el mensaje de aviso
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_HURT);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
            }
        }

        [Command("reanimar", Messages.GEN_REANIMATE_COMMAND)]
        public void ReanimarCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_EMERGENCY)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_EMERGENCY_FACTION);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_KILLED) != 0)
                    {
                        if (GetRemainingBlood() > 0)
                        {
                            // Eliminamos el timer
                            DestroyDeathTimer(target);

                            // Creamos el modelo
                            BloodModel bloodModel = new BloodModel();
                            bloodModel.doctor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                            bloodModel.patient = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
                            bloodModel.type = String.Empty;
                            bloodModel.used = true;

                            // Añadimos la sangre a la base de datos
                            bloodModel.id = Database.AddBloodTransaction(bloodModel);
                            bloodList.Add(bloodModel);

                            String playerMessage = String.Format(Messages.INF_PLAYER_REANIMATED, target.Name);
                            String targetMessage = String.Format(Messages.SUC_TARGET_REANIMATED, player.Name);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SUCCESS + targetMessage);
                        }
                        else
                        {
                            // No queda sangre en las reservas
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_BLOOD_LEFT);
                        }
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_DEAD);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("extraer", Messages.GEN_EXTRACT_COMMAND)]
        public void ExtraerCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null && NAPI.Data.GetEntityData(player, "PLAYER_FACTION") == Constants.FACTION_EMERGENCY)
                {
                    if (NAPI.Player.GetPlayerHealth(target) > 15)
                    {
                        // Creamos el modelo
                        BloodModel blood = new BloodModel();
                        blood.doctor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        blood.patient = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
                        blood.type = String.Empty;
                        blood.used = false;

                        // Añadimos la sangre a la base de datos
                        blood.id = Database.AddBloodTransaction(blood);
                        bloodList.Add(blood);

                        NAPI.Player.SetPlayerHealth(target, NAPI.Player.GetPlayerHealth(target) - 15);
                        NAPI.Chat.SendChatMessageToPlayer(player, String.Format("Sangre extraida, ahora tiene {0}", NAPI.Player.GetPlayerHealth(target)));
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, String.Format("El jugador {0} tiene muy poca sangre, por lo tanto, no se le puede extraer", targetString));
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("morir")]
        public void MorirCommand(Client player)
        {
            // Miramos si está muerto y esperando a ir al hospital
            if (NAPI.Data.HasEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN) == true)
            {
                int totalSeconds = Globals.GetTotalSeconds();

                if (NAPI.Data.GetEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN) <= totalSeconds)
                {
                    // Movemos al jugador al hospital
                    TeleportPlayerToHospital(player);

                    // Eliminamos el timer
                    DestroyDeathTimer(player);

                    // Obtenemos el aviso
                    FactionWarningModel factionWarn = Faction.GetFactionWarnByTarget(player.Value, Constants.FACTION_EMERGENCY);

                    if (factionWarn != null)
                    {
                        // Miramos si está atendido el aviso
                        if (factionWarn.takenBy >= 0)
                        {
                            Client doctor = Globals.GetPlayerById(factionWarn.takenBy);
                            NAPI.Chat.SendChatMessageToPlayer(doctor, Constants.COLOR_INFO + Messages.INF_FACTION_WARN_CANCELED);
                        }

                        // Borramos el aviso de la lista
                        Faction.factionWarningList.Remove(factionWarn);
                    }

                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_DEATH_TIME_NOT_PASSED);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_DEAD);
            }
        }
    }
}