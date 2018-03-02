using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.database;
using WiredPlayers.model;
using WiredPlayers.drivingschool;
using WiredPlayers.weapons;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace WiredPlayers.police
{
    public class Police : Script
    {
        private static Timer reinforcesTimer;
        public static List<PoliceControlModel> policeControlList;

        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_HANDCUFFED) == true)
            {
                // Remove player's cuffs
                GTANetworkAPI.Object cuff = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HANDCUFFED);
                NAPI.Entity.DetachEntity(cuff);
                NAPI.Entity.DeleteEntity(cuff);
            }
        }

        private List<String> GetDifferentPoliceControls()
        {
            List<String> policeControls = new List<String>();
            foreach (PoliceControlModel policeControl in policeControlList)
            {
                if (policeControls.Contains(policeControl.name) == false && policeControl.name != String.Empty)
                {
                    policeControls.Add(policeControl.name);
                }
            }
            return policeControls;
        }

        private void RemoveClosestPoliceControlItem(Client player, int hash)
        {
            foreach (PoliceControlModel policeControl in policeControlList)
            {
                if (NAPI.Entity.DoesEntityExist(policeControl.controlObject) && NAPI.Entity.GetEntityPosition(policeControl.controlObject).DistanceTo(player.Position) < 2.0f && policeControl.item == hash)
                {
                    NAPI.Entity.DeleteEntity(policeControl.controlObject);
                    policeControl.controlObject = null;
                    break;
                }
            }
        }

        private void UpdateReinforcesRequests(object unused)
        {
            List<ReinforcesModel> policeReinforces = new List<ReinforcesModel>();
            List<Client> policeMembers = NAPI.Pools.GetAllPlayers().Where(x => NAPI.Data.GetEntityData(x, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE).ToList();
            
            foreach (Client police in policeMembers)
            {
                if (NAPI.Data.HasEntityData(police, EntityData.PLAYER_REINFORCES) == true)
                {
                    ReinforcesModel reinforces = new ReinforcesModel(police.Value, police.Position);
                    policeReinforces.Add(reinforces);
                }
            }
            
            String reinforcesJsonList = NAPI.Util.ToJson(policeReinforces);

            foreach (Client police in policeMembers)
            {
                if (NAPI.Data.HasEntityData(police, EntityData.PLAYER_PLAYING) == true)
                {
                    // Update reinforces position for each policeman
                    NAPI.ClientEvent.TriggerClientEvent(police, "updatePoliceReinforces", reinforcesJsonList);
                }
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            // Initialize reinforces updater
            reinforcesTimer = new Timer(UpdateReinforcesRequests, null, 250, 250);
        }

        [RemoteEvent("applyCrimesToPlayer")]
        public void ApplyCrimesToPlayerEvent(Client player, String crimeJson)
        {
            int fine = 0, jail = 0;
            Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_INCRIMINATED_TARGET);
            List<CrimeModel> crimeList = JsonConvert.DeserializeObject<List<CrimeModel>>(crimeJson);

            // Calculate fine amount and jail time
            foreach (CrimeModel crime in crimeList)
            {
                fine += crime.fine;
                jail += crime.jail;
            }
            
            Random random = new Random();
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_INCRIMINATED_TARGET, target);
            NAPI.Entity.SetEntityPosition(target, Constants.JAIL_SPAWNS[random.Next(3)]);

            // Remove money and jail the player
            int money = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY);
            NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_MONEY, money - fine);
            NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAIL_TYPE, Constants.JAIL_TYPE_IC);
            NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAILED, jail);
        }

        [RemoteEvent("policeControlSelected")]
        public void PoliceControlSelectedEvent(Client player, String policeControl)
        {
            if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_POLICE_CONTROL) == Constants.ACTION_LOAD)
            {
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (!NAPI.Entity.DoesEntityExist(policeControlModel.controlObject) && policeControlModel.name == policeControl)
                    {
                        policeControlModel.controlObject = NAPI.Object.CreateObject(policeControlModel.item, policeControlModel.position, policeControlModel.rotation);
                    }
                }
            }
            else if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_POLICE_CONTROL) == Constants.ACTION_SAVE)
            {
                List<PoliceControlModel> copiedPoliceControlModels = new List<PoliceControlModel>();
                List<PoliceControlModel> deletedPoliceControlModels = new List<PoliceControlModel>();
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (NAPI.Entity.DoesEntityExist(policeControlModel.controlObject) && policeControlModel.name != policeControl)
                    {
                        if (policeControlModel.name != String.Empty)
                        {
                            PoliceControlModel policeControlCopy = policeControlModel;
                            policeControlCopy.name = policeControl;
                            policeControlCopy.id = Database.AddPoliceControlItem(policeControlCopy);
                            copiedPoliceControlModels.Add(policeControlCopy);
                        }
                        else
                        {
                            policeControlModel.name = policeControl;
                            policeControlModel.id = Database.AddPoliceControlItem(policeControlModel);
                        }
                    }
                    else if (!NAPI.Entity.DoesEntityExist(policeControlModel.controlObject) && policeControlModel.name == policeControl)
                    {
                        Database.DeletePoliceControlItem(policeControlModel.id);
                        deletedPoliceControlModels.Add(policeControlModel);
                    }
                }
                policeControlList.AddRange(copiedPoliceControlModels);
                policeControlList = policeControlList.Except(deletedPoliceControlModels).ToList();
            }
            else
            {
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (NAPI.Entity.DoesEntityExist(policeControlModel.controlObject) && policeControlModel.name == policeControl)
                    {
                        NAPI.Entity.DeleteEntity(policeControlModel.controlObject);
                    }
                }
                policeControlList.RemoveAll(control => control.name == policeControl);
                Database.DeletePoliceControl(policeControl);
            }
        }

        [RemoteEvent("updatePoliceControlName")]
        public void UpdatePoliceControlNameEvent(Client player, String policeControlSource, String policeControlTarget)
        {
            if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_POLICE_CONTROL) == Constants.ACTION_SAVE)
            {
                List<PoliceControlModel> copiedPoliceControlModels = new List<PoliceControlModel>();
                List<PoliceControlModel> deletedPoliceControlModels = new List<PoliceControlModel>();
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (NAPI.Entity.DoesEntityExist(policeControlModel.controlObject) && policeControlModel.name != policeControlTarget)
                    {
                        if (policeControlModel.name != String.Empty)
                        {
                            PoliceControlModel policeControlCopy = policeControlModel.Copy();
                            policeControlModel.controlObject = null;
                            policeControlCopy.name = policeControlTarget;
                            policeControlCopy.id = Database.AddPoliceControlItem(policeControlCopy);
                            copiedPoliceControlModels.Add(policeControlCopy);
                        }
                        else
                        {
                            policeControlModel.name = policeControlTarget;
                            policeControlModel.id = Database.AddPoliceControlItem(policeControlModel);
                        }
                    }
                }
                policeControlList.AddRange(copiedPoliceControlModels);
            }
            else
            {
                policeControlList.Where(s => s.name == policeControlSource).ToList().ForEach(t => t.name = policeControlTarget);
                Database.RenamePoliceControl(policeControlSource, policeControlTarget);
            }
        }

        [Command(Commands.COMMAND_CHECK)]
        public void CheckCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
            else
            {
                Vehicle vehicle = Globals.GetClosestVehicle(player, 3.5f);
                if (vehicle == null)
                {
                    int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                    String checkTitle = String.Format(Messages.GEN_VEHICLE_CHECK_TITLE, vehicleId);
                    String model = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                    String plate = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PLATE);
                    String owner = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER);
                    NAPI.Chat.SendChatMessageToPlayer(player, checkTitle);
                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_MODEL + model);
                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_PLATE + plate);
                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_VEHICLE_OWNER + owner);

                    String message = String.Format(Messages.INF_CHECK_VEHICLE_PLATE, player.Name, model);

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if(player != target && player.Position.DistanceTo(target.Position) < 20.0f)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_CHAT_ME + message);
                        }
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                }
            }
        }

        [Command(Commands.COMMAND_FRISK, Messages.GEN_FRISK_COMMAND)]
        public void FriskCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    if (target == player)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_SEARCHED_HIMSELF);
                    }
                    else
                    {
                        String message = String.Format(Messages.INF_PLAYER_FRISK, player.Name, target.Name);
                        List<InventoryModel> inventory = Globals.GetPlayerInventoryAndWeapons(target);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_SEARCHED_TARGET, target);

                        foreach (Client nearPlayer in NAPI.Pools.GetAllPlayers())
                        {
                            if (player != nearPlayer && player.Position.DistanceTo(nearPlayer.Position) < 20.0f)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(nearPlayer, Constants.COLOR_CHAT_ME + message);
                            }
                        }

                        // Show target's inventory to the player
                        NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_PLAYER);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Commands.COMMAND_INCRIMINATE, Messages.GEN_INCRIMINATE_COMMAND)]
        public void IncriminateCommand(Client player, String targetString)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JAIL_AREA) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_JAIL_AREA);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    if (target == player)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_INCRIMINATED_HIMSELF);
                    }
                    else
                    {
                        String crimeList = NAPI.Util.ToJson(Constants.CRIME_LIST);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_INCRIMINATED_TARGET, target);
                        NAPI.ClientEvent.TriggerClientEvent(player, "showCrimesMenu", crimeList);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Commands.COMMAND_FINE, Messages.GEN_FINE_COMMAND)]
        public void FineCommand(Client player, String name = "", String surname = "", String amount = "", String reason = "")
        {
            if (name == String.Empty)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_FINE_COMMAND);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
            else
            {
                Client target = null;

                if (Int32.TryParse(name, out int targetId) == true)
                {
                    target = Globals.GetPlayerById(targetId);
                    reason = amount;
                    amount = surname;
                }
                else
                {
                    target = NAPI.Player.GetPlayerFromName(name + " " + surname);
                }
                if (target != null && NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                {
                    if (player.Position.DistanceTo(target.Position) > 2.5f)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                    }
                    else if (target == player)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_FINED_HIMSELF);
                    }
                    else
                    {
                        String playerMessage = String.Format(Messages.INF_FINE_GIVEN, target.Name);
                        String targetMessage = String.Format(Messages.INF_FINE_RECEIVED, player.Name);
                        FineModel fine = new FineModel();
                        fine.officer = player.Name;
                        fine.target = target.Name;
                        fine.amount = Int32.Parse(amount);
                        fine.reason = reason;
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                        Database.InsertFine(fine);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Commands.COMMAND_HANDCUFF, Messages.GEN_HANDCUFF_COMMAND)]
        public void HandcuffCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null)
                {
                    if (player.Position.DistanceTo(target.Position) > 1.5f)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                    }
                    else if (target == player)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HANDCUFFED_HIMSELF);
                    }
                    else if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_HANDCUFFED) == false)
                    {
                        String playerMessage = String.Format(Messages.INF_CUFFED, target.Name);
                        String targetMessage = String.Format(Messages.INF_CUFFED_BY, player.Name);
                        GTANetworkAPI.Object cuff = NAPI.Object.CreateObject(-1281059971, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                        NAPI.Entity.AttachEntityToEntity(cuff, target, "IK_R_Hand", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                        NAPI.Player.PlayPlayerAnimation(target, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "mp_arresting", "idle");
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, true);
                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_HANDCUFFED, cuff);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                        // Disable some player movements
                        NAPI.ClientEvent.TriggerClientEvent(player, "toggleHandcuffed", true);
                    }
                    else
                    {
                        String playerMessage = String.Format(Messages.INF_UNCUFFED, target.Name);
                        String targetMessage = String.Format(Messages.INF_UNCUFFED_BY, player.Name);
                        GTANetworkAPI.Object cuff = NAPI.Data.GetEntityData(target, EntityData.PLAYER_HANDCUFFED);
                        NAPI.Entity.DetachEntity(cuff);
                        NAPI.Entity.DeleteEntity(cuff);
                        NAPI.Player.StopPlayerAnimation(target);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ANIMATION);
                        NAPI.Data.ResetEntityData(target, EntityData.PLAYER_HANDCUFFED);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                        // Enable previously disabled player movements
                        NAPI.ClientEvent.TriggerClientEvent(player, "toggleHandcuffed", false);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command(Commands.COMMAND_EQUIPMENT, Messages.GEN_EQUIPMENT_COMMAND, GreedyArg = true)]
        public void EquipmentCommand(Client player, String action, String type = "")
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_IN_LSPD_ROOM_LOCKERS_AREA) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_ROOM_LOCKERS);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
            {
                switch (action.ToLower())
                {
                    case Commands.ARGUMENT_BASIC:
                        NAPI.Player.SetPlayerArmor(player, 100);
                        Weapons.GivePlayerNewWeapon(player, WeaponHash.Flashlight, 0, false);
                        Weapons.GivePlayerNewWeapon(player, WeaponHash.Nightstick, 0, true);
                        Weapons.GivePlayerNewWeapon(player, WeaponHash.StunGun, 0, true);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_EQUIP_BASIC_RECEIVED);
                        break;
                    case Commands.ARGUMENT_AMMUNITION:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) > 1)
                        {
                            WeaponHash[] playerWeaps = NAPI.Player.GetPlayerWeapons(player);
                            foreach (WeaponHash playerWeap in playerWeaps)
                            {
                                String ammunition = Weapons.GetGunAmmunitionType(playerWeap);
                                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                                ItemModel bulletItem = Globals.GetPlayerItemModelFromHash(playerId, ammunition);
                                if (bulletItem != null)
                                {
                                    switch (playerWeap)
                                    {
                                        case WeaponHash.CombatPistol:
                                            bulletItem.amount += Constants.STACK_PISTOL_CAPACITY;
                                            break;
                                        case WeaponHash.SMG:
                                            bulletItem.amount += Constants.STACK_MACHINEGUN_CAPACITY;
                                            break;
                                        case WeaponHash.CarbineRifle:
                                            bulletItem.amount += Constants.STACK_ASSAULTRIFLE_CAPACITY;
                                            break;
                                        case WeaponHash.PumpShotgun:
                                            bulletItem.amount += Constants.STACK_SHOTGUN_CAPACITY;
                                            break;
                                        case WeaponHash.SniperRifle:
                                            bulletItem.amount += Constants.STACK_SNIPERRIFLE_CAPACITY;
                                            break;
                                    }
                                    Database.UpdateItem(bulletItem);
                                }
                                else
                                {
                                    bulletItem = new ItemModel();
                                    bulletItem.hash = ammunition;
                                    bulletItem.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                                    bulletItem.ownerIdentifier = playerId;
                                    bulletItem.amount = 30;
                                    bulletItem.position = new Vector3(0.0f, 0.0f, 0.0f);
                                    bulletItem.dimension = 0;
                                    bulletItem.id = Database.AddNewItem(bulletItem);
                                    Globals.itemList.Add(bulletItem);
                                }
                            }
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_EQUIP_AMMO_RECEIVED);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_POLICE_RANK);
                        }
                        break;
                    case Commands.ARGUMENT_WEAPON:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) > 1)
                        {
                            WeaponHash selectedWeap = new WeaponHash();
                            switch (type.ToLower())
                            {
                                case Commands.ARGUMENT_PISTOL:
                                    selectedWeap = WeaponHash.CombatPistol;
                                    break;
                                case Commands.ARGUMENT_MACHINE_GUN:
                                    selectedWeap = WeaponHash.SMG;
                                    break;
                                case Commands.ARGUMENT_ASSAULT:
                                    selectedWeap = WeaponHash.CarbineRifle;
                                    break;
                                case Commands.ARGUMENT_SNIPER:
                                    selectedWeap = WeaponHash.SniperRifle;
                                    break;
                                case Commands.ARGUMENT_SHOTGUN:
                                    selectedWeap = WeaponHash.PumpShotgun;
                                    break;
                                default:
                                    selectedWeap = 0;
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_EQUIPMENT_WEAP_COMMAND);
                                    break;
                            }

                            if (selectedWeap != 0)
                            {
                                Weapons.GivePlayerNewWeapon(player, selectedWeap, 0, true);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_EQUIP_WEAP_RECEIVED);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_POLICE_RANK);
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_EQUIPMENT_AMMO_COMMAND);
                        break;
                }
            }
        }

        [Command(Commands.COMMAND_CONTROL, Messages.GEN_POLICE_CONTROL_COMMAND)]
        public void ControlCommand(Client player, String action)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
            else
            {
                List<String> policeControls = GetDifferentPoliceControls();
                switch (action.ToLower())
                {
                    case Commands.ARGUMENT_LOAD:
                        if (policeControls.Count > 0)
                        {
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_POLICE_CONTROL, Constants.ACTION_LOAD);
                            NAPI.ClientEvent.TriggerClientEvent(player, "loadPoliceControlList", NAPI.Util.ToJson(policeControls));
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_POLICE_CONTROLS);
                        }
                        break;
                    case Commands.ARGUMENT_SAVE:
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_POLICE_CONTROL, Constants.ACTION_SAVE);
                        if (policeControls.Count > 0)
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "loadPoliceControlList", NAPI.Util.ToJson(policeControls));
                        }
                        else
                        {
                            NAPI.ClientEvent.TriggerClientEvent(player, "showPoliceControlName");
                        }
                        break;
                    case Commands.ARGUMENT_RENAME:
                        if (policeControls.Count > 0)
                        {
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_POLICE_CONTROL, Constants.ACTION_RENAME);
                            NAPI.ClientEvent.TriggerClientEvent(player, "loadPoliceControlList", NAPI.Util.ToJson(policeControls));
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_POLICE_CONTROLS);
                        }
                        break;
                    case Commands.ARGUMENT_REMOVE:
                        if (policeControls.Count > 0)
                        {
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_POLICE_CONTROL, Constants.ACTION_DELETE);
                            NAPI.ClientEvent.TriggerClientEvent(player, "loadPoliceControlList", NAPI.Util.ToJson(policeControls));
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_POLICE_CONTROLS);
                        }
                        break;
                    case Commands.ARGUMENT_CLEAR:
                        foreach (PoliceControlModel policeControl in policeControlList)
                        {
                            if (NAPI.Entity.DoesEntityExist(policeControl.controlObject) == true)
                            {
                                NAPI.Entity.DeleteEntity(policeControl.controlObject);
                            }
                        }
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_POLICE_CONTROL_CLEARED);
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_POLICE_CONTROL_COMMAND);
                        break;
                }
            }
        }

        [Command(Commands.COMMAND_PUT, Messages.GEN_POLICE_PUT_COMMAND)]
        public void PutCommand(Client player, String item)
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
                PoliceControlModel policeControl = null;
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                {
                    switch (item.ToLower())
                    {
                        case Commands.ARGUMENT_CONE:
                            policeControl = new PoliceControlModel(0, String.Empty, Constants.POLICE_DEPLOYABLE_CONE, player.Position, player.Rotation);
                            policeControl.position = new Vector3(policeControl.position.X, policeControl.position.Y, policeControl.position.Z - 1.0f);
                            policeControl.controlObject = NAPI.Object.CreateObject(Constants.POLICE_DEPLOYABLE_CONE, policeControl.position, policeControl.rotation);
                            policeControlList.Add(policeControl);
                            break;
                        case Commands.ARGUMENT_BEACON:
                            policeControl = new PoliceControlModel(0, String.Empty, Constants.POLICE_DEPLOYABLE_BEACON, player.Position, player.Rotation);
                            policeControl.position = new Vector3(policeControl.position.X, policeControl.position.Y, policeControl.position.Z - 1.0f);
                            policeControl.controlObject = NAPI.Object.CreateObject(Constants.POLICE_DEPLOYABLE_BEACON, policeControl.position, policeControl.rotation);
                            policeControlList.Add(policeControl);
                            break;
                        case Commands.ARGUMENT_BARRIER:
                            policeControl = new PoliceControlModel(0, String.Empty, Constants.POLICE_DEPLOYABLE_BARRIER, player.Position, player.Rotation);
                            policeControl.position = new Vector3(policeControl.position.X, policeControl.position.Y, policeControl.position.Z - 1.0f);
                            policeControl.controlObject = NAPI.Object.CreateObject(Constants.POLICE_DEPLOYABLE_BARRIER, policeControl.position, policeControl.rotation);
                            policeControlList.Add(policeControl);
                            break;
                        case Commands.ARGUMENT_SPIKES:
                            policeControl = new PoliceControlModel(0, String.Empty, Constants.POLICE_DEPLOYABLE_SPIKES, player.Position, player.Rotation);
                            policeControl.position = new Vector3(policeControl.position.X, policeControl.position.Y, policeControl.position.Z - 1.0f);
                            policeControl.controlObject = NAPI.Object.CreateObject(Constants.POLICE_DEPLOYABLE_SPIKES, policeControl.position, policeControl.rotation);
                            policeControlList.Add(policeControl);
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_POLICE_PUT_COMMAND);
                            break;
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
                }
            }
        }

        [Command(Commands.COMMAND_REMOVE, Messages.GEN_POLICE_REMOVE_COMMAND)]
        public void RemoveCommand(Client player, String item)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
            else
            {
                switch (item.ToLower())
                {
                    case Commands.ARGUMENT_CONE:
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_CONE);
                        break;
                    case Commands.ARGUMENT_BEACON:
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_BEACON);
                        break;
                    case Commands.ARGUMENT_BARRIER:
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_BARRIER);
                        break;
                    case Commands.ARGUMENT_SPIKES:
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_SPIKES);
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_POLICE_REMOVE_COMMAND);
                        break;
                }
            }
        }

        [Command(Commands.COMMAND_REINFORCES)]
        public void ReinforcesCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
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
                // Get police department's members
                List<Client> policeMembers = NAPI.Pools.GetAllPlayers().Where(x => NAPI.Data.GetEntityData(x, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE).ToList();

                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_REINFORCES) == true)
                {
                    String targetMessage = String.Format(Messages.INF_TARGET_REINFORCES_CANCELED, player.Name);

                    foreach (Client target in policeMembers)
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) == 1)
                        {
                            // Remove the blip from the map
                            NAPI.ClientEvent.TriggerClientEvent(target, "reinforcesRemove", player.Value);
                            
                            if (player == target)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_REINFORCES_CANCELED);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                        }
                    }

                    // Remove player's reinforces
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REINFORCES);
                }
                else
                {
                    String targetMessage = String.Format(Messages.INF_TARGET_REINFORCES_ASKED, player.Name);

                    foreach (Client target in policeMembers)
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) == 1)
                        {
                            if (player == target)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_REINFORCES_ASKED);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                            }
                        }
                    }

                    // Ask for reinforces
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_REINFORCES, true);
                }
            }
        }

        [Command(Commands.COMMAND_LICENSE, Messages.GEN_LICENSE_COMMAND, GreedyArg = true)]
        public void LicenseCommand(Client player, String args)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) == 6)
            {
                String[] arguments = args.Trim().Split(' ');
                if (arguments.Length == 3 || arguments.Length == 4)
                {
                    Client target = null;

                    // Get the target player
                    if (Int32.TryParse(arguments[2], out int targetId) && arguments.Length == 3)
                    {
                        target = Globals.GetPlayerById(targetId);
                    }
                    else
                    {
                        target = NAPI.Player.GetPlayerFromName(arguments[2] + arguments[3]);
                    }

                    // Check whether the target player is connected
                    if (target == null || NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                    }
                    else if (player.Position.DistanceTo(target.Position) > 2.5f)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                    }
                    else
                    {
                        String playerMessage = String.Empty;
                        String targetMessage = String.Empty;

                        switch (arguments[0].ToLower())
                        {
                            case Commands.ARGUMENT_GIVE:
                                switch (arguments[1].ToLower())
                                {
                                    case Commands.ARGUMENT_WEAPON:
                                        // Add one month to the license
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_WEAPON_LICENSE, Globals.GetTotalSeconds() + 2628000);
                                        
                                        playerMessage = String.Format(Messages.INF_WEAPON_LICENSE_GIVEN, target.Name);
                                        targetMessage = String.Format(Messages.INF_WEAPON_LICENSE_RECEIVED, player.Name);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        break;
                                    default:
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_LICENSE_COMMAND);
                                        break;
                                }
                                break;
                            case Commands.ARGUMENT_REMOVE:
                                switch (arguments[1].ToLower())
                                {
                                    case Commands.ARGUMENT_WEAPON:
                                        // Adjust the date to the current one
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_WEAPON_LICENSE, Globals.GetTotalSeconds());
                                        
                                        playerMessage = String.Format(Messages.INF_WEAPON_LICENSE_REMOVED, target.Name);
                                        targetMessage = String.Format(Messages.INF_WEAPON_LICENSE_LOST, player.Name);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        break;
                                    case Commands.ARGUMENT_CAR:
                                        // Remove car license
                                        DrivingSchool.SetPlayerLicense(target, Constants.LICENSE_CAR, -1);
                                        
                                        playerMessage = String.Format(Messages.INF_CAR_LICENSE_REMOVED, target.Name);
                                        targetMessage = String.Format(Messages.INF_CAR_LICENSE_LOST, player.Name);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        break;
                                    case Commands.ARGUMENT_MOTORCYCLE:
                                        // Remove motorcycle license
                                        DrivingSchool.SetPlayerLicense(target, Constants.LICENSE_MOTORCYCLE, -1);
                                        
                                        playerMessage = String.Format(Messages.INF_MOTO_LICENSE_REMOVED, target.Name);
                                        targetMessage = String.Format(Messages.INF_MOTO_LICENSE_LOST, player.Name);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_LICENSE_COMMAND);
                                break;
                        }
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_LICENSE_COMMAND);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_POLICE_CHIEF);
            }
        }

        [Command(Commands.COMMAND_BREATHALYZER, Messages.GEN_ALCOHOLIMETER_COMMAND)]
        public void BreathalyzerCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) > 0)
            {
                float alcoholLevel = 0.0f;
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_DRUNK_LEVEL) == true)
                {
                    alcoholLevel = NAPI.Data.GetEntityData(target, EntityData.PLAYER_DRUNK_LEVEL);
                }
                
                String playerMessage = String.Format(Messages.INF_ALCOHOLIMETER_TEST, target.Name, alcoholLevel);
                String targetMessage = String.Format(Messages.INF_ALCOHOLIMETER_RECEPTOR, player.Name, alcoholLevel);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_POLICE_FACTION);
            }
        }
    }
}
