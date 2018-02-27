using GTANetworkAPI;
using WiredPlayers.weazelNews;
using WiredPlayers.globals;
using System.Linq;
using System;

namespace WiredPlayers.chat
{
    public class Chat : Script
    {
        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            // Deleting player's attached label
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_AME) == true)
            {
                TextLabel label = NAPI.Data.GetEntityData(player, EntityData.PLAYER_AME);
                NAPI.Entity.DetachEntity(label);
                NAPI.Entity.DeleteEntity(label);
            }
        }

        public static void SendMessageToNearbyPlayers(Client player, String message, int type, float range, bool excludePlayer = false)
        {
            String secondMessage = String.Empty;
            float distanceGap = range / Constants.CHAT_RANGES;            

            if (message.Length > Constants.CHAT_LENGTH)
            {
                // We need two lines to show the message
                secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
            }

            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && player.Dimension == target.Dimension)
                {
                    if (player != target || (player == target && !excludePlayer))
                    {
                        float distance = player.Position.DistanceTo(target.Position);

                        if (distance <= range)
                        {
                            // Getting message color
                            String chatMessageColor = GetChatMessageColor(distance, distanceGap);
                            String oocMessageColor = GetOocMessageColor(distance, distanceGap);

                            switch (type)
                            {
                                case Constants.MESSAGE_TALK:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + Messages.GEN_CHAT_SAY + message + "..." : chatMessageColor + player.Name + Messages.GEN_CHAT_SAY + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_YELL:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + Messages.GEN_CHAT_YELL + message + "..." : chatMessageColor + player.Name + Messages.GEN_CHAT_YELL + message + "!");
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage + "!");
                                    }
                                    break;
                                case Constants.MESSAGE_WHISPER:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + Messages.GEN_CHAT_WHISPER + message + "..." : chatMessageColor + player.Name + Messages.GEN_CHAT_WHISPER + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_PHONE:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + Messages.GEN_CHAT_PHONE + message + "..." : chatMessageColor + player.Name + Messages.GEN_CHAT_PHONE + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_RADIO:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + Messages.GEN_CHAT_RADIO + message + "..." : chatMessageColor + player.Name + Messages.GEN_CHAT_RADIO + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_ME:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_ME + player.Name + " " + message + "..." : Constants.COLOR_CHAT_ME + player.Name + " " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_ME + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_DO:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_DO + "[ID: " + player.Value + "] " + message + "..." : Constants.COLOR_CHAT_DO + "[ID: " + player.Value + "] " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_DO + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_OOC:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "..." : oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, oocMessageColor + secondMessage + "))");
                                    }
                                    break;
                                case Constants.MESSAGE_DISCONNECT:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_MEGAPHONE:
                                    // We send the message
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_INFO + "[Megáfono de " + player.Name + "]: " + message + "..." : Constants.COLOR_INFO + "[Megáfono de " + player.Name + "]: " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_SU_TRUE:
                                    // We send the message
                                    message = String.Format(Messages.SUC_POSSITIVE_RESULT, player.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SU_POSITIVE + message);
                                    break;
                                case Constants.MESSAGE_SU_FALSE:
                                    // We send the message
                                    message = String.Format(Messages.ERR_NEGATIVE_RESULT, player.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ERROR + message);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        
        private static String GetChatMessageColor(float distance, float distanceGap)
        {
            String color = null;
            if (distance < distanceGap)
            {
                color = Constants.COLOR_CHAT_CLOSE;
            }
            else if (distance < distanceGap * 2)
            {
                color = Constants.COLOR_CHAT_NEAR;
            }
            else if (distance < distanceGap * 3)
            {
                color = Constants.COLOR_CHAT_MEDIUM;
            }
            else if (distance < distanceGap * 4)
            {
                color = Constants.COLOR_CHAT_FAR;
            }
            else
            {
                color = Constants.COLOR_CHAT_LIMIT;
            }
            return color;
        }
        
        private static String GetOocMessageColor(float distance, float distanceGap)
        {
            String color = null;
            if (distance < distanceGap)
            {
                color = Constants.COLOR_OOC_CLOSE;
            }
            else if (distance < distanceGap * 2)
            {
                color = Constants.COLOR_OOC_NEAR;
            }
            else if (distance < distanceGap * 3)
            {
                color = Constants.COLOR_OOC_MEDIUM;
            }
            else if (distance < distanceGap * 4)
            {
                color = Constants.COLOR_OOC_FAR;
            }
            else
            {
                color = Constants.COLOR_OOC_LIMIT;
            }
            return color;
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnChatMessage(Client player, string message)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_CHAT);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ON_AIR) == true)
            {
                WeazelNews.SendNewsMessage(player, message);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PHONE_TALKING) == true)
            {
                // Target player of the message
                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE_TALKING);
                
                String secondMessage = String.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // We split the message in two lines
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                // We send the message to the player and target
                NAPI.Chat.SendChatMessageToPlayer(player, secondMessage.Length > 0 ? Constants.COLOR_CHAT_PHONE + Messages.GEN_PHONE + player.Name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_CHAT_PHONE + Messages.GEN_PHONE + player.Name + Messages.GEN_CHAT_SAY + message);
                NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_PHONE + Messages.GEN_PHONE + player.Name + Messages.GEN_CHAT_SAY + message + "..." : Constants.COLOR_CHAT_PHONE + Messages.GEN_PHONE + player.Name + Messages.GEN_CHAT_SAY + message);
                if (secondMessage.Length > 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_CHAT_PHONE + secondMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_PHONE + secondMessage);
                }

                // We send the message to nearby players
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_PHONE, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f, true);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_TALK, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                NAPI.Util.ConsoleOutput("[ID:" + player.Value + "]" + player.Name + Messages.GEN_CHAT_SAY + message);
            }
        }

        [Command(Commands.COMMAND_SAY, Messages.GEN_SAY_COMMAND, GreedyArg = true)]
        public void DecirCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_TALK, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
            }
        }

        [Command(Commands.COMMAND_YELL, Messages.GEN_YELL_COMMAND, GreedyArg = true)]
        public void GritarCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_YELL, 45.0f);
            }
        }

        [Command(Commands.COMMAND_WHISPER, Messages.GEN_WHISPER_COMMAND, GreedyArg = true)]
        public void SusurrarCommand(Client player, String message)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_WHISPER, 3.0f);
            }
        }

        [Command(Commands.COMMAND_ME, Messages.GEN_ME_COMMAND, GreedyArg = true)]
        public void MeCommand(Client player, String message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_ME, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 20.0f);
        }

        [Command(Commands.COMMAND_DO, Messages.GEN_DO_COMMAND, GreedyArg = true)]
        public void DoCommand(Client player, String message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_DO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 20.0f);
        }

        [Command(Commands.COMMAND_OOC, Messages.GEN_OOC_COMMAND, GreedyArg = true)]
        public void OocCommand(Client player, String message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_OOC, NAPI.Entity.GetEntityDimension(player) > 0 ? 5.0f : 10.0f);
        }

        [Command(Commands.COMMAND_LUCK)]
        public void SuCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                Random random = new Random();
                int messageType = random.Next(0, 2) > 0 ? Constants.MESSAGE_SU_TRUE : Constants.MESSAGE_SU_FALSE;
                SendMessageToNearbyPlayers(player, String.Empty, messageType, 20.0f);
            }
        }

        [Command(Commands.COMMAND_AME, Messages.GEN_AME_COMMAND, GreedyArg = true)]
        public void AmeCommand(Client player, String message = "")
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_AME) == true)
            {
                // We get player's TextLabel
                TextLabel label = NAPI.Data.GetEntityData(player, EntityData.PLAYER_AME);
                
                if (message.Length > 0)
                {
                    // We update label's text
                    NAPI.TextLabel.SetTextLabelText(label, "*" + message + "*");
                }
                else
                {
                    // Deleting TextLabel
                    NAPI.Entity.DetachEntity(label);
                    NAPI.Entity.DeleteEntity(label);
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_AME);
                }
            }
            else
            {
                TextLabel ameLabel = NAPI.TextLabel.CreateTextLabel("*" + message + "*", new Vector3(0.0f, 0.0f, 0.0f), 50.0f, 0.5f, 4, new Color(201, 90, 0, 255));
                NAPI.Entity.AttachEntityToEntity(ameLabel, player, "SKEL_Head", new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f));
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_AME, ameLabel);
            }
        }

        [Command(Commands.COMMAND_MEGAPHONE, Messages.GEN_MEGAPHONE_COMMAND, GreedyArg = true)]
        public void MegafonoCommand(Client player, String message)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player) == true)
            {
                NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                int vehicleFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);

                if (vehicleFaction == Constants.FACTION_POLICE || vehicleFaction == Constants.FACTION_EMERGENCY)
                {
                    SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_MEGAPHONE, 45.0f);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_MEGAPHONE);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
            }
        }

        [Command(Commands.COMMAND_PM, Messages.GEN_MP_COMMAND, GreedyArg = true)]
        public void MpCommand(Client player, String arguments)
        {
            Client target = null;
            String[] args = arguments.Trim().Split(' ');

            if (Int32.TryParse(args[0], out int targetId) == true)
            {
                // We get the player from the id
                target = Globals.GetPlayerById(targetId);
                args = args.Where(w => w != args[0]).ToArray();
                if (args.Length < 1)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_MP_COMMAND);
                    return;
                }
            }
            else if (args.Length > 2)
            {
                target = NAPI.Player.GetPlayerFromName(args[0] + " " + args[1]);
                args = args.Where(w => w != args[1]).ToArray();
                args = args.Where(w => w != args[0]).ToArray();
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_MP_COMMAND);
                return;
            }
            
            if (target != null && NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_MPS_ONLY_ADMIN);
                }
                else
                {
                    String message = String.Join(" ", args);
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We split the message in two lines
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    // Sending messages to both players
                    NAPI.Chat.SendChatMessageToPlayer(player, secondMessage.Length > 0 ? Constants.COLOR_ADMIN_MP + "((" + Messages.GEN_PM_TO + "[ID: " + target.Value + "] " + target.Name + ": " + message + "..." : Constants.COLOR_ADMIN_MP + "((" + Messages.GEN_PM_TO + "[ID: " + target.Value + "] " + target.Name + ": " + message + "))");
                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_ADMIN_MP + "((" + Messages.GEN_PM_FROM + "[ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_ADMIN_MP + "((" + Messages.GEN_PM_FROM + "[ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                    if (secondMessage.Length > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_MP + secondMessage + "))");
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_MP + secondMessage + "))");
                    }
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
            }
        }
    }
}
