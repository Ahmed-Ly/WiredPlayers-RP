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

        public Faction()
        {
            Event.OnResourceStart += OnResourceStartHandler;
            Event.OnPlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
        }

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
                if(factionWarn.playerId == playerId && factionWarn.faction == faction)
                {
                    warn = factionWarn;
                    break;
                }
            }
            return warn;
        }

        private void OnResourceStartHandler()
        {
            factionWarningList = new List<FactionWarningModel>();
        }

        private void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_FACTION_WARNING) == true)
            {
                // Sacamos el identificador jugador
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);

                // Borramos el checkpoint
                Checkpoint locationCheckpoint = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION_WARNING);
                NAPI.Entity.DeleteEntity(locationCheckpoint);

                // Borramos las marcas
                NAPI.ClientEvent.TriggerClientEvent(player, "deleteFactionWarning");

                // Reseteamos la variable de localización
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_FACTION_WARNING);

                // Borramos el aviso
                factionWarningList.RemoveAll(x => x.takenBy == playerId);
            }
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

        [Command("f", Messages.GEN_F_COMMAND, GreedyArg = true)]
        public void FCommand(Client player, String message)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
            if (faction > 0 && faction < Constants.LAST_STATE_FACTION)
            {
                String rank = GetPlayerFactionRank(player);
                String name = NAPI.Data.GetEntityData(player, EntityData.PLAYER_NAME);
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);

                // Comprobación de la longitud del mensaje
                String secondMessage = String.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // El mensaje tiene una longitud de dos líneas
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == faction)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_FACTION + "(([ID: " + playerId + "] " + rank + " " + name + ": " + message + "..." : Constants.COLOR_CHAT_FACTION + "(([ID: " + playerId + "] " + rank + " " + name + ": " + message + "))");
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

        private bool CheckInternalAffairs(int faction, Client target)
        {
            bool isInternalAffairs = false;

            if (faction == Constants.FACTION_TOWNHALL && (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && NAPI.Data.GetEntityData(target, EntityData.PLAYER_RANK) == 7))
                {
                    isInternalAffairs = true;
                }

            return isInternalAffairs;
        }

        [Command("r", Messages.GEN_R_COMMAND, GreedyArg = true)]
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
                    // Obtenemos el rango
                    String rank = GetPlayerFactionRank(player);

                    // Comprobación de la longitud del mensaje
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // El mensaje tiene una longitud de dos líneas
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == faction || CheckInternalAffairs(faction, target) == true))
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + "[RADIO] " + rank + " " + player.Name + " dice: " + message + "..." : Constants.COLOR_RADIO + "[RADIO] " + rank + " " + player.Name + " dice: " + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                                // TODO Mandar a los que estén cerca de quien recibe el mensaje si este no lleva cascos
                            }
                        }
                    }

                    // Mandar a jugadores cercanos al que manda mensaje de radio
                    Chat.DendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                    
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_STATE_FACTION);
                }
            }
        }

        [Command("dp", Messages.GEN_DP_COMMAND, GreedyArg = true)]
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

                    // Comprobación de la longitud del mensaje
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // El mensaje tiene una longitud de dos líneas
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + "[RADIO] " + rank + " " + player.Name + " dice: " + message + "..." : Constants.COLOR_RADIO + "[RADIO] " + rank + " " + player.Name + " dice: " + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                                // TODO Mandar a los que estén cerca de quien recibe el mensaje si este no lleva cascos
                            }
                        }
                        else if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO_POLICE + "[RADIO] " + rank + " " + player.Name + " dice: " + message + "..." : Constants.COLOR_RADIO_POLICE + "[RADIO] " + rank + " " + player.Name + " dice: " + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO_POLICE + secondMessage);
                                // TODO Mandar a los que estén cerca de quien recibe el mensaje si este no lleva cascos
                            }
                        }
                    }

                    // Mandar a jugadores cercanos al que manda mensaje de radio.
                    Chat.DendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_EMERGENCY_FACTION);
                }
            }
        }

        [Command("de", Messages.GEN_DE_COMMAND, GreedyArg = true)]
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

                    // Comprobación de la longitud del mensaje
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // El mensaje tiene una longitud de dos líneas
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target,secondMessage.Length > 0 ? Constants.COLOR_RADIO_POLICE + "[RADIO] " + rank + " " + player.Name + " dice: " + message + "..." : Constants.COLOR_RADIO_POLICE + "[RADIO] " + rank + " " + player.Name + " dice: " + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO_POLICE + secondMessage);
                                // TODO Mandar a los que estén cerca de quien recibe el mensaje si este no lleva cascos
                            }
                        }
                        else if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + "[RADIO] " + rank + " " + player.Name + " dice: " + message + "..." : Constants.COLOR_RADIO + "[RADIO] " + rank + " " + player.Name + " dice: " + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                                // TODO Mandar a los que estén cerca de quien recibe el mensaje si este no lleva cascos
                            }
                        }
                    }

                    // Mandar a jugadores cercanos al que manda mensaje de radio
                    Chat.DendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_EMERGENCY_FACTION);
                }
            }
        }

        [Command("fr", Messages.GEN_FR_COMMAND, GreedyArg = true)]
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

                    // Comprobación de la longitud del mensaje
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // El mensaje tiene una longitud de dos líneas
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_RADIO) == radio)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_RADIO + "[RADIO] " + name + " dice: " + message + "..." : Constants.COLOR_RADIO + "[RADIO] " + name + " dice: " + message);
                            if (secondMessage.Length > 0)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_RADIO + secondMessage);
                                // TODO Mandar a los que estén cerca de quien recibe el mensaje si este no lleva cascos
                            }
                        }
                    }

                    // Mandar a jugadores cercanos al que manda mensaje de radio
                    Chat.DendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RADIO_FREQUENCY_NONE);
                }
            }
        }

        [Command("frecuencia", Messages.GEN_FREQUENCY_COMMAND, GreedyArg = true)]
        public void FrecuenciaCommand(Client player, String args)
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
                        case "crear":
                            if (arguments.Length == 2)
                            {
                                if (ownedChannel == null)
                                {
                                    // Creamos la frecuencia
                                    MD5 md5Hash = MD5.Create();
                                    ChannelModel channel = new ChannelModel();
                                    channel.owner = playerId;
                                    channel.password = GetMd5Hash(md5Hash, arguments[1]);
                                    channel.id = Database.AddChannel(channel);
                                    channelList.Add(channel);

                                    // Mandamos el mensaje con el identificador
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
                        case "modificar":
                            if (arguments.Length == 2)
                            {
                                if (ownedChannel != null)
                                {
                                    // Creamos la frecuencia
                                    MD5 md5Hash = MD5.Create();
                                    ownedChannel.password = GetMd5Hash(md5Hash, arguments[1]);
                                    Database.UpdateChannel(ownedChannel);

                                    // Echamos a todos de la frecuencia
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

                                    // Mandamos el mensaje con el identificador
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
                        case "eliminar":
                            if (ownedChannel != null)
                            {
                                // Echamos a todos de la frecuencia
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

                                // Eliminamos el canal
                                Database.RemoveChannel(ownedChannel.id);
                                channelList.Remove(ownedChannel);

                                // Mandamos el mensaje con el identificador
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CHANNEL_DELETED);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_OWNED_CHANNEL);
                            }
                            break;
                        case "conectar":
                            if (arguments.Length == 3)
                            {
                                if (Int32.TryParse(arguments[1], out int frequency) == true)
                                {
                                    // Ciframos la contraseña
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

                                    // No se ha encontrado ninguna frecuencia coincidente
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
                        case "desconectar":
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

        [Command("reclutar", Messages.GEN_RECRUIT_COMMAND)]
        public void ReclutarCommand(Client player, String targetString)
        {
            // Sacamos la facción del jugador
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
                    // Miramos el rango
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
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, "LSPD");

                                // Metemos al jugador a la facción
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_POLICE);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Mandamos el mensaje al jugador
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
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, "EMS");

                                // Metemos al jugador a la facción
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_EMERGENCY);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Mandamos el mensaje al jugador
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
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, "Weazel News");

                                // Metemos al jugador a la facción
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_NEWS);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Mandamos el mensaje al jugador
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
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, "Ayuntamiento");

                                // Metemos al jugador a la facción
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_TOWNHALL);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Mandamos el mensaje al jugador
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
                                String targetMessage = String.Format(Messages.INF_FACTION_RECRUITED, "Servicio de transportes");

                                // Metemos al jugador a la facción
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, Constants.FACTION_TAXI_DRIVER);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Mandamos el mensaje al jugador
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

                                // Metemos al jugador a la facción
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, faction);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 1);

                                // Mandamos el mensaje al jugador
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                    }

                    // Mandamos el mensaje al reclutador
                    String playerMessage = String.Format(Messages.INF_PLAYER_RECRUITED, target.Name);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FACTION);
            }
        }

        [Command("expulsar", Messages.GEN_DISMISS_COMMAND)]
        public void ExpulsarCommand(Client player, String targetString)
        {
            // Sacamos la facción del jugador
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
                    // Miramos el rango
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
                                // Sacamos al jugador de la facción
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
                                // Sacamos al jugador de la facción
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
                                // Sacamos al jugador de la facción
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
                                // Sacamos al jugador de la facción
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
                                // Sacamos al jugador de la facción
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
                                // Sacamos al jugador de la facción
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_FACTION, 0);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, 0);
                            }
                            break;
                    }

                    String playerMessage = String.Format(Messages.INF_PLAYER_DISMISSED, target.Name);
                    String targetMessage = String.Format(Messages.INF_FACTION_DISMISSED, player.Name);

                    // Mandamos el mensaje a los jugadores
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FACTION);
            }
        }

        [Command("rango", Messages.GEN_RANK_COMMAND)]
        public void RangoCommand(Client player, String arguments)
        {
            // Sacamos la facción del jugador
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction != Constants.FACTION_NONE)
            {
                String[] args = arguments.Split(' ');

                // Sacamos el objetivo
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
                    // Miramos el rango
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
                                // Sacamos cambiamos el rango del jugador
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
                                // Sacamos cambiamos el rango del jugador
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
                                // Sacamos cambiamos el rango del jugador
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
                                // Sacamos cambiamos el rango del jugador
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
                                // Sacamos cambiamos el rango del jugador
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
                                // Sacamos cambiamos el rango del jugador
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                    }

                    String playerMessage = String.Format(Messages.INF_PLAYER_RANK_CHANGED, target.Name, givenRank);
                    String targetMessage = String.Format(Messages.INF_FACTION_RANK_CHANGED, player.Name, givenRank);

                    // Mandamos el mensaje a los jugadores
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NO_FACTION);
            }
        }

        [Command("avisos")]
        public void AvisosCommand(Client player)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if(faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                // Inicializamos los contadores
                int currentElement = 0;
                int totalWarnings = 0;

                // Mandamos la cabecera de avisos
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Lista de avisos");

                foreach (FactionWarningModel factionWarning in factionWarningList)
                {
                    if(factionWarning.faction == faction)
                    {
                        String message = String.Empty;
                        if(factionWarning.place.Length > 0)
                        {
                            message = currentElement + ". Hora: " + factionWarning.hour + ", lugar: " + factionWarning.place;
                        }
                        else
                        {
                            message = currentElement + ". Hora: " + factionWarning.hour;
                        }

                        // Miramos si ha sido atendido
                        if(factionWarning.takenBy > -1)
                        {
                            Client target = Globals.GetPlayerById(factionWarning.takenBy);
                            message += ", estado: atendido por " + target.Name;
                        }
                        else
                        {
                            message += ", estado: sin atender";
                        }

                        // Mandamos el mensaje
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + message);

                        // Incrementamos el contador
                        totalWarnings++;
                    }

                    // Incrementamos el contador
                    currentElement++;
                }

                // Si no hay avisos informamos de ello
                if (totalWarnings == 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_NOT_FACTION_WARNING);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_EMERGENCY_FACTION);
            }
        }
    
        [Command("atender", Messages.GEN_ATENDER_COMMAND)]
        public void AtenderCommand(Client player, int warning)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                try
                {
                    FactionWarningModel factionWarning = factionWarningList.ElementAt(warning);

                    // Comprobamos si está en la facción correcta y si está cogido el aviso
                    if(factionWarning.faction != faction)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_NOT_FOUND);
                    }
                    else if(factionWarning.takenBy > -1)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_TAKEN);
                    }
                    else if(NAPI.Data.HasEntityData(player, EntityData.PLAYER_FACTION_WARNING) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HAVE_FACTION_WARNING);
                    }
                    else
                    {
                        Checkpoint factionWarningCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, factionWarning.position, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_FACTION_WARNING, factionWarningCheckpoint);
                        factionWarning.takenBy = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                        NAPI.ClientEvent.TriggerClientEvent(player, "showFactionWarning", factionWarning.position);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_FACTION_WARNING_TAKEN);
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

        [Command("borraraviso", Messages.GEN_BORRAR_AVISO_COMMAND)]
        public void BorraravisoCommand(Client player, int warning)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                try
                {
                    FactionWarningModel factionWarning = factionWarningList.ElementAt(warning);

                    // Comprobamos si está en la facción correcta y si está cogido el aviso
                    if (factionWarning.faction != faction)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_FACTION_WARNING_NOT_FOUND);
                    }
                    else
                    {
                        // Borramos el aviso
                        factionWarningList.Remove(factionWarning);

                        // Mandamos el mensaje al usuario
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

        [Command("miembros")]
        public void MiembrosCommand(Client player)
        {
            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
            if(faction > 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Miembros conectados:");
                foreach(Client target in NAPI.Pools.GetAllPlayers())
                {
                    if(NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == faction)
                    {
                        // Obtenemos las variables del personaje
                        String rank = GetPlayerFactionRank(target);
                        int playerId = NAPI.Data.GetEntityData(target, EntityData.PLAYER_ID);

                        if(rank == String.Empty)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "[Id: " + playerId + "] " + target.Name);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "[Id: " + playerId + "] " + rank + " " + target.Name);
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
