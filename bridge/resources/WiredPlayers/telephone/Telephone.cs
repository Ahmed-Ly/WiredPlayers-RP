using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System;

namespace WiredPlayers.telephone
{
    public class Telephone : Script
    {
        public static List<ContactModel> contactList;

        private ContactModel GetContactFromId(int contactId)
        {
            ContactModel contact = null;
            foreach (ContactModel contactModel in contactList)
            {
                if (contactModel.id == contactId)
                {
                    contact = contactModel;
                    break;
                }
            }
            return contact;
        }

        private int GetNumerFromContactName(String contactName, int playerPhone)
        {
            int targetPhone = 0;
            foreach (ContactModel contact in contactList)
            {
                if (contact.owner == playerPhone && contact.contactName == contactName)
                {
                    targetPhone = contact.contactNumber;
                    break;
                }
            }
            return targetPhone;
        }

        private List<ContactModel> GetTelephoneContactList(int number)
        {
            List<ContactModel> contacts = new List<ContactModel>();
            foreach (ContactModel contact in contactList)
            {
                if (contact.owner == number)
                {
                    contacts.Add(contact);
                }
            }
            return contacts;
        }

        private String GetContactInTelephone(int phone, int number)
        {
            String contactName = String.Empty;
            foreach (ContactModel contact in contactList)
            {
                if (contact.owner == phone && contact.contactNumber == number)
                {
                    contactName = contact.contactName;
                    break;
                }
            }
            return contactName;
        }

        [RemoteEvent("addNewContact")]
        public void AddNewContactEvent(Client player, params object[] arguments)
        {
            // Obtenemos el número y nombre
            int contactNumber = Int32.Parse(arguments[0].ToString());
            String contactName = arguments[1].ToString();

            // Creamos el nuevo modelo
            ContactModel contact = new ContactModel();
            contact.owner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
            contact.contactNumber = contactNumber;
            contact.contactName = contactName;

            // Añadimos el nuevo contacto
            contact.id = Database.AddNewContact(contact);
            contactList.Add(contact);

            // Informamos al jugador
            String actionMessage = String.Format(Messages.INF_CONTACT_CREATED, contactName, contactNumber);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + actionMessage);
        }

        [RemoteEvent("modifyContact")]
        public void ModifyContactEvent(Client player, params object[] arguments)
        {
            // Obtenemos el número, nombre y el identificador
            int contactNumber = Int32.Parse(arguments[0].ToString());
            String contactName = arguments[1].ToString();
            int contactIndex = Int32.Parse(arguments[2].ToString());

            // Modificamos los datos del contacto
            ContactModel contact = GetContactFromId(contactIndex);
            contact.contactNumber = contactNumber;
            contact.contactName = contactName;
            Database.ModifyContact(contact);

            // Informamos al jugador
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CONTACT_MODIFIED);
        }

        [RemoteEvent("deleteContact")]
        public void DeleteContactEvent(Client player, params object[] arguments)
        {
            // Obtenemos el identificador del contacto
            int contactIndex = Int32.Parse(arguments[0].ToString());

            // Obtenemos los datos del contacto
            ContactModel contact = GetContactFromId(contactIndex);
            String contactName = contact.contactName;
            int contactNumber = contact.contactNumber;

            // Eliminamos el contacto
            Database.DeleteContact(contactIndex);
            contactList.Remove(contact);

            // Informamos al jugador
            String actionMessage = String.Format(Messages.INF_CONTACT_DELETED, contactName, contactNumber);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + actionMessage);
        }

        [RemoteEvent("sendPhoneMessage")]
        public void SendPhoneMessageEvent(Client player, params object[] arguments)
        {
            // Capturamos el contacto y el mensaje
            int contactIndex = Int32.Parse(arguments[0].ToString());
            String textMessage = arguments[1].ToString();

            // Obtenemos el contacto
            ContactModel contact = GetContactFromId(contactIndex);

            // Obtenemos el jugador objetivo
            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE) == contact.contactNumber)
                {
                    // Miramos si lo tiene añadido como contacto
                    int phone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);
                    String contactName = GetContactInTelephone(phone, contact.contactNumber);

                    if (contactName.Length == 0)
                    {
                        contactName = contact.contactNumber.ToString();
                    }

                    // Comprobación de la longitud del mensaje
                    String secondMessage = String.Empty;

                    if (textMessage.Length > Constants.CHAT_LENGTH)
                    {
                        // El mensaje tiene una longitud de dos líneas
                        secondMessage = textMessage.Substring(Constants.CHAT_LENGTH, textMessage.Length - Constants.CHAT_LENGTH);
                        textMessage = textMessage.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    // Mandamos el mensaje al jugador objetivo
                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_INFO + "[SMS de " + contactName + "] " + textMessage + "..." : Constants.COLOR_INFO + "[SMS de " + contactName + "] " + textMessage);
                    if (secondMessage.Length > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + secondMessage);
                    }

                    // Avisamos al jugador del envío del mensaje
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_SMS_SENT);

                    // Añadimos el mensaje a la base de datos
                    Database.AddSMSLog(phone, contact.contactNumber, textMessage);
                    return;
                }
            }

            // No hay ningún jugador con ese número de teléfono conectado
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
        }

        [Command("llamar", Messages.GEN_PHONE_CALL_COMMAND)]
        public void LlamarCommand(Client player, String called)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PHONE_TALKING) || NAPI.Data.HasEntityData(player, EntityData.PLAYER_CALLING) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_PHONE_TALKING);
            }
            else
            {
                ItemModel item = Globals.GetItemInEntity(NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
                if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
                {
                    int peopleOnline = 0;

                    if (Int32.TryParse(called, out int number) == true)
                    {
                        // Llamamos a un número
                        switch (number)
                        {
                            case Constants.NUMBER_POLICE:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CENTRAL_CALL);
                                        peopleOnline++;
                                    }
                                }

                                // Miramos si hay alguien del departamento
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_POLICE);

                                    // Avisamos de que está llamando
                                    String playerMessage = String.Format(Messages.INF_CALLING, Constants.NUMBER_POLICE);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LINE_OCCUPIED);
                                }
                                break;
                            case Constants.NUMBER_EMERGENCY:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CENTRAL_CALL);
                                        peopleOnline++;
                                    }
                                }

                                // Miramos si hay alguien del departamento
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_EMERGENCY);

                                    // Avisamos de que está llamando
                                    String playerMessage = String.Format(Messages.INF_CALLING, Constants.NUMBER_EMERGENCY);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LINE_OCCUPIED);
                                }
                                break;
                            case Constants.NUMBER_NEWS:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_NEWS)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CENTRAL_CALL);
                                        peopleOnline++;
                                    }
                                }

                                // Miramos si hay alguien del departamento
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_NEWS);

                                    // Avisamos de que está llamando
                                    String playerMessage = String.Format(Messages.INF_CALLING, Constants.NUMBER_NEWS);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LINE_OCCUPIED);
                                }
                                break;
                            case Constants.NUMBER_TAXI:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_TAXI_DRIVER)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CENTRAL_CALL);
                                        peopleOnline++;
                                    }
                                }

                                // Miramos si hay alguien del departamento
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_TAXI_DRIVER);

                                    // Avisamos de que está llamando
                                    String playerMessage = String.Format(Messages.INF_CALLING, Constants.NUMBER_TAXI);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LINE_OCCUPIED);
                                }
                                break;
                            case Constants.NUMBER_FASTFOOD:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) == Constants.JOB_FASTFOOD)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CENTRAL_CALL);
                                        peopleOnline++;
                                    }
                                }

                                // Miramos si hay alguien del departamento
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.JOB_FASTFOOD + 100);

                                    // Avisamos de que está llamando
                                    String playerMessage = String.Format(Messages.INF_CALLING, Constants.NUMBER_FASTFOOD);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LINE_OCCUPIED);
                                }
                                break;
                            case Constants.NUMBER_MECHANIC:
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_JOB) == Constants.JOB_MECHANIC)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CENTRAL_CALL);
                                        peopleOnline++;
                                    }
                                }

                                // Miramos si hay alguien del departamento
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.JOB_MECHANIC + 100);

                                    // Avisamos de que está llamando
                                    String playerMessage = String.Format(Messages.INF_CALLING, Constants.NUMBER_MECHANIC);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LINE_OCCUPIED);
                                }
                                break;
                            default:
                                // Comprobamos que el número existe
                                if (number > 0)
                                {
                                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                                    {
                                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE) == number)
                                        {
                                            int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);

                                            // Miramos si el jugador objetivo lo tiene añadido como contacto
                                            int phone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);
                                            String contact = GetContactInTelephone(phone, playerPhone);
                                            if (contact.Length == 0)
                                            {
                                                contact = playerPhone.ToString();
                                            }
                                            // Miramos si está como contacto en la agenda del receptor
                                            String playerMessage = String.Format(Messages.INF_CALLING, number);
                                            String targetMessage = String.Format(Messages.INF_CALL_FROM, contact.Length > 0 ? contact : contact.ToString());

                                            // Avisamos de la llamada
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                                            // Marcamos a la persona como que está llamando
                                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, target);
                                            return;
                                        }
                                    }
                                }

                                // No hay nadie con ese número de teléfono
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
                                break;
                        }
                    }
                    else
                    {
                        // Llamamos a un contacto
                        int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                        int targetPhone = GetNumerFromContactName(called, playerPhone);

                        // Comprobamos que el número existe
                        if (targetPhone > 0)
                        {
                            foreach (Client target in NAPI.Pools.GetAllPlayers())
                            {
                                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE) == targetPhone)
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_CALLING) || NAPI.Data.HasEntityData(target, EntityData.PLAYER_PHONE_TALKING) || NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == false)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
                                    }
                                    else
                                    {
                                        // Miramos si está como contacto en la agenda del receptor
                                        String contact = GetContactInTelephone(NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE), playerPhone);
                                        String targetMessage = String.Format(Messages.INF_CALL_FROM, contact.Length > 0 ? contact : playerPhone.ToString());
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_INCOMING_CALL);

                                        // Mandamos el aviso al jugador
                                        String playerMessage = String.Format(Messages.INF_CALLING, called);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, target);
                                    }
                                    return;
                                }
                            }
                        }

                        // No hay ningún jugador con ese número de teléfono conectado
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_TELEPHONE_HAND);
                }
            }
        }

        [Command("contestar")]
        public void ContestarCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_CALLING) || NAPI.Data.HasEntityData(player, EntityData.PLAYER_PHONE_TALKING) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_PHONE_TALKING);
            }
            else
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    // Comprobamos si el jugador está llamando
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_CALLING) == true)
                    {
                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_CALLING) is int)
                        {
                            int factionJob = NAPI.Data.GetEntityData(target, EntityData.PLAYER_CALLING);
                            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                            int job = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);

                            if (factionJob == faction || factionJob == job + 100)
                            {
                                // Relacionamos a los dos jugadores
                                NAPI.Data.ResetEntityData(target, EntityData.PLAYER_CALLING);
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE_TALKING, target);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_TALKING, player);

                                // Avisamos de que la llamada se efectua
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CALL_RECEIVED);
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CALL_TAKEN);

                                // Guardamos la hora de comienzo
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_CALL_STARTED, Globals.GetTotalSeconds());
                                return;
                            }
                        }
                        else if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_CALLING) == player)
                        {
                            // Relacionamos a los dos jugadores
                            NAPI.Data.ResetEntityData(target, EntityData.PLAYER_CALLING);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE_TALKING, target);
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_TALKING, player);

                            // Avisamos de que la llamada se efectua
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CALL_RECEIVED);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CALL_TAKEN);

                            // Guardamos la hora de comienzo
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_CALL_STARTED, Globals.GetTotalSeconds());
                            return;
                        }
                    }
                }

                // Nadie está llamando al jugador
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_CALLED);
            }
        }

        [Command("colgar")]
        public void ColgarCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_CALLING) == true)
            {
                // Colgamos la llamada
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_CALLING);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PHONE_TALKING) == true)
            {
                // Recuperamos el jugador con el que habla y los números de teléfono
                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE_TALKING);
                int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                int targetPhone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);

                // Obtenemos la duración de la llamada
                int elapsed = 0;
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PHONE_CALL_STARTED) == true)
                {
                    elapsed = Globals.GetTotalSeconds() - NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE_CALL_STARTED);
                    Database.AddCallLog(playerPhone, targetPhone, elapsed);
                }
                else
                {
                    elapsed = Globals.GetTotalSeconds() - NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE_CALL_STARTED);
                    Database.AddCallLog(targetPhone, playerPhone, elapsed);
                }

                // Colgamos la llamada para ambos jugadores
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_PHONE_TALKING);
                NAPI.Data.ResetEntityData(target, EntityData.PLAYER_PHONE_TALKING);
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_PHONE_CALL_STARTED);

                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_FINISHED_CALL);
                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_FINISHED_CALL);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_PHONE_TALKING);
            }
        }

        [Command("sms", Messages.GEN_SMS_COMMAND, GreedyArg = true)]
        public void SmsCommand(Client player, int number, String message)
        {
            ItemModel item = Globals.GetItemInEntity(NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
            if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (number > 0 && NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE) == number)
                    {
                        // Obtenemos el numero del jugador que manda el mensaje
                        int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);

                        // Miramos si el jugador objetivo lo tiene añadido como contacto
                        int phone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);
                        String contact = GetContactInTelephone(phone, playerPhone);
                        if (contact.Length == 0)
                        {
                            contact = playerPhone.ToString();
                        }

                        // Comprobación de la longitud del mensaje
                        String secondMessage = String.Empty;

                        if (message.Length > Constants.CHAT_LENGTH)
                        {
                            // El mensaje tiene una longitud de dos líneas
                            secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                            message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                        }

                        // Mandamos el mensaje al jugador objetivo
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_INFO + "[SMS de " + playerPhone + "] " + message + "..." : Constants.COLOR_INFO + "[SMS de " + playerPhone + "] " + message);
                        if (secondMessage.Length > 0)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + secondMessage);
                        }

                        foreach (Client targetPlayer in NAPI.Pools.GetAllPlayers())
                        {
                            if (targetPlayer.Position.DistanceTo(player.Position) < 20.0f)
                            {
                                String nearMessage = String.Format(Messages.INF_PLAYER_TEXTING, player.Name);
                                NAPI.Chat.SendChatMessageToPlayer(targetPlayer, Constants.COLOR_CHAT_ME + nearMessage);
                            }
                        }

                        // Añadimos el mensaje a la base de datos
                        Database.AddSMSLog(playerPhone, number, message);

                        return;
                    }
                }

                // No hay ningún jugador con ese número de teléfono conectado
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_TELEPHONE_HAND);
            }
        }

        [Command("agenda", Messages.GEN_CONTACTS_COMMAND)]
        public void AgendaCommand(Client player, String action)
        {
            ItemModel item = Globals.GetItemInEntity(NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
            if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                // Obtenemos la lista de contactos
                int phoneNumber = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                List<ContactModel> contacts = GetTelephoneContactList(phoneNumber);
                switch (action.ToLower())
                {
                    case "numero":
                        String message = String.Format(Messages.INF_PHONE_NUMBER, phoneNumber);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                        break;
                    case "ver":
                        if (contacts.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_LOAD);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CONTACT_LIST_EMPTY);
                        }
                        break;
                    case "añadir":
                        NAPI.ClientEvent.TriggerClientEvent(player, "addContactWindow", Constants.ACTION_ADD);
                        break;
                    case "modificar":
                        if (contacts.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_RENAME);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CONTACT_LIST_EMPTY);
                        }
                        break;
                    case "borrar":
                        if (contacts.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_DELETE);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CONTACT_LIST_EMPTY);
                        }
                        break;
                    case "sms":
                        if (contacts.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_SMS);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CONTACT_LIST_EMPTY);
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_CONTACTS_COMMAND);
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_TELEPHONE_HAND);
            }
        }
    }
}