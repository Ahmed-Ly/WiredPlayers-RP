using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.globals;
using WiredPlayers.chat;
using WiredPlayers.database;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;

namespace WiredPlayers.faction
{
    public class Faction : Script
    {
        public static List<ChannelModel> channelList;
        public static List<FactionWarningModel> factionWarningList;

        public static String GetPlayerFactionRank(Client player)
        {
            String rankString = String.Empty;
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
            int rank = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK);
            foreach (FactionModel factionModel in Constants.FACTION_RANK_LIST)
            {
                if (factionModel.faction == faction && factionModel.rank == rank)
                {
                    rankString = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? factionModel.descriptionMale : factionModel.descriptionFemale;
                    break;
                }
            }
            return rankString;
        }

        public static FactionWarningModel GetFactionWarnByTarget(int playerId, int faction)
        {
            FactionWarningModel warn = null;
            foreach (FactionWarningModel factionWarn in factionWarningList)
            {
                if (factionWarn.playerId == playerId && factionWarn.faction == faction)
                {
                    warn = factionWarn;
                    break;
                }
            }
            return warn;
        }

        private ChannelModel GetPlayerOwnedChannel(int playerId)
        {
            ChannelModel channel = null;
            foreach (ChannelModel channelModel in channelList)
            {
                if (channelModel.owner == playerId)
                {
                    channel = channelModel;
                    break;
                }
            }
            return channel;
        }

        private String GetMd5Hash(MD5 md5Hash, String input)
        {
            StringBuilder sBuilder = new StringBuilder();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private bool CheckInternalAffairs(int faction, Client target)
        {
            bool isInternalAffairs = false;

            if (faction == Constants.FACTION_TOWNHALL && (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && NAPI.Data.GetEntityData(target, EntityData.PLAYER_RANK) == 7))
            {
                isInternalAffairs = true;
            }

            return isInternalAffairs;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            factionWarningList = new List<FactionWarningModel>();
        }

        [ServerEvent(Event.PlayerEnterCheckpoint)]
        public void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_FACTION_WARNING) == true)
            {
                Checkpoint locationCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION_WARNING);
                NAPI.Entity.DeleteEntity(locationCheckpoint);

                // Delete map blip
                NAPI.ClientEvent.TriggerClientEvent(player, "deleteFactionWarning");
                
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_FACTION_WARNING);

                // Remove the report
                factionWarningList.RemoveAll(x => x.takenBy == player.Value);
            }
        }

        [Command(Commands.COMMAND_F, Messages.GEN_F_COMMAND, GreedyArg = true)]
        public void FCommand(Client player, String message)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
            if (faction > 0 && faction < Constants.LAST_STATE_FACTION)
            {
                String rank = GetPlayerFactionRank(player);
                
                String secondMessage = String.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // We need two lines to write the message
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == faction)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_FACTION + "(([ID: " + player.Value + "] " + rank + " " + player.Name + ": " + message + "..." : Constants.COLOR_CHAT_FACTION + "(([ID: " + player.Value + "] " + rank + " " + player.Name + ": " + message + "))");
                        if (secondMessage.Length > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_FACTION + secondMessage + "))");
                        }
                    }
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_STATE_FACTION);
            }
        }

        [Command(Commands.COMMAND_R, Messages.GEN_R_COMMAND, GreedyArg = true)]
        public void RCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                if (faction > 0 && faction < Constants.LAST_STATE_FACTION)
                {
                    // Get player's rank in faction
                    String rank = GetPlayerFactionRank(player);
                    
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == faction || CheckInternalAffairs(faction, target) == true))
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_RADIO + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);

                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_STATE_FACTION);
                }
            }
        }

        [Command(Commands.COMMAND_DP, Messages.GEN_DP_COMMAND, GreedyArg = true)]
        public void DpCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                {
                    String rank = GetPlayerFactionRank(player);
                    
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_RADIO + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                        else if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO_POLICE + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_RADIO_POLICE + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO_POLICE + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_EMERGENCY_FACTION);
                }
            }
        }

        [Command(Commands.COMMAND_DE, Messages.GEN_DE_COMMAND, GreedyArg = true)]
        public void DeCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                {
                    String rank = GetPlayerFactionRank(player);
                    
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO_POLICE + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_RADIO_POLICE + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO_POLICE + secondMessage);
                            }
                        }
                        else if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_RADIO + Messages.GEN_RADIO + rank + " " + player.Name + Messages.GEN_CHAT_SAY + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_EMERGENCY_FACTION);
                }
            }
        }

        [Command(Commands.COMMAND_FR, Messages.GEN_FR_COMMAND, GreedyArg = true)]
        public void FrCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                int radio = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RADIO);
                if (radio > 0)
                {
                    String name = NAPI.Data.GetEntityData(player, EntityData.PLAYER_NAME);
                    
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_RADIO) == radio)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + Messages.GEN_RADIO + name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_RADIO + Messages.GEN_RADIO + name + Messages.GEN_CHAT_SAY + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RADIO_FREQUENCY_NONE);
                }
            }
        }

        [Command(Commands.COMMAND_FREQUENCY, Messages.GEN_FREQUENCY_COMMAND, GreedyArg = true)]
        public void FrequencyCommand(Client player, String args)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                ItemModel item = Globals.GetItemModelFromId(itemId);
                if (item != null && item.hash == Constants.ITEM_HASH_WALKIE)
                {
                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    ChannelModel ownedChannel = GetPlayerOwnedChannel(playerId);
                    String[] arguments = args.Trim().Split(' ');
                    switch (arguments[0].ToLower())
                    {
                        case Commands.ARGUMENT_CREATE:
                            if (arguments.Length == 2)
                            {
                                if (ownedChannel == null)
                                {
                                    // We create the new frequency
                                    MD5 md5Hash = MD5.Create();
                                    ChannelModel channel = new ChannelModel();
                                    channel.owner = playerId;
                                    channel.password = GetMd5Hash(md5Hash, arguments[1]);
                                    channel.id = Database.AddChannel(channel);
                                    channelList.Add(channel);

                                    // Sending the message with created channel
                                    String message = String.Format(Messages.INF_CHANNEL_CREATED, channel.id);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_OWNED_CHANNEL);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_FREQUENCY_CREATE_COMMAND);
                            }
                            break;
                        case Commands.ARGUMENT_MODIFY:
                            if (arguments.Length == 2)
                            {
                                if (ownedChannel != null)
                                {
                                    MD5 md5Hash = MD5.Create();
                                    ownedChannel.password = GetMd5Hash(md5Hash, arguments[1]);
                                    Database.UpdateChannel(ownedChannel);

                                    // We kick all the players from the channel
                                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                                    {
                                        int targetId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_RADIO) == ownedChannel.id && targetId != ownedChannel.owner)
                                        {
                                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_RADIO, 0);
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CHANNEL_DISCONNECTED);
                                        }
                                    }
                                    Database.DisconnectFromChannel(ownedChannel.id);

                                    // Message sent with the confirmation
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CHANNEL_UPDATED);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_OWNED_CHANNEL);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_FREQUENCY_MODIFY_COMMAND);
                            }
                            break;
                        case Commands.ARGUMENT_REMOVE:
                            if (ownedChannel != null)
                            {
                                // We kick all the players from the channel
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    int targetId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                                    if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_RADIO) == ownedChannel.id)
                                    {
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_RADIO, 0);
                                        if (ownedChannel.owner != targetId)
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CHANNEL_DISCONNECTED);
                                        }
                                    }
                                }
                                Database.DisconnectFromChannel(ownedChannel.id);

                                // We destroy the channel
                                Database.RemoveChannel(ownedChannel.id);
                                channelList.Remove(ownedChannel);

                                // Message sent with the confirmation
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CHANNEL_DELETED);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_OWNED_CHANNEL);
                            }
                            break;
                        case Commands.ARGUMENT_CONNECT:
                            if (arguments.Length == 3)
                            {
                                if (Int32.TryParse(arguments[1], out int frequency) == true)
                                {
                                    // We encrypt the password
                                    MD5 md5Hash = MD5.Create();
                                    String password = GetMd5Hash(md5Hash, arguments[2]);

                                    foreach (ChannelModel channel in channelList)
                                    {
                                        if (channel.id == frequency && channel.password == password)
                                        {
                                            String message = String.Format(Messages.INF_CHANNEL_CONNECTED, channel.id);
                                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RADIO, channel.id);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                                            return;
                                        }
                                    }

                                    // Couldn't find any channel with that id
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CHANNEL_NOT_FOUND);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_FREQUENCY_CONNECT_COMMAND);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_FREQUENCY_CONNECT_COMMAND);
                            }
                            break;
                        case Commands.ARGUMENT_DISCONNECT:
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RADIO, 0);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CHANNEL_DISCONNECTED);
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_FREQUENCY_COMMAND);
                            break;
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_WALKIE_IN_HAND);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_EMPTY);
            }
        }

        [Command(Commands.COMMAND_RECRUIT, Messages.GEN_RECRUIT_COMMAND)]
        public void RecruitCommand(Client player, String targetString)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction > Constants.FACTION_NONE)
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
                else if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) > 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_FACTION);
                }
                else
                {
                    int rank = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK);

                    switch (faction)
                    {
                        case Constants.FACTION_POLICE:
                            if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_JOB);
                            }
                            else if (rank < 6)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RECRUIT);
                            }
                            else
                            {
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, Messages.GEN_FACTION_LSPD);

                                // We get the player into the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_POLICE);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_EMERGENCY:
                            if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_JOB);
                            }
                            else if (rank < 10)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RECRUIT);
                            }
                            else
                            {
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, Messages.GEN_FACTION_EMS);

                                // We get the player into the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_EMERGENCY);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_NEWS:
                            if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_JOB);
                            }
                            else if (rank < 5)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RECRUIT);
                            }
                            else
                            {
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, Messages.GEN_FACTION_NEWS);

                                // We get the player into the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_NEWS);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_TOWNHALL:
                            if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_JOB);
                            }
                            else if (rank < 3)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RECRUIT);
                            }
                            else
                            {
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, Messages.GEN_FACTION_TOWNHALL);

                                // We get the player into the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_TOWNHALL);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_ALREADY_JOB);
                            }
                            else if (rank < 5)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RECRUIT);
                            }
                            else
                            {
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, Messages.GEN_FACTION_TRANSPORT);

                                // We get the player into the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_TAXI_DRIVER);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        default:
                            if (rank < 6)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RECRUIT);
                            }
                            else
                            {
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, faction);

                                // We get the player into the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, faction);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                    }

                    // We send the message to the recruiter
                    String playerMessage = String.Format(Messages.INF_PLAYER_RECRUITED, target.Name);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FACTION);
            }
        }

        [Command(Commands.COMMAND_DISMISS, Messages.GEN_DISMISS_COMMAND)]
        public void DismissCommand(Client player, String targetString)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction != Constants.FACTION_NONE)
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
                else if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) != faction)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_SAME_FACTION);
                }
                else
                {
                    int rank = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK);

                    switch (faction)
                    {
                        case Constants.FACTION_POLICE:
                            if (rank < 6)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_DISMISS);
                            }
                            else
                            {
                                // We kick the player from the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, 0);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_EMERGENCY:
                            if (rank < 10)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_DISMISS);
                            }
                            else
                            {
                                // We kick the player from the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, 0);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_NEWS:
                            if (rank < 5)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_DISMISS);
                            }
                            else
                            {
                                // We kick the player from the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, 0);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_TOWNHALL:
                            if (rank < 3)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_DISMISS);
                            }
                            else
                            {
                                // We kick the player from the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, 0);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            if (rank < 5)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_DISMISS);
                            }
                            else
                            {
                                // We kick the player from the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, 0);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        default:
                            if (rank < 6)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_DISMISS);
                            }
                            else
                            {
                                // We kick the player from the faction
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, 0);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 0);
                            }
                            break;
                    }

                    String playerMessage = String.Format(Messages.INF_PLAYER_DISMISSED, target.Name);
                    String targetMessage = String.Format(Messages.INF_FACTION_DISMISSED, player.Name);

                    // Send the messages to both players
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FACTION);
            }
        }

        [Command(Commands.COMMAND_RANK, Messages.GEN_RANK_COMMAND)]
        public void RangoCommand(Client player, String arguments)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction != Constants.FACTION_NONE)
            {
                String[] args = arguments.Split(' ');

                // Get the target player
                Client target = Int32.TryParse(args[0], out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(args[0] + " " + args[1]);

                if (target == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
                else if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) != faction)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_SAME_FACTION);
                }
                else
                {
                    int rank = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK);
                    int givenRank = args.Length > 2 ? Int32.Parse(args[2]) : Int32.Parse(args[1]);

                    switch (faction)
                    {
                        case Constants.FACTION_POLICE:
                            if (rank < 6)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RANK);
                            }
                            else
                            {
                                // Change player's rank
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_EMERGENCY:
                            if (rank < 10)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RANK);
                            }
                            else
                            {
                                // Change player's rank
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_NEWS:
                            if (rank < 5)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RANK);
                            }
                            else
                            {
                                // Change player's rank
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_TOWNHALL:
                            if (rank < 3)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RANK);
                            }
                            else
                            {
                                // Change player's rank
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            if (rank < 5)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RANK);
                            }
                            else
                            {
                                // Change player's rank
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        default:
                            if (rank < 6)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RANK_TOO_LOW_RANK);
                            }
                            else
                            {
                                // Change player's rank
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                    }

                    String playerMessage = String.Format(Messages.INF_PLAYER_RANK_CHANGED, target.Name, givenRank);
                    String targetMessage = String.Format(Messages.INF_FACTION_RANK_CHANGED, player.Name, givenRank);

                    // Send the message to both players
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FACTION);
            }
        }

        [Command(Commands.COMMAND_REPORTS)]
        public void ReportsCommand(Client player)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                int currentElement = 0;
                int totalWarnings = 0;

                // Reports' header
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.GEN_REPORTS_HEADER);

                foreach (FactionWarningModel factionWarning in factionWarningList)
                {
                    if (factionWarning.faction == faction)
                    {
                        String message = String.Empty;
                        if (factionWarning.place.Length > 0)
                        {
                            message = currentElement + ". " + Messages.GEN_TIME + factionWarning.hour + ", " + Messages.GEN_PLACE + factionWarning.place;
                        }
                        else
                        {
                            message = currentElement + ". " + Messages.GEN_TIME + factionWarning.hour;
                        }

                        // Check if attended
                        if (factionWarning.takenBy > -1)
                        {
                            Client target = Globals.GetPlayerById(factionWarning.takenBy);
                            message += ", " + Messages.GEN_ATTENDED_BY + target.Name;
                        }
                        else
                        {
                            message += ", " + Messages.GEN_UNATTENDED;
                        }

                        // We send the message to the player
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + message);
                        
                        totalWarnings++;
                    }
                    
                    currentElement++;
                }
                
                if (totalWarnings == 0)
                {
                    // There are no reports in the list
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_NOT_FACTION_WARNING);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_EMERGENCY_FACTION);
            }
        }

        [Command(Commands.COMMAND_ATTEND, Messages.GEN_ATTEND_COMMAND)]
        public void AttendCommand(Client player, int warning)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                try
                {
                    FactionWarningModel factionWarning = factionWarningList.ElementAt(warning);

                    // Check the faction and whether the report is attended
                    if (factionWarning.faction != faction)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_NOT_FOUND);
                    }
                    else if (factionWarning.takenBy > -1)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_TAKEN);
                    }
                    else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_FACTION_WARNING) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAVE_FACTION_WARNING);
                    }
                    else
                    {
                        Checkpoint factionWarningCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, factionWarning.position, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_FACTION_WARNING, factionWarningCheckpoint);
                        factionWarning.takenBy = player.Value;

                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_FACTION_WARNING_TAKEN);

                        NAPI.ClientEvent.TriggerClientEvent(player, "showFactionWarning", factionWarning.position);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_NOT_FOUND);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_EMERGENCY_FACTION);
            }
        }

        [Command(Commands.COMMAND_CLEAR_REPORTS, Messages.GEN_CLEAR_REPORTS_COMMAND)]
        public void BorraravisoCommand(Client player, int warning)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                try
                {
                    FactionWarningModel factionWarning = factionWarningList.ElementAt(warning);

                    // Check the faction and whether the report is attended
                    if (factionWarning.faction != faction)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_NOT_FOUND);
                    }
                    else
                    {
                        // We remove the report
                        factionWarningList.Remove(factionWarning);

                        // Send the message to the user
                        String message = String.Format(Messages.INF_FACTION_WARNING_DELETED, warning);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_NOT_FOUND);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_EMERGENCY_FACTION);
            }
        }

        [Command(Commands.COMMAND_MEMBERS)]
        public void MiembrosCommand(Client player)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
            if (faction > 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.GEN_MEMBERS_ONLINE);
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == faction)
                    {
                        String rank = GetPlayerFactionRank(target);

                        if (rank == String.Empty)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "[Id: " + player.Value + "] " + target.Name);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "[Id: " + player.Value + "] " + rank + " " + target.Name);
                        }
                    }
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FACTION);
            }
        }
    }
}
