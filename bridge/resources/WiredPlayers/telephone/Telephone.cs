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
        public void AddNewContactEvent(Client player, int contactNumber, String contactName)
        {
            // Create the model for the new contact
            ContactModel contact = new ContactModel();
            contact.owner = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
            contact.contactNumber = contactNumber;
            contact.contactName = contactName;

            // Add contact to database
            contact.id = Database.AddNewContact(contact);
            contactList.Add(contact);
            
            String actionMessage = String.Format(Messages.INF_CONTACT_CREATED, contactName, contactNumber);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + actionMessage);
        }

        [RemoteEvent("modifyContact")]
        public void ModifyContactEvent(Client player, int contactNumber, String contactName, int contactIndex)
        {
            // Modify contact data
            ContactModel contact = GetContactFromId(contactIndex);
            contact.contactNumber = contactNumber;
            contact.contactName = contactName;
            Database.ModifyContact(contact);
            
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CONTACT_MODIFIED);
        }

        [RemoteEvent("deleteContact")]
        public void DeleteContactEvent(Client player, int contactIndex)
        {
            ContactModel contact = GetContactFromId(contactIndex);
            String contactName = contact.contactName;
            int contactNumber = contact.contactNumber;

            // Delete the contact
            Database.DeleteContact(contactIndex);
            contactList.Remove(contact);
            
            String actionMessage = String.Format(Messages.INF_CONTACT_DELETED, contactName, contactNumber);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + actionMessage);
        }

        [RemoteEvent("sendPhoneMessage")]
        public void SendPhoneMessageEvent(Client player, int contactIndex, String textMessage)
        {
            ContactModel contact = GetContactFromId(contactIndex);
            
            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE) == contact.contactNumber)
                {
                    // Check player's number
                    int phone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);
                    String contactName = GetContactInTelephone(phone, contact.contactNumber);

                    if (contactName.Length == 0)
                    {
                        contactName = contact.contactNumber.ToString();
                    }
                    
                    String secondMessage = String.Empty;

                    if (textMessage.Length > Constants.CHAT_LENGTH)
                    {
                        // We need to lines to print the message
                        secondMessage = textMessage.Substring(Constants.CHAT_LENGTH, textMessage.Length - Constants.CHAT_LENGTH);
                        textMessage = textMessage.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    // Send the message to the target
                    NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_INFO + "[" + Messages.GEN_SMS_FROM + contactName + "] " + textMessage + "..." : Constants.COLOR_INFO + "[" + Messages.GEN_SMS_FROM + contactName + "] " + textMessage);
                    if (secondMessage.Length > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + secondMessage);
                    }
                    
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_SMS_SENT);

                    Database.AddSMSLog(phone, contact.contactNumber, textMessage);

                    return;
                }
            }

            // There's no player matching the contact
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
        }

        [Command(Commands.COMMAND_CALL, Messages.GEN_PHONE_CALL_COMMAND)]
        public void CallCommand(Client player, String called)
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
                                
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_POLICE);
                                    
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
                                
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_EMERGENCY);
                                    
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
                                
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_NEWS);
                                    
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
                                
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.FACTION_TAXI_DRIVER);
                                    
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
                                
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.JOB_FASTFOOD + 100);
                                    
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
                                
                                if (peopleOnline > 0)
                                {
                                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, Constants.JOB_MECHANIC + 100);
                                    
                                    String playerMessage = String.Format(Messages.INF_CALLING, Constants.NUMBER_MECHANIC);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_LINE_OCCUPIED);
                                }
                                break;
                            default:
                                if (number > 0)
                                {
                                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                                    {
                                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE) == number)
                                        {
                                            int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);

                                            // Check if the player has the number as contact
                                            int phone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);
                                            String contact = GetContactInTelephone(phone, playerPhone);

                                            if (contact.Length == 0)
                                            {
                                                contact = playerPhone.ToString();
                                            }

                                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, target);

                                            // Check if the player calling is a contact into target's contact list
                                            String playerMessage = String.Format(Messages.INF_CALLING, number);
                                            String targetMessage = String.Format(Messages.INF_CALL_FROM, contact.Length > 0 ? contact : contact.ToString());
                                            
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                                            return;
                                        }
                                    }
                                }

                                // The phone number doesn't exist
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
                                break;
                        }
                    }
                    else
                    {
                        // Call a contact
                        int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                        int targetPhone = GetNumerFromContactName(called, playerPhone);
                        
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
                                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_CALLING, target);

                                        // Check if the player is in target's contact list
                                        String contact = GetContactInTelephone(NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE), playerPhone);

                                        String playerMessage = String.Format(Messages.INF_CALLING, called);
                                        String targetMessage = String.Format(Messages.INF_CALL_FROM, contact.Length > 0 ? contact : playerPhone.ToString());
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_INCOMING_CALL);
                                    }
                                    return;
                                }
                            }
                        }

                        // The contact player isn't online
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_TELEPHONE_HAND);
                }
            }
        }

        [Command(Commands.COMMAND_ANSWER)]
        public void AnswerCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_CALLING) || NAPI.Data.HasEntityData(player, EntityData.PLAYER_PHONE_TALKING) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_PHONE_TALKING);
            }
            else
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    // Check if the target player is calling somebody
                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_CALLING) == true)
                    {
                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_CALLING) is int)
                        {
                            int factionJob = NAPI.Data.GetEntityData(target, EntityData.PLAYER_CALLING);
                            int faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                            int job = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);

                            if (factionJob == faction || factionJob == job + 100)
                            {
                                // Link both players in the same call
                                NAPI.Data.ResetEntityData(target, EntityData.PLAYER_CALLING);
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE_TALKING, target);
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_TALKING, player);
                                
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CALL_RECEIVED);
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CALL_TAKEN);

                                // Store call starting time
                                NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_CALL_STARTED, Globals.GetTotalSeconds());
                                return;
                            }
                        }
                        else if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_CALLING) == player)
                        {
                            // Link both players in the same call
                            NAPI.Data.ResetEntityData(target, EntityData.PLAYER_CALLING);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE_TALKING, target);
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_TALKING, player);
                            
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_CALL_RECEIVED);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.INF_CALL_TAKEN);

                            // Store call starting time
                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_PHONE_CALL_STARTED, Globals.GetTotalSeconds());
                            return;
                        }
                    }
                }

                // Nobody's calling the player
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_CALLED);
            }
        }

        [Command(Commands.COMMAND_HANG)]
        public void HangCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_CALLING) == true)
            {
                // Hang up the call
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_CALLING);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PHONE_TALKING) == true)
            {
                // Get the player he's talking with
                int elapsed = 0;
                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE_TALKING);
                int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                int targetPhone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);

                // Get phone call time
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

                // Hang up the call for both players
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

        [Command(Commands.COMMAND_SMS, Messages.GEN_SMS_COMMAND, GreedyArg = true)]
        public void SmsCommand(Client player, int number, String message)
        {
            ItemModel item = Globals.GetItemInEntity(NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
            if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (number > 0 && NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE) == number)
                    {
                        int playerPhone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);

                        // Check if the player's in the contact list
                        int phone = NAPI.Data.GetEntityData(target, EntityData.PLAYER_PHONE);
                        String contact = GetContactInTelephone(phone, playerPhone);

                        if (contact.Length == 0)
                        {
                            contact = playerPhone.ToString();
                        }
                        
                        String secondMessage = String.Empty;

                        if (message.Length > Constants.CHAT_LENGTH)
                        {
                            // We need two lines to print the full message
                            secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                            message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                        }
                        
                        NAPI.Chat.SendChatMessageToPlayer(target, secondMessage.Length > 0 ? Constants.COLOR_INFO + "[" + Messages.GEN_SMS_FROM + playerPhone + "] " + message + "..." : Constants.COLOR_INFO + "[" + Messages.GEN_SMS_FROM + playerPhone + "] " + message);
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

                        // Add the SMS into the database
                        Database.AddSMSLog(playerPhone, number, message);

                        return;
                    }
                }

                // The phone doesn't exist
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PHONE_DISCONNECTED);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_TELEPHONE_HAND);
            }
        }

        [Command(Commands.COMMAND_CONTACTS, Messages.GEN_CONTACTS_COMMAND)]
        public void AgendaCommand(Client player, String action)
        {
            ItemModel item = Globals.GetItemInEntity(NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID), Constants.ITEM_ENTITY_RIGHT_HAND);
            if (item != null && item.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                // Get the contact list
                int phoneNumber = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                List<ContactModel> contacts = GetTelephoneContactList(phoneNumber);

                switch (action.ToLower())
                {
                    case Commands.ARGUMENT_NUMBER:
                        String message = String.Format(Messages.INF_PHONE_NUMBER, phoneNumber);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                        break;
                    case Commands.ARGUMENT_VIEW:
                        if (contacts.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_LOAD);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CONTACT_LIST_EMPTY);
                        }
                        break;
                    case Commands.ARGUMENT_ADD:
                        NAPI.ClientEvent.TriggerClientEvent(player, "addContactWindow", Constants.ACTION_ADD);
                        break;
                    case Commands.ARGUMENT_MODIFY:
                        if (contacts.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_RENAME);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CONTACT_LIST_EMPTY);
                        }
                        break;
                    case Commands.ARGUMENT_REMOVE:
                        if (contacts.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPhoneContacts", NAPI.Util.ToJson(contacts), Constants.ACTION_DELETE);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_CONTACT_LIST_EMPTY);
                        }
                        break;
                    case Commands.ARGUMENT_SMS:
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