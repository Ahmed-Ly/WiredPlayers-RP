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

namespace WiredPlayers.police
{
    class Police : Script
    {
        private static Timer reinforcesTimer;
        public static List<PoliceControlModel> policeControlList;

        public Police()
        {
            Event.OnResourceStart += OnResourceStart;
            Event.OnClientEventTrigger += OnClientEventTrigger;
            Event.OnPlayerDisconnected += OnPlayerDisconnectedHandler;
        }

        private void OnResourceStart()
        {
            // Inicializamos el timer que mira si se han pedido refuerzos
            reinforcesTimer = new Timer(UpdateReinforcesRequests, null, 250, 250);
        }

        private void OnClientEventTrigger(Client player, String eventName, params object[] arguments)
        {
            if (eventName == "applyCrimesToPlayer")
            {
                int fine = 0, jail = 0;
                String[] crimeList = arguments[0].ToString().Split(',');
                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_INCRIMINATED_TARGET);

                // Calculamos la multa y tiempo de cárcel
                for (int i = 0; i < Constants.CRIME_LIST.Count; i++)
                {
                    if (crimeList.Contains(Constants.CRIME_LIST.ElementAt(i).crime) == true)
                    {
                        fine += Constants.CRIME_LIST.ElementAt(i).fine;
                        jail += Constants.CRIME_LIST.ElementAt(i).jail;
                    }
                }

                // Metemos al jugador en una de las celdas
                Random random = new Random();
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_INCRIMINATED_TARGET, target);
                NAPI.Entity.SetEntityPosition(target, Constants.JAIL_SPAWNS[random.Next(3)]);

                // Aplicamos la condena
                int money = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY);
                NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_MONEY, money - fine);
                NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAIL_TYPE, Constants.JAIL_TYPE_IC);
                NAPI.Data.SetEntityData(target, EntityData.PLAYER_JAILED, jail);
            }
            else if (eventName == "policeControlSelected")
            {
                String policeControl = arguments[0].ToString();
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
            else if (eventName == "policeControlNamed")
            {
                String policeControlSource = arguments[0].ToString();
                String policeControlTarget = arguments[1].ToString();
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
                                policeControlModel.controlObject = new NetHandle();
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
        }

        private void OnPlayerDisconnectedHandler(Client player, byte type, string reason)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_HANDCUFFED) == true)
            {
                // Quitamos las esposas al personaje
                NetHandle cuff = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HANDCUFFED);
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
                    policeControl.controlObject = new NetHandle();
                    break;
                }
            }
        }

        private void UpdateReinforcesRequests(object unused)
        {
            // Obtenemos los miembros de policía
            List<ReinforcesModel> policeReinforces = new List<ReinforcesModel>();
            List<Client> policeMembers = NAPI.Pools.GetAllPlayers().Where(x => NAPI.Data.GetEntityData(x, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE).ToList();
            
            // Recogemos todas las posiciones
            foreach(Client police in policeMembers)
            {
                if(NAPI.Data.HasEntityData(police, EntityData.PLAYER_REINFORCES) == true)
                {
                    int policeId = NAPI.Data.GetEntityData(police, EntityData.PLAYER_ID);
                    ReinforcesModel reinforces = new ReinforcesModel(policeId, police.Position);
                    policeReinforces.Add(reinforces);
                }
            }

            // Convertimos la lista a JSON
            String reinforcesJsonList = NAPI.Util.ToJson(policeReinforces);

            foreach(Client police in policeMembers)
            {
                if(NAPI.Data.HasEntityData(police, EntityData.PLAYER_PLAYING) == true)
                {
                    // Actualizamos la posición para cada policía
                    NAPI.ClientEvent.TriggerClientEvent(police, "updatePoliceReinforces", reinforcesJsonList);
                }
            }
        }

        [Command("comprobar")]
        public void ComprobarCommand(Client player)
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
                if(vehicle == null)
                {
                    int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                    String model = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                    String plate = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PLATE);
                    String owner = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER);
                    NAPI.Chat.SendChatMessageToPlayer(player, "_________Información del vehículo con ID " + vehicleId + "_________");
                    NAPI.Chat.SendChatMessageToPlayer(player, "Modelo: " + model);
                    NAPI.Chat.SendChatMessageToPlayer(player, "Matrícula: " + plate);
                    NAPI.Chat.SendChatMessageToPlayer(player, "Propietario: " + owner);

                    List<Client> playerList = NAPI.Player.GetPlayersInRadiusOfPlayer(20, player);
                    foreach (Client playerItem in playerList)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(playerItem, Constants.COLOR_CHAT_ME + player.Name + " comprueba la matrícula del vehículo " + model + ".");
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_VEHICLES_NEAR);
                }
            }
        }

        [Command("cachear")]
        public void CachearCommand(Client player, String targetString)
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
                        List<Client> playerList = NAPI.Player.GetPlayersInRadiusOfPlayer(20, player);
                        foreach (Client playerItem in playerList)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(playerItem, Constants.COLOR_CHAT_ME + player.Name + " realiza un cacheo a " + target.Name);
                        }
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_SEARCHED_TARGET, target);
                        List<InventoryModel> inventory = Globals.GetPlayerInventoryAndWeapons(target);
                        NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_PLAYER);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("inculpar")]
        public void InculparCommand(Client player, String targetString)
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
                        NAPI.ClientEvent.TriggerClientEvent(player, "mostrarMenuDelitos", crimeList);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("multar", Messages.GEN_FINE_COMMAND)]
        public void MultarCommand(Client player, String name = "", String surname = "", String amount = "", String reason = "")
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

        [Command("esposar", Messages.GEN_HANDCUFF_COMMAND)]
        public void EsposarCommand(Client player, String targetString)
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
                        NetHandle cuff = NAPI.Object.CreateObject(-1281059971, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                        NAPI.Entity.AttachEntityToEntity(cuff, target, "IK_R_Hand", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                        NAPI.Player.PlayPlayerAnimation(target, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "mp_arresting", "idle");
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, true);
                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_HANDCUFFED, cuff);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                        // Inhabilitamos ciertos movimientos
                        NAPI.ClientEvent.TriggerClientEvent(player, "toggleHandcuffed", true);
                    }
                    else
                    {
                        String playerMessage = String.Format(Messages.INF_UNCUFFED, target.Name);
                        String targetMessage = String.Format(Messages.INF_UNCUFFED_BY, player.Name);
                        NetHandle cuff = NAPI.Data.GetEntityData(target, EntityData.PLAYER_HANDCUFFED);
                        NAPI.Entity.DetachEntity(cuff);
                        NAPI.Entity.DeleteEntity(cuff);
                        NAPI.Player.StopPlayerAnimation(target);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ANIMATION);
                        NAPI.Data.ResetEntityData(target, EntityData.PLAYER_HANDCUFFED);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                        // Habilitamos los movimientos deshabilitados
                        NAPI.ClientEvent.TriggerClientEvent(player, "toggleHandcuffed", false);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
            }
        }

        [Command("equipo", Messages.GEN_EQUIPMENT_COMMAND, GreedyArg = true)]
        public void EquipoCommand(Client player, String action, String type = "")
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
                    case "basico":
                        NAPI.Player.SetPlayerArmor(player, 100);
                        Weapons.GivePlayerNewWeapon(player, WeaponHash.Flashlight, 0, false);
                        Weapons.GivePlayerNewWeapon(player, WeaponHash.Nightstick, 0, true);
                        Weapons.GivePlayerNewWeapon(player, WeaponHash.StunGun, 0, true);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_EQUIP_BASIC_RECEIVED);
                        break;
                    case "municion":
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
                    case "arma":
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) > 1)
                        {
                            WeaponHash selectedWeap = new WeaponHash();
                            switch (type.ToLower())
                            {
                                case "pistola":
                                    selectedWeap = WeaponHash.CombatPistol;
                                    break;
                                case "metralleta":
                                    selectedWeap = WeaponHash.SMG;
                                    break;
                                case "asalto":
                                    selectedWeap = WeaponHash.CarbineRifle;
                                    break;
                                case "francotirador":
                                    selectedWeap = WeaponHash.SniperRifle;
                                    break;
                                case "escopeta":
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

        [Command("control", Messages.GEN_POLICE_CONTROL_COMMAND)]
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
                    case "cargar":
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
                    case "guardar":
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
                    case "renombrar":
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
                    case "eliminar":
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
                    case "limpiar":
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

        [Command("poner", Messages.GEN_POLICE_PUT_COMMAND)]
        public void PonerCommand(Client player, String item)
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
                        case "cono":
                            policeControl = new PoliceControlModel(0, String.Empty, Constants.POLICE_DEPLOYABLE_CONE, player.Position, player.Rotation);
                            policeControl.position = new Vector3(policeControl.position.X, policeControl.position.Y, policeControl.position.Z - 1.0f);
                            policeControl.controlObject = NAPI.Object.CreateObject(Constants.POLICE_DEPLOYABLE_CONE, policeControl.position, policeControl.rotation);
                            policeControlList.Add(policeControl);
                            break;
                        case "baliza":
                            policeControl = new PoliceControlModel(0, String.Empty, Constants.POLICE_DEPLOYABLE_BEACON, player.Position, player.Rotation);
                            policeControl.position = new Vector3(policeControl.position.X, policeControl.position.Y, policeControl.position.Z - 1.0f);
                            policeControl.controlObject = NAPI.Object.CreateObject(Constants.POLICE_DEPLOYABLE_BEACON, policeControl.position, policeControl.rotation);
                            policeControlList.Add(policeControl);
                            break;
                        case "barrera":
                            policeControl = new PoliceControlModel(0, String.Empty, Constants.POLICE_DEPLOYABLE_BARRIER, player.Position, player.Rotation);
                            policeControl.position = new Vector3(policeControl.position.X, policeControl.position.Y, policeControl.position.Z - 1.0f);
                            policeControl.controlObject = NAPI.Object.CreateObject(Constants.POLICE_DEPLOYABLE_BARRIER, policeControl.position, policeControl.rotation);
                            policeControlList.Add(policeControl);
                            break;
                        case "clavos":
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

        [Command("quitar", Messages.GEN_POLICE_REMOVE_COMMAND)]
        public void QuitarCommand(Client player, String item)
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
                    case "cono":
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_CONE);
                        break;
                    case "baliza":
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_BEACON);
                        break;
                    case "barrera":
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_BARRIER);
                        break;
                    case "clavos":
                        RemoveClosestPoliceControlItem(player, Constants.POLICE_DEPLOYABLE_SPIKES);
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_POLICE_REMOVE_COMMAND);
                        break;
                }
            }
        }

        [Command("refuerzos", Alias = "sr")]
        public void RefuerzosCommand(Client player)
        {
            if(NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) != Constants.FACTION_POLICE)
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
                // Obtenemos la lista de policías
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                List<Client> policeMembers = NAPI.Pools.GetAllPlayers().Where(x => NAPI.Data.GetEntityData(x, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE).ToList();

                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_REINFORCES) == true)
                {
                    // Creamos el mensaje
                    String targetMessage = String.Format(Messages.INF_TARGET_REINFORCES_CANCELED, player.Name);

                    foreach (Client target in policeMembers)
                    {
                        if(NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) == 1)
                        {
                            // Mostramos la marca en el mapa
                            NAPI.ClientEvent.TriggerClientEvent(target, "reinforcesRemove", playerId);

                            // Mandamos el mensaje a los jugadores
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

                    // Quitamos los refuerzos del jugador
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REINFORCES);
                }
                else
                {
                    // Creamos el mensaje
                    String targetMessage = String.Format(Messages.INF_TARGET_REINFORCES_ASKED, player.Name);

                    foreach(Client target in policeMembers)
                    {
                        if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) == 1)
                        {
                            // Mandamos el mensaje a los jugadores
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

                    // Marcamos los refuerzos del jugador
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_REINFORCES, true);
                }
            }
        }

        [Command("licencia", Messages.GEN_LICENSE_COMMAND, GreedyArg = true)]
        public void LicenciaCommand(Client player, String args)
        {
            if(NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) == 6)
            {
                String[] arguments = args.Trim().Split(' ');
                if(arguments.Length == 3 || arguments.Length == 4)
                {
                    Client target = null;

                    // Miramos cuál es el jugador objetivo
                    if(Int32.TryParse(arguments[2], out int targetId) && arguments.Length == 3)
                    {
                        target = Globals.GetPlayerById(targetId);
                    }
                    else
                    {
                        target = NAPI.Player.GetPlayerFromName(arguments[2] + arguments[3]);
                    }

                    // Miramos si está conectado
                    if (target == null || NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                    }
                    else if(player.Position.DistanceTo(target.Position) > 2.5f)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                    }
                    else
                    {
                        String playerMessage = String.Empty;
                        String targetMessage = String.Empty;

                        switch (arguments[0].ToLower())
                        {
                            case "dar":
                                // Miramos las opciones de dar
                                switch (arguments[1].ToLower())
                                {
                                    case "armas":
                                        // Añadimos un mes a la licencia
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_WEAPON_LICENSE, Globals.GetTotalSeconds() + 2628000);

                                        // Mandamos el mensaje a los jugadores
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
                            case "quitar":
                                switch (arguments[1].ToLower())
                                {
                                    case "armas":
                                        // Añadimos un mes a la licencia
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_WEAPON_LICENSE, Globals.GetTotalSeconds());

                                        // Mandamos el mensaje a los jugadores
                                        playerMessage = String.Format(Messages.INF_WEAPON_LICENSE_REMOVED, target.Name);
                                        targetMessage = String.Format(Messages.INF_WEAPON_LICENSE_LOST, player.Name);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        break;
                                    case "turismos":
                                        // Eliminamos la licencia de turismos
                                        DrivingSchool.SetPlayerLicense(target, Constants.LICENSE_CAR, -1);

                                        // Mandamos el mensaje a los jugadores
                                        playerMessage = String.Format(Messages.INF_CAR_LICENSE_REMOVED, target.Name);
                                        targetMessage = String.Format(Messages.INF_CAR_LICENSE_LOST, player.Name);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        break;
                                    case "motocicletas":
                                        // Eliminamos la licencia de motocicletas
                                        DrivingSchool.SetPlayerLicense(target, Constants.LICENSE_MOTORCYCLE, -1);

                                        // Mandamos el mensaje a los jugadores
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

        [Command("alcoholimetro", Messages.GEN_ALCOHOLIMETER_COMMAND)]
        public void AlcoholimetroCommand(Client player, String targetString)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) > 0)
            {
                float alcoholLevel = 0.0f;
                Client target = Int32.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_DRUNK_LEVEL) == true)
                {
                    alcoholLevel = NAPI.Data.GetEntityData(target, EntityData.PLAYER_DRUNK_LEVEL);
                }

                // Mandamos el mensaje a los jugadores
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
