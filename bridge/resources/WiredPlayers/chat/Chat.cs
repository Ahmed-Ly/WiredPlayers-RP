using GTANetworkAPI;
using WiredPlayers.weazelNews;
using WiredPlayers.globals;
using System.Linq;
using System;

namespace WiredPlayers.chat
{
    class Chat : Script
    {
        public Chat()
        {
            Event.OnChatMessage += OnChatMessageHandler;
            Event.OnPlayerDisconnected += OnPlayerDisconnectedHandler;
        }

        private void OnChatMessageHandler(Client player, string message, CancelEventArgs e)
        {
            try
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
                    // Obtenemos el jugador destinatario
                    Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE_TALKING);

                    // Comprobación de la longitud del mensaje
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // El mensaje tiene una longitud de dos líneas
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    // Mandamos el mensaje al jugador y al objetivo
                    NAPI.Chat.SendChatMessageToPlayer(player, secondMessage.Length > 0 ? Constants.COLOR_CHAT_PHONE + "[Teléfono] " + player.Name + " dice: " + message + "..." : Constants.COLOR_CHAT_PHONE + "[Teléfono] " + player.Name + " dice: " + message);
                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_PHONE + "[Teléfono] " + player.Name + " dice: " + message + "..." : Constants.COLOR_CHAT_PHONE + "[Teléfono] " + player.Name + " dice: " + message);
                    if (secondMessage.Length > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_CHAT_PHONE + secondMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_PHONE + secondMessage);
                    }

                    // Mandamos el mensaje a los jugadores cercanos
                    SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_PHONE, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f, true);
                }
                else
                {
                    SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_TALK, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 10.0f);
                    NAPI.Util.ConsoleOutput("[ID:" + player.Value + "]" + player.Name + " dice: " + message);
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION Chat.OnChatMessageHandler] " + ex.Message);
            }
            finally
            {
                e.Cancel = true;
            }
        }

        private void OnPlayerDisconnectedHandler(Client player, byte type, string reason)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                // Quitamos la etiqueta descriptiva
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_AME) == true)
                {
                    TextLabel label = NAPI.Data.GetEntityData(player, EntityData.PLAYER_AME);
                    NAPI.Entity.DetachEntity(label);
                    NAPI.Entity.DeleteEntity(label);
                }

                // Avisamos a los jugadores cercanos de la desconexión
                SendMessageToNearbyPlayers(player, " se ha desconectado. (" + reason + ")", Constants.MESSAGE_DISCONNECT, 10.0f);
            }
        }

        public static void SendMessageToNearbyPlayers(Client player, String message, int type, float range, bool excludePlayer = false)
        {
            // División en rangos de distancia
            float distanceGap = range / Constants.CHAT_RANGES;

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
                if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && player.Dimension == target.Dimension)
                {
                    if (player != target || (player == target && !excludePlayer))
                    {
                        float distance = player.Position.DistanceTo(target.Position);

                        if (distance <= range)
                        {
                            // Obtención del color del mensaje
                            String chatMessageColor = GetChatMessageColor(distance, distanceGap);
                            String oocMessageColor = GetOocMessageColor(distance, distanceGap);

                            switch (type)
                            {
                                case Constants.MESSAGE_TALK:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + " dice: " + message + "..." : chatMessageColor + player.Name + " dice: " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_YELL:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + " grita: ¡" + message + "..." : chatMessageColor + player.Name + " grita: ¡" + message + "!");
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage + "!");
                                    }
                                    break;
                                case Constants.MESSAGE_WHISPER:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + " susurra: " + message + "..." : chatMessageColor + player.Name + " susurra: " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_PHONE:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + " por teléfono: " + message + "..." : chatMessageColor + player.Name + " por teléfono: " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_RADIO:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? chatMessageColor + player.Name + " por radio: " + message + "..." : chatMessageColor + player.Name + " por radio: " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_ME:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_ME + player.Name + " " + message + "..." : Constants.COLOR_CHAT_ME + player.Name + " " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_ME + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_DO:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_CHAT_DO + "[ID: " + player.Value + "] " + message + "..." : Constants.COLOR_CHAT_DO + "[ID: " + player.Value + "] " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_DO + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_OOC:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "..." : oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, oocMessageColor + secondMessage + "))");
                                    }
                                    break;
                                case Constants.MESSAGE_DISCONNECT:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_MEGAPHONE:
                                    // Envío del mensaje
                                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_INFO + "[Megáfono de " + player.Name + "]: " + message + "..." : Constants.COLOR_INFO + "[Megáfono de " + player.Name + "]: " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_SU_TRUE:
                                    // Envío del mensaje
                                    message = String.Format(Messages.SUC_POSSITIVE_RESULT, player.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SU_POSITIVE + message);
                                    break;
                                case Constants.MESSAGE_SU_FALSE:
                                    // Envío del mensaje
                                    message = String.Format(Messages.ERR_NEGATIVE_RESULT, player.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ERROR + message);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // Función para obtener el color de chat en función de la distancia
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

        // Función para obtener el color OOC en función de la distancia
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

        [Command("decir", Messages.GEN_SAY_COMMAND, GreedyArg = true)]
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

        [Command("gritar", Messages.GEN_YELL_COMMAND, Alias = "gr", GreedyArg = true)]
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

        [Command("susurrar", Messages.GEN_WHISPER_COMMAND, Alias = "sus", GreedyArg = true)]
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

        [Command("me", Messages.GEN_ME_COMMAND, GreedyArg = true)]
        public void MeCommand(Client player, String message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_ME, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 20.0f);
        }

        [Command("do", Messages.GEN_DO_COMMAND, GreedyArg = true)]
        public void DoCommand(Client player, String message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_DO, NAPI.Entity.GetEntityDimension(player) > 0 ? 7.5f : 20.0f);
        }

        [Command("ooc", Messages.GEN_OOC_COMMAND, GreedyArg = true)]
        public void OocCommand(Client player, String message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_OOC, NAPI.Entity.GetEntityDimension(player) > 0 ? 5.0f : 10.0f);
        }

        [Command("su")]
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

        [Command("ame", Messages.GEN_AME_COMMAND, GreedyArg = true)]
        public void AmeCommand(Client player, String message = "")
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_AME) == true)
            {
                // Cogemos el label del jugador
                TextLabel label = NAPI.Data.GetEntityData(player, EntityData.PLAYER_AME);

                // Miramos si tenemos que actualizarlo o quitárselo
                if (message.Length > 0)
                {
                    // Actualizamos el texto
                    NAPI.TextLabel.SetTextLabelText(label, "*" + message + "*");
                }
                else
                {
                    // Borramos la etiqueta
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

        [Command("megafono", Messages.GEN_MEGAPHONE_COMMAND, Alias = "m", GreedyArg = true)]
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

        [Command("mp", Messages.GEN_MP_COMMAND, GreedyArg = true)]
        public void MpCommand(Client player, String arguments)
        {
            Client target = null;
            String[] args = arguments.Trim().Split(' ');

            if (Int32.TryParse(args[0], out int targetId) == true)
            {
                // Hemos recibido el id del jugador
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

            // Miramos si el jugador está conectado
            if (target != null && NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_MPS_ONLY_ADMIN);
                }
                else
                {
                    // Unimos el mensaje y lo dividimos si su longitud excede la permitida
                    String message = String.Join(" ", args);
                    String secondMessage = String.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // El mensaje tiene una longitud de dos líneas
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    // Creamos los mensajes a enviar a emisor y receptor
                    NAPI.Chat.SendChatMessageToPlayer(player, secondMessage.Length > 0 ? Constants.COLOR_ADMIN_MP + "((Mensaje privado a [ID: " + target.Value + "] " + target.Name + ": " + message + "..." : Constants.COLOR_ADMIN_MP + "((Mensaje privado a [ID: " + target.Value + "] " + target.Name + ": " + message + "))");
                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_ADMIN_MP + "((Mensaje privado de [ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_ADMIN_MP + "((Mensaje privado de [ID: " + player.Value + "] " + player.Name + ": " + message + "))");
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
