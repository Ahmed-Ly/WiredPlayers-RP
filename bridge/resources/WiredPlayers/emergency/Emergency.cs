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
            // Destroy death timer
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
                // Check if the player was dead
                int databaseKiller = NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED);

                if (databaseKiller == 0)
                {
                    // There's no killer, we set the environment as killer
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_KILLED, Constants.ENVIRONMENT_KILL);
                }
            }
            else
            {
                int killerId = NAPI.Data.GetEntityData(killer, EntityData.PLAYER_SQL_ID);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_KILLED, killerId);
            }

            // We remove the timer from the list
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
            
            Vector3 deathPosition = null;
            String deathPlace = String.Empty;
            String deathHour = DateTime.Now.ToString("h:mm:ss tt");

            // Checking if player died into a house or business
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

            // We add the report to the list
            FactionWarningModel factionWarning = new FactionWarningModel(Constants.FACTION_EMERGENCY, player.Value, deathPlace, deathPosition, -1, deathHour);
            Faction.factionWarningList.Add(factionWarning);

            // Report message
            String warnMessage = String.Format(Messages.INF_EMERGENCY_WARNING, Faction.factionWarningList.Count - 1);

            // Sending the report to all the emergency department's members
            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) > 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + warnMessage);
                }
            }

            // Timer to process player's death
            Timer deathTimer = new Timer(OnDeathTimer, death, 2500, Timeout.Infinite);
            deathTimerList.Add(player.Value, deathTimer);
        }

        [Command(Commands.COMMAND_HEAL, Messages.GEN_HEAL_COMMAND)]
        public void HealCommand(Client player, String targetString)
        {
            Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

            if (target != null && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
            {
                if (NAPI.Player.GetPlayerHealth(target) < 100)
                {
                    String playerMessage = String.Format(Messages.INF_MEDIC_HEALED_PLAYER, target.Name);
                    String targetMessage = String.Format(Messages.INF_PLAYER_HEALED_MEDIC, player.Name);

                    // We heal the character
                    NAPI.Player.SetPlayerHealth(target, 100);

                    foreach (Client targetPlayer in NAPI.Pools.GetAllPlayers())
                    {
                        if (targetPlayer.Position.DistanceTo(player.Position) < 20.0f)
                        {
                            String message = String.Format(Messages.INF_MEDIC_REANIMATED, player.Name, target.Name);
                            NAPI.Chat.SendChatMessageToPlayer(targetPlayer, Constants.COLOR_CHAT_ME + message);
                        }
                    }
                    
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

        [Command(Commands.COMMAND_REANIMATE, Messages.GEN_REANIMATE_COMMAND)]
        public void ReanimateCommand(Client player, String targetString)
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
                            DestroyDeathTimer(target);

                            // We create blood model
                            BloodModel bloodModel = new BloodModel();
                            bloodModel.doctor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                            bloodModel.patient = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
                            bloodModel.type = String.Empty;
                            bloodModel.used = true;

                            // Add the blood consumption to the database
                            bloodModel.id = Database.AddBloodTransaction(bloodModel);
                            bloodList.Add(bloodModel);

                            String playerMessage = String.Format(Messages.INF_PLAYER_REANIMATED, target.Name);
                            String targetMessage = String.Format(Messages.SUC_TARGET_REANIMATED, player.Name);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + playerMessage);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SUCCESS + targetMessage);
                        }
                        else
                        {
                            // There's no blood left
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

        [Command(Commands.COMMAND_EXTRACT, Messages.GEN_EXTRACT_COMMAND)]
        public void ExtractCommand(Client player, String targetString)
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

                if (target != null && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                {
                    if (NAPI.Player.GetPlayerHealth(target) > 15)
                    {
                        // We create the blood model
                        BloodModel blood = new BloodModel();
                        blood.doctor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        blood.patient = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
                        blood.type = String.Empty;
                        blood.used = false;

                        // We add the blood unit to the database
                        blood.id = Database.AddBloodTransaction(blood);
                        bloodList.Add(blood);
                        
                        NAPI.Player.SetPlayerHealth(target, NAPI.Player.GetPlayerHealth(target) - 15);

                        String playerMessage = String.Format(Messages.INF_BLOOD_EXTRACTED, target.Name);
                        String targetMessage = String.Format(Messages.INF_BLOOD_EXTRACTED, player.Name);
                        NAPI.Chat.SendChatMessageToPlayer(player, playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, targetMessage);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Messages.ERR_LOW_BLOOD);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Commands.COMMAND_DIE)]
        public void DieCommand(Client player)
        {
            // Check if the player is dead
            if (NAPI.Data.HasEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN) == true)
            {
                int totalSeconds = Globals.GetTotalSeconds();

                if (NAPI.Data.GetEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN) <= totalSeconds)
                {
                    // Move player to the hospital
                    TeleportPlayerToHospital(player);
                    DestroyDeathTimer(player);

                    // Get the report generated with the death
                    FactionWarningModel factionWarn = Faction.GetFactionWarnByTarget(player.Value, Constants.FACTION_EMERGENCY);

                    if (factionWarn != null)
                    {
                        if (factionWarn.takenBy >= 0)
                        {
                            // Tell the player who attended the report it's been canceled
                            Client doctor = Globals.GetPlayerById(factionWarn.takenBy);
                            NAPI.Chat.SendChatMessageToPlayer(doctor, Constants.COLOR_INFO + Messages.INF_FACTION_WARN_CANCELED);
                        }

                        // Remove the report from the list
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