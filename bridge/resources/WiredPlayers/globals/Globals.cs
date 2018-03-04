using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.database;
using WiredPlayers.house;
using WiredPlayers.business;
using WiredPlayers.chat;
using WiredPlayers.weapons;
using WiredPlayers.hooker;
using WiredPlayers.parking;
using WiredPlayers.faction;
using WiredPlayers.vehicles;
using WiredPlayers.drivingschool;
using WiredPlayers.emergency;
using WiredPlayers.fastfood;
using WiredPlayers.fishing;
using WiredPlayers.garbage;
using WiredPlayers.login;
using WiredPlayers.police;
using WiredPlayers.thief;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

namespace WiredPlayers.globals
{
    public class Globals : Script
    {
        private int fastFoodId = 1;
        public static int orderGenerationTime;
        public static List<FastFoodOrderModel> fastFoodOrderList;
        public static List<ClothesModel> clothesList;
        public static List<TattooModel> tattooList;
        public static List<ItemModel> itemList;
        public static List<ScoreModel> scoreList;
        public static List<AdminTicketModel> adminTicketList;
        private Timer minuteTimer;
        private Timer playersCheckTimer;

        public static Client GetPlayerById(int id)
        {
            Client target = null;
            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (player.Value == id)
                {
                    target = player;
                    break;
                }
            }
            return target;
        }

        public static Vector3 GetBusinessIplExit(String ipl)
        {
            Vector3 position = null;
            foreach (BusinessIplModel iplModel in Constants.BUSINESS_IPL_LIST)
            {
                if (iplModel.ipl == ipl)
                {
                    position = iplModel.position;
                    break;
                }
            }
            return position;
        }

        public static Vector3 GetHouseIplExit(String ipl)
        {
            Vector3 position = null;
            foreach (HouseIplModel iplModel in Constants.HOUSE_IPL_LIST)
            {
                if (iplModel.ipl == ipl)
                {
                    position = iplModel.position;
                    break;
                }
            }
            return position;
        }

        /*
        private void OnChatCommandHandler(Client player, String command, CancelEventArgs e)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                NAPI.Util.ConsoleOutput(player.Name + " ha usado el comando '" + command + "'");
            }
            else if(command.StartsWith("/login") == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_COMMAND);
                e.Cancel = true;
            }
        }*/

        public static Vehicle GetClosestVehicle(Client player, float distance = 2.5f)
        {
            Vehicle vehicle = null;
            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
            {
                Vector3 vehPos = NAPI.Entity.GetEntityPosition(veh);
                uint vehicleDimension = NAPI.Entity.GetEntityDimension(veh);
                float distanceVehicleToPlayer = player.Position.DistanceTo(vehPos);
                if (distanceVehicleToPlayer < distance && player.Dimension == vehicleDimension)
                {
                    distance = distanceVehicleToPlayer;
                    vehicle = veh;

                }
            }
            return vehicle;
        }

        public static int GetTotalSeconds()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private void UpdatePlayerList(object unused)
        {
            // Update player list
            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
                {
                    ScoreModel scoreModel = scoreList.First(score => score.playerId == player.Value);
                    scoreModel.playerPing = NAPI.Player.GetPlayerPing(player);
                }
            }
        }

        private void OnMinuteSpent(object unused)
        {
            // Adjust server's time
            TimeSpan currentTime = TimeSpan.FromTicks(DateTime.Now.Ticks);
            NAPI.World.SetTime(currentTime.Hours, currentTime.Minutes, currentTime.Seconds);

            int totalSeconds = GetTotalSeconds();
            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
                {
                    int played = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PLAYED);
                    if (played > 0 && played % 60 == 0)
                    {
                        // Reduce job cooldown
                        int employeeCooldown = NAPI.Data.GetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN);
                        if (employeeCooldown > 0)
                        {
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN, employeeCooldown - 1);
                        }

                        // Generate the payday
                        GeneratePlayerPayday(player);
                    }
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_PLAYED, played + 1);

                    // Check if the player is injured waiting for the hospital respawn
                    if (NAPI.Data.HasEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN) == true)
                    {
                        if (NAPI.Data.GetEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN) <= totalSeconds)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_CAN_DIE);
                        }
                    }

                    // Check if the player has job cooldown
                    int jobCooldown = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN);
                    if (jobCooldown > 0)
                    {
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN, jobCooldown - 1);
                    }

                    // Check if the player's in jail
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JAILED) == true)
                    {
                        int jailTime = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JAILED);
                        if (jailTime == 1)
                        {
                            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JAIL_TYPE) == Constants.JAIL_TYPE_IC)
                            {
                                NAPI.Entity.SetEntityPosition(player, Constants.JAIL_SPAWNS[3]);
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, Constants.JAIL_SPAWNS[4]);
                            }

                            // Remove player from jail
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JAILED, 0);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JAIL_TYPE, 0);
                            
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_UNJAILED);
                        }
                        else if (jailTime > 0)
                        {
                            jailTime--;
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JAILED, jailTime);
                        }
                    }

                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRUNK_LEVEL) == true)
                    {
                        // Lower alcohol level
                        float drunkLevel = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRUNK_LEVEL) - 0.05f;

                        if (drunkLevel <= 0.0f)
                        {
                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DRUNK_LEVEL);
                        }
                        else
                        {
                            if (drunkLevel < Constants.WASTED_LEVEL)
                            {
                                NAPI.Data.ResetEntitySharedData(player, EntityData.PLAYER_WALKING_STYLE);
                                NAPI.ClientEvent.TriggerClientEventForAll("resetPlayerWalkingStyle", player.Handle);
                            }
                            
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRUNK_LEVEL, drunkLevel);
                        }
                    }

                    PlayerModel character = new PlayerModel();
                    
                    character.position = NAPI.Entity.GetEntityPosition(player);
                    character.rotation = NAPI.Entity.GetEntityRotation(player);
                    character.health = NAPI.Player.GetPlayerHealth(player);
                    character.armor = NAPI.Player.GetPlayerArmor(player);
                    character.id = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    character.phone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                    character.radio = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RADIO);
                    character.killed = NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED);
                    character.faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                    character.job = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);
                    character.rank = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK);
                    character.duty = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY);
                    character.carKeys = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS);
                    character.documentation = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DOCUMENTATION);
                    character.licenses = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSES);
                    character.insurance = NAPI.Data.GetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE);
                    character.weaponLicense = NAPI.Data.GetEntityData(player, EntityData.PLAYER_WEAPON_LICENSE);
                    character.houseRent = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RENT_HOUSE);
                    character.houseEntered = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                    character.businessEntered = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                    character.employeeCooldown = NAPI.Data.GetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN);
                    character.jobCooldown = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN);
                    character.jobDeliver = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_DELIVER);
                    character.jobPoints = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_POINTS);
                    character.rolePoints = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ROLE_POINTS);
                    character.played = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PLAYED);
                    character.jailed = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JAIL_TYPE) + "," + NAPI.Data.GetEntityData(player, EntityData.PLAYER_JAILED);
                    
                    character.money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                    character.bank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK);

                    // Save the player into database
                    Database.SaveCharacterInformation(character);
                }
            }

            // Generate new fastfood orders
            if (orderGenerationTime <= totalSeconds && House.houseList.Count > 0)
            {
                Random rnd = new Random();
                int generatedOrders = rnd.Next(7, 20);
                for (int i = 0; i < generatedOrders; i++)
                {
                    FastFoodOrderModel order = new FastFoodOrderModel();
                    order.id = fastFoodId;
                    order.pizzas = rnd.Next(0, 4);
                    order.hamburgers = rnd.Next(0, 4);
                    order.sandwitches = rnd.Next(0, 4);
                    order.position = GetPlayerFastFoodDeliveryDestination();
                    order.limit = totalSeconds + 300;
                    order.taken = false;
                    fastFoodOrderList.Add(order);
                    fastFoodId++;
                }

                // Update the new timer time
                orderGenerationTime = totalSeconds + rnd.Next(2, 5) * 60;
            }

            // Remove old orders
            fastFoodOrderList.RemoveAll(order => !order.taken && order.limit <= totalSeconds);
            
            List<VehicleModel> vehicleList = new List<VehicleModel>();

            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                if (!NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_TESTING) && NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION) == 0)
                {
                    Color primaryColor = NAPI.Vehicle.GetVehicleCustomPrimaryColor(vehicle);
                    Color secondaryColor = NAPI.Vehicle.GetVehicleCustomSecondaryColor(vehicle);
                    
                    VehicleModel vehicleModel = new VehicleModel();
                    vehicleModel.id = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                    vehicleModel.model = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                    vehicleModel.position = NAPI.Entity.GetEntityPosition(vehicle);
                    vehicleModel.rotation = NAPI.Entity.GetEntityRotation(vehicle);
                    vehicleModel.dimension = NAPI.Entity.GetEntityDimension(vehicle);
                    vehicleModel.colorType = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE);
                    vehicleModel.firstColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR);
                    vehicleModel.secondColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR);
                    vehicleModel.pearlescent = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR);
                    vehicleModel.faction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);
                    vehicleModel.plate = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PLATE);
                    vehicleModel.owner = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER);
                    vehicleModel.price = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PRICE);
                    vehicleModel.parking = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PARKING);
                    vehicleModel.parked = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PARKED);
                    vehicleModel.gas = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_GAS);
                    vehicleModel.kms = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_KMS);

                    // Add vehicle into the list
                    vehicleList.Add(vehicleModel);
                }
            }

            // Save all the vehicles
            Database.SaveAllVehicles(vehicleList);
        }

        private void GeneratePlayerPayday(Client player)
        {
            int total = 0;
            int bank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK);
            int playerJob = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);
            int playerRank = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK);
            int playerFaction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PAYDAY_TITLE);

            // Generate the salary
            if (playerFaction > 0 && playerFaction <= Constants.LAST_STATE_FACTION)
            {
                foreach (FactionModel faction in Constants.FACTION_RANK_LIST)
                {
                    if (faction.faction == playerFaction && faction.rank == playerRank)
                    {
                        total += faction.salary;
                        break;
                    }
                }
            }
            else
            {
                foreach (JobModel job in Constants.JOB_LIST)
                {
                    if (job.job == playerJob)
                    {
                        total += job.salary;
                        break;
                    }
                }
            }
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SALARY + total + "$");

            // Extra income from the level
            int levelEarnings = GetPlayerLevel(player) * Constants.PAID_PER_LEVEL;
            total += levelEarnings;
            if (levelEarnings > 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_EXTRA_INCOME + levelEarnings + "$");
            }

            // Bank interest
            int bankInterest = (int)Math.Round(bank * 0.001);
            total += bankInterest;
            if (bankInterest > 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_BANK_INTEREST + bankInterest + "$");
            }

            // Generación de impuestos por vehículos
            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                VehicleHash vehicleHass = (VehicleHash)NAPI.Entity.GetEntityModel(vehicle);
                if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER) == player.Name && NAPI.Vehicle.GetVehicleClass(vehicleHass) != Constants.VEHICLE_CLASS_CYCLES)
                {
                    int vehicleTaxes = (int)Math.Round(NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PRICE) * Constants.TAXES_VEHICLE);
                    int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                    String vehicleModel = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                    String vehiclePlate = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PLATE) == String.Empty ? "LS " + (1000 + vehicleId) : NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PLATE);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "Impuestos de " + vehicleModel + " (" + vehiclePlate + "): -" + vehicleTaxes + "$");
                    total -= vehicleTaxes;
                }
            }

            // Vehicle taxes
            foreach (ParkedCarModel parkedCar in Parking.parkedCars)
            {
                VehicleHash vehicleHass = NAPI.Util.VehicleNameToModel(parkedCar.vehicle.model);
                if (parkedCar.vehicle.owner == player.Name && NAPI.Vehicle.GetVehicleClass(vehicleHass) != Constants.VEHICLE_CLASS_CYCLES)
                {
                    int vehicleTaxes = (int)Math.Round(parkedCar.vehicle.price * Constants.TAXES_VEHICLE);
                    String vehiclePlate = parkedCar.vehicle.plate == String.Empty ? "LS " + (1000 + parkedCar.vehicle.id) : parkedCar.vehicle.plate;
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_VEHICLE_TAXES_FROM + parkedCar.vehicle.model + " (" + vehiclePlate + "): -" + vehicleTaxes + "$");
                    total -= vehicleTaxes;
                }
            }

            // House taxes
            foreach (HouseModel house in House.houseList)
            {
                if (house.owner == player.Name)
                {
                    int houseTaxes = (int)Math.Round(house.price * Constants.TAXES_HOUSE);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_HOUSE_TAXES_FROM + house.name + ": -" + houseTaxes + "$");
                    total -= houseTaxes;
                }
            }

            // Calculate the total balance
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + "=====================");
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_TOTAL + total + "$");
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, bank + total);

            // Add the payment log
            Database.LogPayment("Payday", player.Name, "Payday", total);
        }

        private Vector3 GetPlayerFastFoodDeliveryDestination()
        {
            Random random = new Random();
            int element = random.Next(House.houseList.Count);
            return House.houseList[element].position;
        }

        public static ItemModel GetItemModelFromId(int itemId)
        {
            ItemModel item = null;
            foreach (ItemModel itemModel in itemList)
            {
                if (itemModel.id == itemId)
                {
                    item = itemModel;
                    break;
                }
            }
            return item;
        }

        public static ItemModel GetPlayerItemModelFromHash(int playerId, String hash)
        {
            ItemModel itemModel = null;
            foreach (ItemModel item in itemList)
            {
                if (item.ownerEntity == Constants.ITEM_ENTITY_PLAYER && item.ownerIdentifier == playerId && item.hash == hash)
                {
                    itemModel = item;
                    break;
                }
            }
            return itemModel;
        }

        public static ItemModel GetClosestItem(Client player)
        {
            ItemModel itemModel = null;
            foreach (ItemModel item in itemList)
            {
                if (item.ownerEntity == Constants.ITEM_ENTITY_GROUND && player.Position.DistanceTo(item.position) < 2.0f)
                {
                    itemModel = item;
                    break;
                }
            }
            return itemModel;
        }

        public static ItemModel GetClosestItemWithHash(Client player, String hash)
        {
            ItemModel itemModel = null;
            foreach (ItemModel item in itemList)
            {
                if (item.ownerEntity == Constants.ITEM_ENTITY_GROUND && item.hash == hash && player.Position.DistanceTo(item.position) < 2.0f)
                {
                    itemModel = item;
                    break;
                }
            }
            return itemModel;
        }

        public static ItemModel GetItemInEntity(int entityId, String entity)
        {
            ItemModel item = null;
            foreach (ItemModel itemModel in itemList)
            {
                if (itemModel.ownerEntity == entity && itemModel.ownerIdentifier == entityId)
                {
                    item = itemModel;
                    break;
                }
            }
            return item;
        }

        private void SubstractPlayerItems(ItemModel item, int amount = 1)
        {
            item.amount -= amount;
            if (item.amount == 0)
            {
                Database.RemoveItem(item.id);
                itemList.Remove(item);
            }
        }

        private int GetPlayerInventoryTotal(Client player)
        {
            int totalItems = 0;
            foreach (ItemModel item in itemList)
            {
                if (item.ownerEntity == Constants.ITEM_ENTITY_PLAYER && NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID) == item.ownerIdentifier)
                {
                    totalItems++;
                }
            }
            return totalItems;
        }

        private List<InventoryModel> GetPlayerInventory(Client player)
        {
            List<InventoryModel> inventory = new List<InventoryModel>();
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            foreach (ItemModel item in itemList)
            {
                if (item.ownerEntity == Constants.ITEM_ENTITY_PLAYER && item.ownerIdentifier == playerId)
                {
                    BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);
                    if (businessItem != null && businessItem.type != Constants.ITEM_TYPE_WEAPON)
                    {
                        // Create the item into the inventory
                        InventoryModel inventoryItem = new InventoryModel();
                        inventoryItem.id = item.id;
                        inventoryItem.hash = item.hash;
                        inventoryItem.description = businessItem.description;
                        inventoryItem.type = businessItem.type;
                        inventoryItem.amount = item.amount;

                        // Add the item to the inventory
                        inventory.Add(inventoryItem);
                    }
                }
            }
            return inventory;
        }

        public static List<InventoryModel> GetPlayerInventoryAndWeapons(Client player)
        {
            List<InventoryModel> inventory = new List<InventoryModel>();
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            foreach (ItemModel item in itemList)
            {
                if (item.ownerEntity == Constants.ITEM_ENTITY_PLAYER && item.ownerIdentifier == playerId)
                {
                    BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);
                    if (businessItem != null)
                    {
                        // Create the item into the inventory
                        InventoryModel inventoryItem = new InventoryModel();
                        inventoryItem.id = item.id;
                        inventoryItem.hash = item.hash;
                        inventoryItem.description = businessItem.description;
                        inventoryItem.type = businessItem.type;
                        inventoryItem.amount = item.amount;

                        // Add the item to the inventory
                        inventory.Add(inventoryItem);
                    }
                }
            }
            return inventory;
        }

        public static List<InventoryModel> GetVehicleTrunkInventory(Vehicle vehicle)
        {
            List<InventoryModel> inventory = new List<InventoryModel>();
            int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
            foreach (ItemModel item in itemList)
            {
                if (item.ownerEntity == Constants.ITEM_ENTITY_VEHICLE && item.ownerIdentifier == vehicleId)
                {
                    // Check whether is a common item or a weapon
                    InventoryModel inventoryItem = new InventoryModel();
                    BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);

                    if (businessItem != null)
                    {
                        inventoryItem.description = businessItem.description;
                        inventoryItem.type = businessItem.type;
                    }
                    else
                    {
                        inventoryItem.description = item.hash;
                        inventoryItem.type = Constants.ITEM_TYPE_WEAPON;
                    }

                    // Update the values
                    inventoryItem.id = item.id;
                    inventoryItem.hash = item.hash;
                    inventoryItem.amount = item.amount;

                    // Add the item to the inventory
                    inventory.Add(inventoryItem);
                }
            }
            return inventory;
        }

        public static List<ClothesModel> GetPlayerClothes(int playerId)
        {
            List<ClothesModel> clothesModelList = new List<ClothesModel>();
            foreach (ClothesModel clothes in clothesList)
            {
                if (clothes.player == playerId)
                {
                    clothesModelList.Add(clothes);
                }
            }
            return clothesModelList;
        }

        public static ClothesModel GetDressedClothesInSlot(int playerId, int type, int slot)
        {
            ClothesModel clothesDressed = null;
            foreach (ClothesModel clothes in clothesList)
            {
                if (clothes.player == playerId && clothes.type == type && clothes.slot == slot && clothes.dressed)
                {
                    clothesDressed = clothes;
                    break;
                }
            }
            return clothesDressed;
        }

        public static List<String> GetClothesNames(List<ClothesModel> clothesList)
        {
            List<String> clothesNames = new List<String>();
            foreach (ClothesModel clothes in clothesList)
            {
                foreach (BusinessClothesModel businessClothes in Constants.BUSINESS_CLOTHES_LIST)
                {
                    if (businessClothes.clothesId == clothes.drawable && businessClothes.bodyPart == clothes.slot && businessClothes.type == clothes.type)
                    {
                        clothesNames.Add(businessClothes.description);
                        break;
                    }
                }
            }
            return clothesNames;
        }

        public static void UndressClothes(int playerId, int type, int slot)
        {
            foreach (ClothesModel clothes in clothesList)
            {
                if (clothes.player == playerId && clothes.type == type && clothes.slot == slot && clothes.dressed)
                {
                    clothes.dressed = false;
                    Database.UpdateClothes(clothes);
                    break;
                }
            }
        }

        public static void PopulateCharacterClothes(Client player)
        {
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            foreach (ClothesModel clothes in clothesList)
            {
                if (clothes.player == playerId && clothes.dressed)
                {
                    if (clothes.type == 0)
                    {
                        NAPI.Player.SetPlayerClothes(player, clothes.slot, clothes.drawable, 0);
                    }
                    else
                    {
                        NAPI.Player.SetPlayerAccessory(player, clothes.slot, clothes.drawable, 0);
                    }
                }
            }
        }

        public static List<TattooModel> GetPlayerTattoos(int playerId)
        {
            List<TattooModel> tattooModelList = new List<TattooModel>();
            foreach (TattooModel tattoo in tattooList)
            {
                if (tattoo.player == playerId)
                {
                    tattooModelList.Add(tattoo);
                }
            }
            return tattooModelList;
        }

        public static void GetPlayerBasicData(Client asker, Client player)
        {
            int rolePoints = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ROLE_POINTS);
            String sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? Messages.GEN_SEX_MALE : Messages.GEN_SEX_FEMALE;
            String age = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_AGE) + Messages.GEN_YEARS;
            String money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY) + "$";
            String bank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK) + "$";
            String job = Messages.GEN_UNEMPLOYED;
            String faction = Messages.GEN_NO_FACTION;
            String rank = Messages.GEN_NO_RANK;
            String houses = String.Empty;
            String ownedVehicles = String.Empty;
            String lentVehicles = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS);
            TimeSpan played = TimeSpan.FromMinutes(NAPI.Data.GetEntityData(player, EntityData.PLAYER_PLAYED));

            // Check if the player has a job
            foreach (JobModel jobModel in Constants.JOB_LIST)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) == jobModel.job)
                {
                    job = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? jobModel.descriptionMale : jobModel.descriptionFemale;
                    break;
                }
            }

            // Check if the player is in any faction
            foreach (FactionModel factionModel in Constants.FACTION_RANK_LIST)
            {
                if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == factionModel.faction && NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK) == factionModel.rank)
                {
                    switch (factionModel.faction)
                    {
                        case Constants.FACTION_POLICE:
                            faction = Messages.GEN_POLICE_FACTION;
                            break;
                        case Constants.FACTION_EMERGENCY:
                            faction = Messages.GEN_EMERGENCY_FACTION;
                            break;
                        case Constants.FACTION_NEWS:
                            faction = Messages.GEN_NEWS_FACTION;
                            break;
                        case Constants.FACTION_TOWNHALL:
                            faction = Messages.GEN_TOWNHALL_FACTION;
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            faction = Messages.GEN_TRANSPORT_FACTION;
                            break;
                    }

                    // Set player's rank
                    rank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? factionModel.descriptionMale : factionModel.descriptionFemale;
                    break;
                }
            }

            // Check if the player has any rented house
            if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_RENT_HOUSE) > 0)
            {
                houses += " " + NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_RENT_HOUSE);
            }

            // Get player's owned houses
            foreach (HouseModel house in House.houseList)
            {
                if (house.owner == player.Name)
                {
                    houses += " " + house.id;
                }
            }

            // Check for the player's owned vehicles
            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER) == player.Name)
                {
                    ownedVehicles += " " + NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                }
            }
            
            foreach (ParkedCarModel parkedVehicle in Parking.parkedCars)
            {
                if (parkedVehicle.vehicle.owner == player.Name)
                {
                    ownedVehicles += " " + parkedVehicle.vehicle.id;
                }
            }

            // Show all the information
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_INFO + Messages.INF_BASIC_DATA);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_HELP + Messages.GEN_NAME + player.Name + "; " + Messages.GEN_SEX + sex + "; " + Messages.GEN_AGE + age + "; " + Messages.GEN_MONEY + money + "; " + Messages.GEN_BANK + bank);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_INFO + " ");
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_INFO + Messages.INF_JOB_DATA);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_HELP + Messages.GEN_JOB + job + "; " + Messages.GEN_FACTION + faction + "; " + Messages.GEN_RANK + rank);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_INFO + " ");
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_INFO + Messages.INF_PROPERTIES);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_HELP + Messages.GEN_HOUSES + houses);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_HELP + Messages.GEN_OWNED_VEHICLES + ownedVehicles);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_HELP + Messages.GEN_LENT_VEHICLES + lentVehicles);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_INFO + " ");
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_INFO + Messages.INF_ADDITIONAL_DATA);
            NAPI.Chat.SendChatMessageToPlayer(asker, Constants.COLOR_HELP + Messages.GEN_PLAYED_TIME + (int)played.TotalHours + "h " + played.Minutes + "m; " + Messages.GEN_ROLE_POINTS + rolePoints);
        }

        private int GetPlayerLevel(Client player)
        {
            float playedHours = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PLAYED) / 100;
            return (int)Math.Round(Math.Log(playedHours) * Constants.LEVEL_MULTIPLIER);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seat)
        {
            //NAPI.Native.SendNativeToPlayer(player, Hash.SET_PED_HELMET, player, false);
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            scoreList = new List<ScoreModel>();
            adminTicketList = new List<AdminTicketModel>();
            fastFoodOrderList = new List<FastFoodOrderModel>();

            // Area in the lobby to change the character
            NAPI.TextLabel.CreateTextLabel(Messages.GEN_CHARACTER_HELP, new Vector3(152.2911f, -1001.088f, -99f), 20.0f, 0.75f, 4, new Color(255, 255, 255), false);

            // Add car dealer's interior
            NAPI.World.RequestIpl("shr_int");
            NAPI.World.RequestIpl("shr_int_lod");
            NAPI.World.RemoveIpl("fakeint");
            NAPI.World.RemoveIpl("fakeint_lod");
            NAPI.World.RemoveIpl("fakeint_boards");
            NAPI.World.RemoveIpl("fakeint_boards_lod");
            NAPI.World.RemoveIpl("shutter_closed");

            // Add clubhouse's door
            NAPI.World.RequestIpl("hei_bi_hw1_13_door");

            // Avoid player's respawn
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);

            // Disable global server chat
            NAPI.Server.SetGlobalServerChat(false);

            foreach (InteriorModel interior in Constants.INTERIOR_LIST)
            {
                if (interior.blipId > 0)
                {
                    interior.blip = NAPI.Blip.CreateBlip(interior.entrancePosition);
                    NAPI.Blip.SetBlipSprite(interior.blip, interior.blipId);
                    NAPI.Blip.SetBlipName(interior.blip, interior.blipName);
                    NAPI.Blip.SetBlipShortRange(interior.blip, true);
                }

                if (interior.captionMessage != String.Empty)
                {
                    interior.textLabel = NAPI.TextLabel.CreateTextLabel(interior.captionMessage, interior.entrancePosition, 20.0f, 0.75f, 4, new Color(255, 255, 255), false, 0);
                }
            }
            
            // Fastfood orders
            Random rnd = new Random();
            orderGenerationTime = GetTotalSeconds() + rnd.Next(0, 1) * 60;

            // Permanent timers
            playersCheckTimer = new Timer(UpdatePlayerList, null, 500, 500);
            minuteTimer = new Timer(OnMinuteSpent, null, 60000, 60000);
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            Login.OnPlayerDisconnected(player, type, reason);

            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                // Disconnect from the server
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_PLAYING);

                // Remove player from players list
                scoreList.RemoveAll(score => score.playerId == player.Value);

                // Remove opened ticket
                adminTicketList.RemoveAll(ticket => ticket.playerId == player.Value);

                // Other classes' disconnect function
                Chat.OnPlayerDisconnected(player, type, reason);
                DrivingSchool.OnPlayerDisconnected(player, type, reason);
                Emergency.OnPlayerDisconnected(player, type, reason);
                FastFood.OnPlayerDisconnected(player, type, reason);
                Fishing.OnPlayerDisconnected(player, type, reason);
                Garbage.OnPlayerDisconnected(player, type, reason);
                Hooker.OnPlayerDisconnected(player, type, reason);
                Police.OnPlayerDisconnected(player, type, reason);
                Thief.OnPlayerDisconnected(player, type, reason);
                Vehicles.OnPlayerDisconnected(player, type, reason);
                Weapons.OnPlayerDisconnected(player, type, reason);

                // Delete items in the hand
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
                {
                    int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                    ItemModel item = GetItemModelFromId(itemId);
                    if (item != null && item.objectHandle != null && NAPI.Entity.DoesEntityExist(item.objectHandle) == true)
                    {
                        NAPI.Entity.DetachEntity(item.objectHandle);
                        NAPI.Entity.DeleteEntity(item.objectHandle);
                    }
                }

                PlayerModel character = new PlayerModel();
                
                character.position = NAPI.Entity.GetEntityPosition(player);
                character.rotation = NAPI.Entity.GetEntityRotation(player);
                character.health = NAPI.Player.GetPlayerHealth(player);
                character.armor = NAPI.Player.GetPlayerArmor(player);
                character.id = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                character.phone = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE);
                character.radio = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RADIO);
                character.killed = NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED);
                character.faction = NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION);
                character.job = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB);
                character.rank = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RANK);
                character.duty = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY);
                character.carKeys = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS);
                character.documentation = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DOCUMENTATION);
                character.licenses = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSES);
                character.insurance = NAPI.Data.GetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE);
                character.weaponLicense = NAPI.Data.GetEntityData(player, EntityData.PLAYER_WEAPON_LICENSE);
                character.houseRent = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RENT_HOUSE);
                character.houseEntered = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                character.businessEntered = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                character.employeeCooldown = NAPI.Data.GetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN);
                character.jobCooldown = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN);
                character.jobDeliver = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_DELIVER);
                character.jobPoints = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_POINTS);
                character.rolePoints = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ROLE_POINTS);
                character.played = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PLAYED);
                character.jailed = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JAIL_TYPE) + "," + NAPI.Data.GetEntityData(player, EntityData.PLAYER_JAILED);
                
                character.money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                character.bank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK);

                // Save player into database
                Database.SaveCharacterInformation(character);

                // Avisamos a los jugadores cercanos de la desconexión
                String message = String.Format(Messages.INF_PLAYER_DISCONNECTED, player.Name, reason);
                Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_DISCONNECT, 10.0f);
            }
        }

        [RemoteEvent("checkPlayerEventKeyStopAnim")]
        public void CheckPlayerEventKeyStopAnimEvent(Client player)
        {
            if (!NAPI.Data.HasEntityData(player, EntityData.PLAYER_ANIMATION) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Player.StopPlayerAnimation(player);
            }
        }

        [RemoteEvent("checkPlayerInventoryKey")]
        public void CheckPlayerInventoryKeyEvent(Client player)
        {
            if (GetPlayerInventoryTotal(player) > 0)
            {
                List<InventoryModel> inventory = GetPlayerInventory(player);
                NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_SELF);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_ITEMS_INVENTORY);
            }
        }

        [RemoteEvent("checkPlayerEventKey")]
        public void CheckPlayerEventKeyEvent(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                // Check if the player's close to an ATM
                for (int i = 0; i < Constants.ATM_LIST.Count; i++)
                {
                    if (player.Position.DistanceTo(Constants.ATM_LIST[i]) <= 1.5f)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(player, "showATM");
                        return;
                    }
                }

                // Check if the player's in any business
                foreach (BusinessModel business in Business.businessList)
                {
                    if (player.Position.DistanceTo(business.position) <= 1.5f && player.Dimension == business.dimension)
                    {
                        if (!Business.HasPlayerBusinessKeys(player, business) && business.locked)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_BUSINESS_LOCKED);
                        }
                        else
                        {
                            Vector3 pos = GetBusinessIplExit(business.ipl);
                            NAPI.World.RequestIpl(business.ipl);
                            NAPI.Entity.SetEntityPosition(player, pos);
                            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(business.id));
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_IPL, business.ipl);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, business.id);
                        }
                        return;
                    }
                    else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) == business.id)
                    {
                        Vector3 exitPosition = Business.GetBusinessExitPoint(business.ipl);
                        if (player.Position.DistanceTo(exitPosition) < 2.5f)
                        {
                            if (!Business.HasPlayerBusinessKeys(player, business) && business.locked)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_BUSINESS_LOCKED);
                            }
                            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ROBBERY_START) == true)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_STEALING_PROGRESS);
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, business.position);
                                NAPI.Entity.SetEntityDimension(player, business.dimension);
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
                                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_IPL);
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.HasEntityData(target, EntityData.PLAYER_IPL) && target != player)
                                    {
                                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_IPL) == business.ipl)
                                        {
                                            return;
                                        }
                                    }
                                }
                                NAPI.World.RemoveIpl(business.ipl);
                            }
                        }
                        return;
                    }
                }

                // Check if the player's in any house
                foreach (HouseModel house in House.houseList)
                {
                    if (player.Position.DistanceTo(house.position) <= 1.5f && player.Dimension == house.dimension)
                    {
                        if (!House.HasPlayerHouseKeys(player, house) && house.locked)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_LOCKED);
                        }
                        else
                        {
                            Vector3 pos = GetHouseIplExit(house.ipl);
                            NAPI.World.RequestIpl(house.ipl);
                            NAPI.Entity.SetEntityPosition(player, pos);
                            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(house.id));
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_IPL, house.ipl);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, house.id);
                        }
                        return;
                    }
                    else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) == house.id)
                    {
                        Vector3 exitPosition = House.GetHouseExitPoint(house.ipl);
                        if (player.Position.DistanceTo(exitPosition) < 2.5f)
                        {
                            if (!House.HasPlayerHouseKeys(player, house) && house.locked)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_LOCKED);
                            }
                            else if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ROBBERY_START) == true)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_STEALING_PROGRESS);
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, house.position);
                                NAPI.Entity.SetEntityDimension(player, house.dimension);
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
                                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_IPL);
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.HasEntityData(target, EntityData.PLAYER_IPL) && target != player)
                                    {
                                        if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_IPL) == house.ipl)
                                        {
                                            return;
                                        }
                                    }
                                }
                                NAPI.World.RemoveIpl(house.ipl);
                            }
                        }
                        return;
                    }
                }

                // Check if the player's in any interior
                foreach (InteriorModel interior in Constants.INTERIOR_LIST)
                {
                    if (player.Position.DistanceTo(interior.entrancePosition) < 1.5f)
                    {
                        NAPI.World.RequestIpl(interior.iplName);
                        NAPI.Entity.SetEntityPosition(player, interior.exitPosition);
                        return;
                    }
                    else if (player.Position.DistanceTo(interior.exitPosition) < 1.5f)
                    {
                        NAPI.Entity.SetEntityPosition(player, interior.entrancePosition);
                        return;
                    }
                }
            }
            else
            {
                Vector3 lobbyExit = new Vector3(151.3791f, -1007.905f, -99f);

                if (lobbyExit.DistanceTo(player.Position) < 1.25f)
                {
                    // Player must have a character selected
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_SQL_ID) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_CHARACTER_SELECTED);
                    }
                    else
                    {
                        int playerSqlId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        int playerHealth = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HEALTH);
                        int playerArmor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ARMOR);
                        String realName = NAPI.Data.GetEntityData(player, EntityData.PLAYER_NAME);
                        Vector3 spawnPosition = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SPAWN_POS);
                        Vector3 spawnRotation = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SPAWN_ROT);
                        ItemModel rightHand = GetItemInEntity(playerSqlId, Constants.ITEM_ENTITY_RIGHT_HAND);
                        ItemModel leftHand = GetItemInEntity(playerSqlId, Constants.ITEM_ENTITY_LEFT_HAND);

                        // Give the weapons to the player
                        Weapons.GivePlayerWeaponItems(player);
                        
                        if (rightHand != null)
                        {
                            BusinessItemModel businessItem = Business.GetBusinessItemFromHash(rightHand.hash);

                            if (businessItem == null || businessItem.type == Constants.ITEM_TYPE_WEAPON)
                            {
                                WeaponHash weapon = NAPI.Util.WeaponNameToModel(rightHand.hash);
                                NAPI.Player.GivePlayerWeapon(player, weapon, rightHand.amount);
                            }
                            else
                            {
                                rightHand.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(rightHand.hash), rightHand.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)rightHand.dimension);
                                NAPI.Entity.AttachEntityToEntity(rightHand.objectHandle, player, "PH_R_Hand", businessItem.position, businessItem.rotation);
                                NAPI.Player.GivePlayerWeapon(player, WeaponHash.Unarmed, 1);
                            }
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RIGHT_HAND, rightHand.id);
                        }
                        
                        if (leftHand != null)
                        {
                            BusinessItemModel businessItem = Business.GetBusinessItemFromHash(leftHand.hash);
                            leftHand.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(leftHand.hash), leftHand.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)leftHand.dimension);
                            NAPI.Entity.AttachEntityToEntity(leftHand.objectHandle, player, "PH_L_Hand", businessItem.position, businessItem.rotation);
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_LEFT_HAND, leftHand.id);
                        }

                        // Calculate spawn dimension
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) > 0)
                        {
                            int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                            HouseModel house = House.GetHouseById(houseId);
                            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(house.id));
                            NAPI.World.RequestIpl(house.ipl);
                        }
                        else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) > 0)
                        {
                            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                            BusinessModel business = Business.GetBusinessById(businessId);
                            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(business.id));
                            NAPI.World.RequestIpl(business.ipl);
                        }
                        else
                        {
                            NAPI.Entity.SetEntityDimension(player, 0);
                        }

                        // Add player into connected list
                        ScoreModel scoreModel = new ScoreModel(player.Value, player.Name, player.Ping);
                        scoreList.Add(scoreModel);

                        // Spawn the player into the world
                        NAPI.Player.SetPlayerName(player, realName);
                        NAPI.Entity.SetEntityPosition(player, spawnPosition);
                        NAPI.Entity.SetEntityRotation(player, spawnRotation);
                        NAPI.Player.SetPlayerHealth(player, playerHealth);
                        NAPI.Player.SetPlayerArmor(player, playerArmor);
                        
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
                        {
                            Vector3 deathPosition = null;
                            String deathPlace = String.Empty;
                            String deathHour = DateTime.Now.ToString("h:mm:ss tt");
                            
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

                            // Creamos the report for the emergency department
                            FactionWarningModel factionWarning = new FactionWarningModel(Constants.FACTION_EMERGENCY, player.Value, deathPlace, deathPosition, -1, deathHour);
                            Faction.factionWarningList.Add(factionWarning);
                            
                            String warnMessage = String.Format(Messages.INF_EMERGENCY_WARNING, Faction.factionWarningList.Count - 1);
                            
                            foreach (Client target in NAPI.Pools.GetAllPlayers())
                            {
                                if (NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ON_DUTY) == 0)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + warnMessage);
                                }
                            }

                            NAPI.Entity.SetEntityInvincible(player, true);
                            NAPI.Data.SetEntityData(player, EntityData.TIME_HOSPITAL_RESPAWN, GetTotalSeconds() + 240);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_EMERGENCY_WARN);
                        }

                        // Toggle connection flag
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_PLAYING, true);
                    }
                }
                else if (player.Position.DistanceTo(new Vector3(152.2911f, -1001.088f, -99f)) < 1.5f)
                {
                    // Show character menu
                    List<String> playerList = Database.GetAccountCharacters(player.SocialClubName);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerCharacters", NAPI.Util.ToJson(playerList));
                }
            }
        }

        [RemoteEvent("processMenuAction")]
        public void ProcessMenuActionEvent(Client player, int itemId, String action)
        {
            String message = String.Empty;
            ItemModel item = GetItemModelFromId(itemId);
            BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);

            switch (action)
            {
                case Messages.COM_CONSUME:
                    item.amount--;
                    Database.UpdateItem(item);
                    message = String.Format(Messages.INF_PLAYER_INVENTORY_CONSUME, businessItem.description.ToLower());
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);

                    // Check if it grows alcohol level
                    if (businessItem.alcoholLevel > 0)
                    {
                        float currentAlcohol = 0;
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRUNK_LEVEL) == true)
                        {
                            currentAlcohol = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRUNK_LEVEL);
                        }
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRUNK_LEVEL, currentAlcohol + businessItem.alcoholLevel);
                        
                        if (currentAlcohol + businessItem.alcoholLevel > Constants.WASTED_LEVEL)
                        {
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_WALKING_STYLE, "move_m@drunk@verydrunk");
                            NAPI.ClientEvent.TriggerClientEventForAll("changePlayerWalkingStyle", player.Handle, "move_m@drunk@verydrunk");
                        }
                    }

                    // Check if it changes the health
                    if (businessItem.health != 0)
                    {
                        int health = NAPI.Player.GetPlayerHealth(player);
                        NAPI.Player.SetPlayerHealth(player, health + businessItem.health);
                    }

                    // Check if it was the last one remaining
                    if (item.amount == 0)
                    {
                        Database.RemoveItem(item.id);
                        itemList.Remove(item);
                    }

                    // Update the inventory
                    List<InventoryModel> inventory = GetPlayerInventory(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_SELF);
                    break;
                case Messages.ARG_OPEN:
                    switch (item.hash)
                    {
                        case Constants.ITEM_HASH_PACK_BEER_AM:
                            ItemModel itemModel = GetPlayerItemModelFromHash(NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID), Constants.ITEM_HASH_BOTTLE_BEER_AM);
                            if (itemModel == null)
                            {
                                // Create the item
                                itemModel = new ItemModel();
                                itemModel.hash = Constants.ITEM_HASH_BOTTLE_BEER_AM;
                                itemModel.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                                itemModel.ownerIdentifier = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                                itemModel.amount = Constants.ITEM_OPEN_BEER_AMOUNT;
                                itemModel.position = new Vector3(0.0f, 0.0f, 0.0f);
                                itemModel.dimension = player.Dimension;
                                itemModel.id = Database.AddNewItem(itemModel);
                                
                                itemList.Add(itemModel);
                            }
                            else
                            {
                                // Add the amount to the current item
                                itemModel.amount += Constants.ITEM_OPEN_BEER_AMOUNT;
                                Database.UpdateItem(item);
                            }
                            break;
                    }

                    // Substract container amount
                    SubstractPlayerItems(item);
                    
                    message = String.Format(Messages.INF_PLAYER_INVENTORY_OPEN, businessItem.description.ToLower());
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);

                    // Update the inventory
                    inventory = GetPlayerInventory(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_SELF);
                    break;
                case Messages.ARG_EQUIP:
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_OCCUPIED);
                    }
                    else
                    {
                        // Set the item into the hand
                        item.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                        item.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(item.hash), item.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)player.Dimension);
                        NAPI.Entity.AttachEntityToEntity(item.objectHandle, player, "PH_R_Hand", businessItem.position, businessItem.rotation);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_RIGHT_HAND, itemId);
                        
                        message = String.Format(Messages.INF_PLAYER_INVENTORY_EQUIP, businessItem.description.ToLower());
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    }
                    break;
                case Messages.COM_DROP:
                    item.amount--;
                    Database.UpdateItem(item);

                    // Check if there are items of the same type near
                    ItemModel closestItem = GetClosestItemWithHash(player, item.hash);

                    if (closestItem != null)
                    {
                        closestItem.amount++;
                        Database.UpdateItem(item);
                    }
                    else
                    {
                        closestItem = item.Copy();
                        closestItem.amount = 1;
                        closestItem.ownerEntity = Constants.ITEM_ENTITY_GROUND;
                        closestItem.dimension = player.Dimension;
                        closestItem.position = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 0.8f);
                        closestItem.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(closestItem.hash), closestItem.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)closestItem.dimension);
                        closestItem.id = Database.AddNewItem(closestItem);
                        itemList.Add(closestItem);
                    }

                    // Check if it was the last one
                    if (item.amount == 0)
                    {
                        Database.RemoveItem(item.id);
                        itemList.Remove(item);
                    }
                    
                    message = String.Format(Messages.INF_PLAYER_INVENTORY_DROP, businessItem.description.ToLower());
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);

                    // Update the inventory
                    inventory = GetPlayerInventory(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_SELF);
                    break;
                case Messages.ARG_CONFISCATE:
                    Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SEARCHED_TARGET);

                    // Transfer the item from the target to the player
                    item.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                    item.ownerIdentifier = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    Database.UpdateItem(item);
                    
                    String playerMessage = String.Format(Messages.INF_POLICE_RETIRED_ITEMS_TO, target.Name);
                    String targetMessage = String.Format(Messages.INF_POLICE_RETIRED_ITEMS_FROM, player.Name);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                    // Update the inventory
                    inventory = GetPlayerInventoryAndWeapons(target);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_PLAYER);
                    break;
                case Messages.ARG_STORE:
                    Vehicle targetVehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_OPENED_TRUNK);

                    // Transfer the item from the player to the vehicle
                    item.ownerEntity = Constants.ITEM_ENTITY_VEHICLE;
                    item.ownerIdentifier = NAPI.Data.GetEntityData(targetVehicle, EntityData.VEHICLE_ID);
                    Database.UpdateItem(item);

                    // Remove the weapon if it's a weapon
                    foreach (WeaponHash weapon in NAPI.Player.GetPlayerWeapons(player))
                    {
                        if (weapon.ToString() == item.hash)
                        {
                            NAPI.Player.RemovePlayerWeapon(player, weapon);
                            break;
                        }
                    }

                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TRUNK_STORED_ITEMS);

                    // Update the inventory
                    inventory = GetPlayerInventoryAndWeapons(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_VEHICLE_PLAYER);
                    break;
                case Messages.ARG_WITHDRAW:
                    Vehicle sourceVehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_OPENED_TRUNK);

                    WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);

                    if (weaponHash != 0)
                    {
                        // Give the weapon to the player
                        item.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
                        NAPI.Player.GivePlayerWeapon(player, weaponHash, 0);
                        NAPI.Player.SetPlayerWeaponAmmo(player, weaponHash, item.amount);
                    }
                    else
                    {
                        // Place the item into the inventory
                        item.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                    }

                    // Transfer the item from the vehicle to the player
                    item.ownerIdentifier = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    Database.UpdateItem(item);

                    Chat.SendMessageToNearbyPlayers(player, Messages.INF_TRUNK_ITEM_WITHDRAW, Constants.MESSAGE_ME, 20.0f);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_TRUNK_WITHDRAW_ITEMS);

                    // Update the inventory
                    inventory = GetVehicleTrunkInventory(sourceVehicle);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_VEHICLE_TRUNK);
                    break;
            }
        }

        [RemoteEvent("closeVehicleTrunk")]
        public void CloseVehicleTrunkEvent(Client player)
        {
            Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_OPENED_TRUNK);
            NAPI.Vehicle.SetVehicleDoorState(vehicle, Constants.VEHICLE_TRUNK, false);            
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_OPENED_TRUNK);
        }

        [RemoteEvent("getPlayerTattoos")]
        public void GetPlayerTattoosEvent(Client player, Client targetPlayer)
        {
            int targetId = NAPI.Data.GetEntityData(targetPlayer, EntityData.PLAYER_SQL_ID);
            List<TattooModel> playerTattooList = GetPlayerTattoos(targetId);
            NAPI.ClientEvent.TriggerClientEvent(player, "updatePlayerTattoos", NAPI.Util.ToJson(playerTattooList), targetPlayer);
        }

        [Command(Messages.COM_STORE)]
        public void StoreCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                ItemModel item = GetItemModelFromId(itemId);
                if (item.objectHandle.IsNull)
                {
                    NAPI.Player.GivePlayerWeapon(player, WeaponHash.Unarmed, 1);
                }
                else
                {
                    NAPI.Entity.DetachEntity(item.objectHandle);
                    NAPI.Entity.DeleteEntity(item.objectHandle);
                }
                item.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                NAPI.Data.ResetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                Database.UpdateItem(item);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_EMPTY);
            }
        }

        [Command(Messages.COM_CONSUME)]
        public void ConsumeCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                // Get the item in the right hand
                int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                ItemModel item = GetItemModelFromId(itemId);
                BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);

                // Check if it's consumable
                if (businessItem.type == Constants.ITEM_TYPE_CONSUMABLE)
                {
                    String message = String.Format(Messages.INF_PLAYER_INVENTORY_CONSUME, businessItem.description.ToLower());
                    
                    item.amount--;
                    Database.UpdateItem(item);
                    
                    if (businessItem.health != 0)
                    {
                        int health = NAPI.Player.GetPlayerHealth(player);
                        NAPI.Player.SetPlayerHealth(player, health + businessItem.health);
                    }
                    
                    if (businessItem.alcoholLevel > 0)
                    {
                        float currentAlcohol = 0;
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DRUNK_LEVEL) == true)
                        {
                            currentAlcohol = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DRUNK_LEVEL);
                        }
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_DRUNK_LEVEL, currentAlcohol + businessItem.alcoholLevel);
                        
                        if (currentAlcohol + businessItem.alcoholLevel > Constants.WASTED_LEVEL)
                        {
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_WALKING_STYLE, "move_m@drunk@verydrunk");
                            NAPI.ClientEvent.TriggerClientEventForAll("changePlayerWalkingStyle", player.Handle, "move_m@drunk@verydrunk");
                        }
                    }
                    
                    if (item.amount == 0)
                    {
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                        NAPI.Entity.DetachEntity(item.objectHandle);
                        NAPI.Entity.DeleteEntity(item.objectHandle);
                        Database.RemoveItem(item.id);
                        itemList.Remove(item);
                    }
                    
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ITEM_NOT_CONSUMABLE);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_EMPTY);
            }
        }

        [Command(Messages.COM_INVENTORY)]
        public void InventoryCommand(Client player)
        {
            if (GetPlayerInventoryTotal(player) > 0)
            {
                List<InventoryModel> inventory = GetPlayerInventory(player);
                NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerInventory", NAPI.Util.ToJson(inventory), Constants.INVENTORY_TARGET_SELF);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_ITEMS_INVENTORY);
            }
        }

        [Command(Messages.COM_PURCHASE)]
        public void PurchaseCommand(Client player, int amount = 0)
        {
            // Check if the player is inside a business
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) > 0)
            {
                int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                BusinessModel business = Business.GetBusinessById(businessId);
                
                switch (business.type)
                {
                    case Constants.BUSINESS_TYPE_CLOTHES:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ABOUT_COMPLEMENTS);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_FOR_AVOID_CLIPPING1);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_FOR_AVOID_CLIPPING2);
                        NAPI.ClientEvent.TriggerClientEvent(player, "showClothesBusinessPurchaseMenu", business.name, business.multiplier);
                        break;
                    case Constants.BUSINESS_TYPE_BARBER_SHOP:
                        NAPI.ClientEvent.TriggerClientEvent(player, "showHairdresserMenu", business.name);
                        break;
                    case Constants.BUSINESS_TYPE_TATTOO_SHOP:
                        int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        int playerSex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);

                        // Remove player's clothes
                        NAPI.Player.SetPlayerClothes(player, 11, 15, 0);
                        NAPI.Player.SetPlayerClothes(player, 3, 15, 0);
                        NAPI.Player.SetPlayerClothes(player, 8, 15, 0);
                        
                        if (playerSex == 0)
                        {
                            NAPI.Player.SetPlayerClothes(player, 4, 61, 0);
                            NAPI.Player.SetPlayerClothes(player, 6, 34, 0);
                        }
                        else
                        {
                            NAPI.Player.SetPlayerClothes(player, 4, 15, 0);
                            NAPI.Player.SetPlayerClothes(player, 6, 35, 0);
                        }

                        // Load tattoo list
                        List<TattooModel> tattooList = GetPlayerTattoos(playerId);

                        NAPI.ClientEvent.TriggerClientEvent(player, "showTattooMenu", NAPI.Util.ToJson(tattooList), NAPI.Util.ToJson(Constants.TATTOO_LIST), business.name, business.multiplier);
                        break;
                    default:
                        List<BusinessItemModel> businessItems = Business.GetBusinessSoldItems(business.type);
                        NAPI.ClientEvent.TriggerClientEvent(player, "showBusinessPurchaseMenu", NAPI.Util.ToJson(businessItems), business.name, business.multiplier);
                        break;
                }
            }
            else
            {
                // Get all the houses
                foreach (HouseModel house in House.houseList)
                {
                    if (player.Position.DistanceTo(house.position) <= 1.5f && player.Dimension == house.dimension)
                    {
                        House.BuyHouse(player, house);
                        return;
                    }
                }

                // Check if the player's in the scrapyard
                foreach (ParkingModel parking in Parking.parkingList)
                {
                    if (player.Position.DistanceTo(parking.position) < 2.5f && parking.type == Constants.PARKING_TYPE_SCRAPYARD)
                    {
                        if (amount > 0)
                        {
                            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                            if (playerMoney >= amount)
                            {
                                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                                ItemModel item = GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                                if (item == null)
                                {
                                    item = new ItemModel();
                                    item.amount = amount;
                                    item.dimension = 0;
                                    item.position = new Vector3(0.0f, 0.0f, 0.0f);
                                    item.hash = Constants.ITEM_HASH_BUSINESS_PRODUCTS;
                                    item.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                                    item.ownerIdentifier = playerId;
                                    item.objectHandle = null;
                                    item.id = Database.AddNewItem(item);
                                    itemList.Add(item);
                                }
                                else
                                {
                                    item.amount += amount;
                                    Database.UpdateItem(item);
                                }
                                
                                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - amount);
                                
                                String message = String.Format(Messages.INF_PRODUCTS_BOUGHT, amount, amount);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_COMMAND_PURCHASE);
                        }
                        return;
                    }
                }
            }

        }

        [Command(Messages.COM_SELL, Messages.GEN_SELL_COMMAND, GreedyArg = true)]
        public void SellCommand(Client player, String args)
        {
            String[] arguments = args.Split(' ');
            int price = 0;
            int targetId = 0;
            int objectId = 0;
            Client target = null;
            String priceString = String.Empty;
            if (arguments.Length > 0)
            {
                switch (arguments[0].ToLower())
                {
                    case Messages.ARG_VEHICLE:
                        if (arguments.Length > 3)
                        {
                            if (Int32.TryParse(arguments[2], out targetId) == true)
                            {
                                target = GetPlayerById(targetId);
                                priceString = arguments[3];
                            }
                            else if (arguments.Length == 5)
                            {
                                target = NAPI.Player.GetPlayerFromName(arguments[2] + " " + arguments[3]);
                                priceString = arguments[4];
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SELL_VEH_COMMAND);
                                return;
                            }
                            
                            if (Int32.TryParse(priceString, out price) == true)
                            {
                                if (price > 0)
                                {
                                    if (Int32.TryParse(arguments[1], out objectId) == true)
                                    {
                                        Vehicle vehicle = Vehicles.GetVehicleById(objectId);
                                        
                                        if (vehicle == null)
                                        {
                                            VehicleModel vehModel = Vehicles.GetParkedVehicleById(objectId);

                                            if (vehModel != null)
                                            {
                                                if (vehModel.owner == player.Name)
                                                {
                                                    String playerString = String.Format(Messages.INF_VEHICLE_SELL, vehModel.model, target.Name, price);
                                                    String targetString = String.Format(Messages.INF_VEHICLE_SOLD, player.Name, vehModel.model, price);

                                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_SELLING_PRICE, price);
                                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_SELLING_HOUSE, objectId);

                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerString);
                                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetString);
                                                }
                                                else
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_VEH_OWNER);
                                                }
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                                            }
                                        }
                                        else
                                        {
                                            foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                                            {
                                                if (NAPI.Data.GetEntityData(veh, EntityData.VEHICLE_ID) == objectId)
                                                {
                                                    if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_OWNER) == player.Name)
                                                    {
                                                        String vehicleModel = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                                                        String playerString = String.Format(Messages.INF_VEHICLE_SELL, vehicleModel, target.Name, price);
                                                        String targetString = String.Format(Messages.INF_VEHICLE_SOLD, player.Name, vehicleModel, price);

                                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_SELLING_PRICE, price);
                                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_SELLING_VEHICLE, objectId);

                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerString);
                                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetString);
                                                    }
                                                    else
                                                    {
                                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_VEH_OWNER);
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SELL_VEH_COMMAND);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PRICE_POSITIVE);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SELL_VEH_COMMAND);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SELL_VEH_COMMAND);
                        }
                        break;
                    case Messages.ARG_HOUSE:
                        if (arguments.Length < 2)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_SELL_HOUSE_COMMAND);
                        }
                        else
                        {
                            if (Int32.TryParse(arguments[1], out objectId) == true)
                            {
                                HouseModel house = House.GetHouseById(objectId);
                                if (house != null)
                                {
                                    if (house.owner == player.Name)
                                    {
                                        foreach (Client rndPlayer in NAPI.Pools.GetAllPlayers())
                                        {
                                            if (NAPI.Data.HasEntityData(rndPlayer, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(rndPlayer, EntityData.PLAYER_HOUSE_ENTERED) == house.id)
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_OCCUPIED);
                                                return;
                                            }
                                        }
                                        if (arguments.Length == 2)
                                        {
                                            int sellValue = (int)Math.Round(house.price * 0.7);
                                            String playerString = String.Format(Messages.INF_HOUSE_SELL_STATE, sellValue);
                                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_SELLING_HOUSE_STATE, objectId);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerString);
                                        }
                                        else
                                        {
                                            if (Int32.TryParse(arguments[2], out targetId) == true)
                                            {
                                                target = GetPlayerById(targetId);
                                                priceString = arguments[3];
                                            }
                                            else if (arguments.Length == 5)
                                            {
                                                target = NAPI.Player.GetPlayerFromName(arguments[2] + " " + arguments[3]);
                                                priceString = arguments[4];
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SELL_HOUSE_COMMAND);
                                                return;
                                            }

                                            if (Int32.TryParse(priceString, out price) == true)
                                            {
                                                if (price > 0)
                                                {
                                                    String playerString = String.Format(Messages.INF_HOUSE_SELL, target.Name, price);
                                                    String targetString = String.Format(Messages.INF_HOUSE_SOLD, player.Name, price);

                                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_SELLING_PRICE, price);
                                                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_SELLING_HOUSE, objectId);

                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerString);
                                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetString);
                                                }
                                                else
                                                {
                                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PRICE_POSITIVE);
                                                }
                                            }
                                            else
                                            {
                                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SELL_VEH_COMMAND);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_HOUSE_OWNER);
                                    }
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_NOT_EXISTS);
                                }

                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_SELL_HOUSE_COMMAND);
                            }
                        }
                        break;
                    case Messages.ARG_WEAPON:
                        // Pending TODO
                        break;
                    case Messages.ARG_FISH:
                        if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) > 0)
                        {
                            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                            BusinessModel business = Business.GetBusinessById(businessId);

                            if (business != null && business.type == Constants.BUSINESS_TYPE_FISHING)
                            {
                                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                                ItemModel fishModel = GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_FISH);

                                if (fishModel == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_FISH_SELLABLE);
                                }
                                else
                                {
                                    int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                                    int amount = (int)Math.Round(fishModel.amount * Constants.PRICE_FISH / 1000.0);
                                    
                                    Database.RemoveItem(fishModel.id);
                                    itemList.Remove(fishModel);
                                    
                                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney + amount);
                                    
                                    String message = String.Format(Messages.INF_FISHING_WON, amount);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_FISHING_BUSINESS);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_FISHING_BUSINESS);
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_SELL_COMMAND);
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_SELL_COMMAND);
            }
        }

        [Command(Messages.COM_HELP)]
        public void HelpCommand(Client player)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "helptext");
        }

        [Command(Messages.COM_WELCOME)]
        public void WelcomeCommand(Client player)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "welcomeHelp");
        }

        [Command(Messages.COM_SHOW, Messages.GEN_SHOW_DOC_COMMAND)]
        public void ShowCommand(Client player, String targetString, String documentation)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                int currentLicense = 0;
                String message = String.Empty;
                String nameChar = NAPI.Data.GetEntityData(player, EntityData.PLAYER_NAME);
                int age = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_AGE);
                String sexDescription = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_MALE ? Messages.GEN_SEX_MALE : Messages.GEN_SEX_FEMALE;

                Client target = Int32.TryParse(targetString, out int targetId) ? GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                switch (documentation.ToLower())
                {
                    case Messages.ARG_LICENSES:
                        String licenseMessage = String.Empty;
                        String playerLicenses = NAPI.Data.GetEntityData(player, EntityData.PLAYER_LICENSES);
                        String[] playerLicensesArray = playerLicenses.Split(',');

                        message = String.Format(Messages.INF_LICENSES_SHOW, target.Name);
                        Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_ME, 20.0f);

                        foreach (String license in playerLicensesArray)
                        {
                            int currentLicenseStatus = Int32.Parse(license);
                            switch (currentLicense)
                            {
                                case Constants.LICENSE_CAR:
                                    switch (currentLicenseStatus)
                                    {
                                        case -1:
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + Messages.INF_CAR_LICENSE_NOT_AVAILABLE);
                                            break;
                                        case 0:
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + Messages.INF_CAR_LICENSE_PRACTICAL_PENDING);
                                            break;
                                        default:
                                            licenseMessage = String.Format(Messages.INF_CAR_LICENSE_POINTS, currentLicenseStatus);
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + licenseMessage);
                                            break;
                                    }
                                    break;
                                case Constants.LICENSE_MOTORCYCLE:
                                    switch (currentLicenseStatus)
                                    {
                                        case -1:
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + Messages.INF_MOTORCYCLE_LICENSE_NOT_AVAILABLE);
                                            break;
                                        case 0:
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + Messages.INF_MOTORCYCLE_LICENSE_PRACTICAL_PENDING);
                                            break;
                                        default:
                                            licenseMessage = String.Format(Messages.INF_MOTORCYCLE_LICENSE_POINTS, currentLicenseStatus);
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + licenseMessage);
                                            break;
                                    }
                                    break;
                                case Constants.LICENSE_TAXI:
                                    if (currentLicenseStatus == -1)
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + Messages.INF_TAXI_LICENSE_NOT_AVAILABLE);
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_HELP + Messages.INF_TAXI_LICENSE_UP_TO_DATE);
                                    }
                                    break;
                            }
                            currentLicense++;
                        }
                        break;
                    case Messages.ARG_INSURANCE:
                        int playerMedicalInsurance = NAPI.Data.GetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE);
                        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        dateTime = dateTime.AddSeconds(playerMedicalInsurance);

                        if (playerMedicalInsurance > 0)
                        {
                            message = String.Format(Messages.INF_INSURANCE_SHOW, target.Name);
                            Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_ME, 20.0f);
                            
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.GEN_NAME + nameChar);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.GEN_AGE + age);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.GEN_SEX + sexDescription);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.GEN_EXPIRY + dateTime.ToShortDateString());
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_MEDICAL_INSURANCE);
                        }

                        break;
                    case Messages.ARG_IDENTIFICATION:
                        int playerDocumentation = NAPI.Data.GetEntityData(player, EntityData.PLAYER_DOCUMENTATION);
                        if (playerDocumentation > 0)
                        {
                            message = String.Format(Messages.INF_IDENTIFICATION_SHOW, target.Name);
                            Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_ME, 20.0f);

                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.GEN_NAME + nameChar);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.GEN_AGE + age);
                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + Messages.GEN_SEX + sexDescription);
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_UNDOCUMENTED);
                        }
                        break;
                }
            }
        }

        [Command(Messages.COM_PAY, Messages.GEN_PAY_COMMAND)]
        public void PayCommand(Client player, String targetString, int price)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);
                if (target == player)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOOKER_OFFERED_HIMSELF);
                }
                else
                {
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_PAYMENT, player);
                    NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, price);

                    String playerMessage = String.Format(Messages.INF_PAYMENT_OFFER, price, target.Name);
                    String targetMessage = String.Format(Messages.INF_PAYMENT_RECEIVED, player.Name, price);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                }
            }
        }

        [Command(Messages.COM_GIVE, Messages.GEN_GIVE_COMMAND)]
        public void GiveCommand(Client player, String targetString)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                Client target = Int32.TryParse(targetString, out int targetId) ? GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                }
                else if (player.Position.DistanceTo(target.Position) > 2.0f)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                }
                else if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_RIGHT_HAND) == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_TARGET_RIGHT_HAND_NOT_EMPTY);
                }
                else
                {
                    String playerMessage = String.Empty;
                    String targetMessage = String.Empty;

                    int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                    ItemModel item = GetItemModelFromId(itemId);

                    // Check if it's a weapon
                    WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);

                    if (weaponHash != 0)
                    {
                        NAPI.Player.GivePlayerWeapon(target, weaponHash, 0);
                        NAPI.Player.SetPlayerWeaponAmmo(target, weaponHash, item.amount);
                        NAPI.Player.RemovePlayerWeapon(player, weaponHash);
                        
                        playerMessage = String.Format(Messages.INF_ITEM_GIVEN, item.hash.ToLower(), target.Name);
                        targetMessage = String.Format(Messages.INF_ITEM_RECEIVED, player.Name, item.hash.ToLower());
                    }
                    else
                    {
                        BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);
                        NAPI.Entity.DetachEntity(item.objectHandle);
                        NAPI.Entity.AttachEntityToEntity(item.objectHandle, target, "PH_R_Hand", businessItem.position, businessItem.rotation);
                        
                        playerMessage = String.Format(Messages.INF_ITEM_GIVEN, businessItem.description.ToLower(), target.Name);
                        targetMessage = String.Format(Messages.INF_ITEM_RECEIVED, player.Name, businessItem.description.ToLower());
                    }

                    // Change item's owner
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                    NAPI.Data.SetEntityData(target, EntityData.PLAYER_RIGHT_HAND, item.id);
                    item.ownerIdentifier = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
                    Database.UpdateItem(item);
                    
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_EMPTY);
            }
        }

        [Command(Messages.COM_CANCEL, Messages.GEN_GLOBALS_CANCEL_COMMAND)]
        public void CancelCommand(Client player, String cancel)
        {
            switch (cancel.ToLower())
            {
                case Messages.ARG_INTERVIEW:
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ON_AIR) == true)
                    {
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ON_AIR);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ON_AIR_CANCELED);
                    }
                    break;
                case Messages.ARG_SERVICE:
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ALREADY_FUCKING) == false)
                    {
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ALREADY_FUCKING);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                        NAPI.Data.ResetEntityData(player, EntityData.HOOKER_TYPE_SERVICE);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_HOOKER_SERVICE_CANCELED);
                    }
                    break;
                case Messages.ARG_MONEY:
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PAYMENT) == true)
                    {
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_PAYMENT);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PAYMENT_CANCELED);
                    }
                    break;
                case Messages.ARG_ORDER:
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_DELIVER_ORDER) == true)
                    {
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_DELIVER_ORDER);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_CHECKPOINT);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_VEHICLE);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_WON);

                        // Remove the checkpoints
                        NAPI.ClientEvent.TriggerClientEvent(player, "fastFoodDeliverFinished");

                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_DELIVERER_ORDER_CANCELED);
                    }
                    break;
                case Messages.ARG_REPAINT:
                    if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_REPAINT_VEHICLE) == true)
                    {
                        // Get the mechanic and the vehicle
                        Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                        Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAINT_VEHICLE);

                        // Get old colors
                        int vehicleColorType = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE);
                        String primaryVehicleColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR);
                        String secondaryVehicleColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR);
                        int vehiclePearlescentColor = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR);

                        if (vehicleColorType == Constants.VEHICLE_COLOR_TYPE_PREDEFINED)
                        {
                            NAPI.Vehicle.SetVehiclePrimaryColor(vehicle, Int32.Parse(primaryVehicleColor));
                            NAPI.Vehicle.SetVehicleSecondaryColor(vehicle, Int32.Parse(secondaryVehicleColor));
                            NAPI.Vehicle.SetVehiclePearlescentColor(vehicle, vehiclePearlescentColor);
                        }
                        else
                        {
                            String[] primaryColor = primaryVehicleColor.Split(',');
                            String[] secondaryColor = secondaryVehicleColor.Split(',');
                            NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, Int32.Parse(primaryColor[0]), Int32.Parse(primaryColor[1]), Int32.Parse(primaryColor[2]));
                            NAPI.Vehicle.SetVehicleCustomSecondaryColor(vehicle, Int32.Parse(secondaryColor[0]), Int32.Parse(secondaryColor[1]), Int32.Parse(secondaryColor[2]));
                        }
                        
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_VEHICLE);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_COLOR_TYPE);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_FIRST_COLOR);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_SECOND_COLOR);
                        NAPI.Data.ResetEntityData(player, EntityData.JOB_OFFER_PRICE);

                        // Remove repaint window
                        NAPI.ClientEvent.TriggerClientEvent(target, "closeRepaintWindow");
                        
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_REPAINT_CANCELED);
                    }
                    break;
                default:
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_GLOBALS_CANCEL_COMMAND);
                    break;
            }
        }

        [Command(Messages.COM_ACCEPT, Messages.GEN_GLOBALS_ACCEPT_COMMAND)]
        public void AcceptCommand(Client player, String accept)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else
            {
                switch (accept.ToLower())
                {
                    case Messages.ARG_REPAIR:
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_REPAIR_VEHICLE) == true)
                        {
                            Client mechanic = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);

                            if (mechanic != null && mechanic.Position.DistanceTo(player.Position) < 5.0f)
                            {
                                int price = NAPI.Data.GetEntityData(player, EntityData.JOB_OFFER_PRICE);
                                int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

                                if (playerMoney >= price)
                                {
                                    // Get the vehicle to repair and the broken part
                                    String type = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAIR_TYPE);
                                    Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAIR_VEHICLE);
                                    
                                    int mechanicId = NAPI.Data.GetEntityData(mechanic, EntityData.PLAYER_SQL_ID);
                                    int mechanicMoney = NAPI.Data.GetEntitySharedData(mechanic, EntityData.PLAYER_MONEY);
                                    ItemModel item = GetPlayerItemModelFromHash(mechanicId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                                    switch (type.ToLower())
                                    {
                                        case Messages.ARG_CHASSIS:
                                            NAPI.Vehicle.RepairVehicle(vehicle);
                                            break;
                                        case Messages.ARG_DOORS:
                                            for (int i = 0; i < 6; i++)
                                            {
                                                if (NAPI.Vehicle.IsVehicleDoorBroken(vehicle, i) == true)
                                                {
                                                    NAPI.Vehicle.BreakVehicleDoor(vehicle, i, false);
                                                }
                                            }
                                            break;
                                        case Messages.ARG_TYRES:
                                            for (int i = 0; i < 4; i++)
                                            {
                                                if (NAPI.Vehicle.IsVehicleTyrePopped(vehicle, i) == true)
                                                {
                                                    NAPI.Vehicle.PopVehicleTyre(vehicle, i, false);
                                                }
                                            }
                                            break;
                                        case Messages.ARG_WINDOWS:
                                            for (int i = 0; i < 4; i++)
                                            {
                                                if (NAPI.Vehicle.IsVehicleWindowBroken(vehicle, i) == true)
                                                {
                                                    NAPI.Vehicle.BreakVehicleWindow(vehicle, i, false);
                                                }
                                            }
                                            break;
                                    }
                                    
                                    if (player != mechanic)
                                    {
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);
                                        NAPI.Data.SetEntitySharedData(mechanic, EntityData.PLAYER_MONEY, mechanicMoney + price);
                                    }
                                    item.amount -= NAPI.Data.GetEntityData(player, EntityData.JOB_OFFER_PRODUCTS);
                                    Database.UpdateItem(item);
                                    
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAIR_VEHICLE);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAIR_TYPE);
                                    NAPI.Data.ResetEntityData(player, EntityData.JOB_OFFER_PRODUCTS);
                                    NAPI.Data.ResetEntityData(player, EntityData.JOB_OFFER_PRICE);
                                    
                                    String playerMessage = String.Format(Messages.INF_VEHICLE_REPAIRED_BY, mechanic.Name, price);
                                    String mechanicMessage = String.Format(Messages.INF_VEHICLE_REPAIRED_BY, player.Name, price);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                    NAPI.Chat.SendChatMessageToPlayer(mechanic, Constants.COLOR_INFO + mechanicMessage);

                                    // Save the log into the database
                                    Database.LogPayment(player.Name, mechanic.Name, Messages.COM_REPAIR, price);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_REPAIR_OFFERED);

                        }
                        break;
                    case Messages.ARG_REPAINT:
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_REPAINT_VEHICLE) == true)
                        {
                            Client mechanic = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);

                            if (mechanic != null && mechanic.Position.DistanceTo(player.Position) < 5.0f)
                            {
                                int price = NAPI.Data.GetEntityData(player, EntityData.JOB_OFFER_PRICE);
                                int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

                                if (playerMoney >= price)
                                {
                                    Vehicle vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAINT_VEHICLE);
                                    int colorType = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAINT_COLOR_TYPE);
                                    String firstColor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAINT_FIRST_COLOR);
                                    String secondColor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAINT_SECOND_COLOR);
                                    int pearlescentColor = NAPI.Data.GetEntityData(player, EntityData.PLAYER_REPAINT_PEARLESCENT);
                                    
                                    int mechanicId = NAPI.Data.GetEntityData(mechanic, EntityData.PLAYER_SQL_ID);
                                    int mechanicMoney = NAPI.Data.GetEntitySharedData(mechanic, EntityData.PLAYER_MONEY);
                                    ItemModel item = GetPlayerItemModelFromHash(mechanicId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                                    // Repaint the vehicle
                                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_COLOR_TYPE, colorType);
                                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_FIRST_COLOR, firstColor);
                                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_SECOND_COLOR, secondColor);
                                    NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_PEARLESCENT_COLOR, pearlescentColor);

                                    // Update the vehicle's color
                                    VehicleModel vehicleModel = new VehicleModel();
                                    vehicleModel.id = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                                    vehicleModel.colorType = colorType;
                                    vehicleModel.firstColor = firstColor;
                                    vehicleModel.secondColor = secondColor;
                                    vehicleModel.pearlescent = pearlescentColor;
                                    Database.UpdateVehicleColor(vehicleModel);
                                    
                                    if (player != mechanic)
                                    {
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);
                                        NAPI.Data.SetEntitySharedData(mechanic, EntityData.PLAYER_MONEY, mechanicMoney + price);
                                    }
                                    item.amount -= NAPI.Data.GetEntityData(player, EntityData.JOB_OFFER_PRODUCTS);
                                    Database.UpdateItem(item);
                                    
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_VEHICLE);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_COLOR_TYPE);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_FIRST_COLOR);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_REPAINT_SECOND_COLOR);
                                    NAPI.Data.ResetEntityData(player, EntityData.JOB_OFFER_PRODUCTS);
                                    NAPI.Data.ResetEntityData(player, EntityData.JOB_OFFER_PRICE);
                                    
                                    String playerMessage = String.Format(Messages.INF_VEHICLE_REPAINTED_BY, mechanic.Name, price);
                                    String mechanicMessage = String.Format(Messages.INF_VEHICLE_REPAINTED_TO, player.Name, price);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                    NAPI.Chat.SendChatMessageToPlayer(mechanic, Constants.COLOR_INFO + mechanicMessage);

                                    // Remove repaint menu
                                    NAPI.ClientEvent.TriggerClientEvent(mechanic, "closeRepaintWindow");

                                    // Save the log into the database
                                    Database.LogPayment(player.Name, mechanic.Name, Messages.COM_REPAINT, price);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                            }
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_REPAIR_OFFERED);
                        }

                        break;
                    case Messages.ARG_SERVICE:
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_ALREADY_FUCKING) == true)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ALREADY_FUCKING);
                        }
                        else if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.Driver)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VEHICLE_DRIVING);
                        }
                        else
                        {
                            NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);

                            if (NAPI.Vehicle.GetVehicleEngineStatus(vehicle) == true)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ENGINE_ON);
                            }
                            else
                            {
                                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                                if (NAPI.Data.HasEntityData(player, EntityData.HOOKER_TYPE_SERVICE) == true)
                                {
                                    int amount = NAPI.Data.GetEntityData(player, EntityData.JOB_OFFER_PRICE);
                                    int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

                                    if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                                    {
                                        if (amount > money)
                                        {
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                                        }
                                        else
                                        {
                                            int targetMoney = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY);
                                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - amount);
                                            NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_MONEY, targetMoney + amount);

                                            String playerMessage = String.Format(Messages.INF_SERVICE_PAID, amount);
                                            String targetMessage = String.Format(Messages.INF_SERVICE_RECEIVED, amount);
                                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                            NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ANIMATION, target);
                                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ALREADY_FUCKING, target);
                                            NAPI.Data.SetEntityData(target, EntityData.PLAYER_ALREADY_FUCKING, player);

                                            // Reset the entity data
                                            NAPI.Data.ResetEntityData(player, EntityData.JOB_OFFER_PRICE);
                                            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_PARTNER);

                                            // Check the type of the service
                                            if (NAPI.Data.GetEntityData(player, EntityData.HOOKER_TYPE_SERVICE) == Constants.HOOKER_SERVICE_BASIC)
                                            {
                                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexlow_veh", "low_car_bj_loop_player");
                                                NAPI.Player.PlayPlayerAnimation(target, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexlow_veh", "low_car_bj_loop_female");

                                                // Timer to finish the service
                                                Timer sexTimer = new Timer(Hooker.OnSexServiceTimer, player, 120000, Timeout.Infinite);
                                                Hooker.sexTimerList.Add(player.Value, sexTimer);
                                            }
                                            else
                                            {
                                                NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexlow_veh", "low_car_sex_loop_player");
                                                NAPI.Player.PlayPlayerAnimation(target, (int)(Constants.AnimationFlags.Loop), "mini@prostitutes@sexlow_veh", "low_car_sex_loop_female");

                                                // Timer to finish the service
                                                Timer sexTimer = new Timer(Hooker.OnSexServiceTimer, player, 180000, Timeout.Infinite);
                                                Hooker.sexTimerList.Add(player.Value, sexTimer);
                                            }

                                            // Save payment log
                                            Database.LogPayment(player.Name, target.Name, Messages.GEN_HOOKER, amount);
                                        }
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                                    }
                                }
                            }
                        }
                        break;
                    case Messages.ARG_INTERVIEW:
                        if (NAPI.Player.IsPlayerInAnyVehicle(player) == false)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
                        }
                        else
                        {
                            NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                            if (NAPI.Player.GetPlayerVehicleSeat(player) != (int)VehicleSeat.RightRear)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_RIGHT_REAR);
                            }
                            else
                            {
                                Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                                NAPI.Data.SetEntityData(player, EntityData.PLAYER_ON_AIR, true);
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ALREADY_ON_AIR);
                                NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_SUCCESS + Messages.SUC_INTERVIEW_ACCEPTED);
                            }
                        }
                        break;
                    case Messages.ARG_MONEY:
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PAYMENT) == true)
                        {
                            Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_PAYMENT);
                            int amount = NAPI.Data.GetEntityData(player, EntityData.JOB_OFFER_PRICE);

                            if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                            {
                                int money = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_MONEY);

                                if (amount > 0 && money >= amount)
                                {
                                    int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney + amount);
                                    NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_MONEY, money - amount);

                                    // Reset the entity data
                                    NAPI.Data.ResetEntityData(player, EntityData.JOB_OFFER_PRICE);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_PAYMENT);

                                    // Send the messages to both players
                                    String playerMessage = String.Format(Messages.INF_PLAYER_PAID, target.Name, amount);
                                    String targetMessage = String.Format(Messages.INF_TARGET_PAID, amount, player.Name);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);

                                    // Save the logs into database
                                    Database.LogPayment(target.Name, player.Name, Messages.GEN_PAYMENT_PLAYERS, amount);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                            }
                        }
                        break;
                    case Messages.ARG_VEHICLE:
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_SELLING_VEHICLE) == true)
                        {
                            Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                            int amount = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SELLING_PRICE);
                            int vehicleId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SELLING_VEHICLE);

                            if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                            {
                                int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK);

                                if (money >= amount)
                                {
                                    String vehicleModel = String.Empty;
                                    Vehicle vehicle = Vehicles.GetVehicleById(vehicleId);
                                    
                                    if (vehicle == null)
                                    {
                                        VehicleModel vehModel = Vehicles.GetParkedVehicleById(vehicleId);
                                        vehModel.owner = player.Name;
                                        vehicleModel = vehModel.model;
                                    }
                                    else
                                    {
                                        NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_OWNER, player.Name);
                                        vehicleModel = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_MODEL);
                                    }
                                    
                                    int targetMoney = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_BANK);
                                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, money - amount);
                                    NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_BANK, targetMoney + amount);
                                    
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_SELLING_VEHICLE);
                                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_SELLING_PRICE);

                                    String playerString = String.Format(Messages.INF_VEHICLE_BUY, target.Name, vehicleModel, amount);
                                    String targetString = String.Format(Messages.INF_VEHICLE_BOUGHT, player.Name, vehicleModel, amount);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerString);
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetString);

                                    // Logeamos el pago en base de datos
                                    Database.LogPayment(target.Name, player.Name, Messages.GEN_VEHICLE_SALE, amount);
                                }
                                else
                                {
                                    String message = String.Format(Messages.ERR_CARSHOP_NO_MONEY, amount);
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_FOUND);
                            }
                        }
                        break;
                    case Messages.ARG_HOUSE:
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_SELLING_HOUSE) == true)
                        {
                            Client target = NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_PARTNER);
                            int amount = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SELLING_PRICE);
                            int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SELLING_HOUSE);

                            if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) == true)
                            {
                                int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK);

                                if (money >= amount)
                                {
                                    HouseModel house = House.GetHouseById(houseId);
                                    
                                    if (house.owner == target.Name)
                                    {
                                        house.owner = player.Name;
                                        Database.KickTenantsOut(house.id);
                                        house.tenants = 2;
                                        Database.UpdateHouse(house);
                                        
                                        int targetMoney = NAPI.Data.GetEntitySharedData(target, EntityData.PLAYER_BANK);
                                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, money - amount);
                                        NAPI.Data.SetEntitySharedData(target, EntityData.PLAYER_BANK, targetMoney + amount);
                                        
                                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_SELLING_HOUSE);
                                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_SELLING_PRICE);
                                        
                                        String playerString = String.Format(Messages.INF_HOUSE_BUYTO, target.Name, amount);
                                        String targetString = String.Format(Messages.INF_HOUSE_BOUGHT, player.Name, amount);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerString);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetString);

                                        // Log the payment into database
                                        Database.LogPayment(target.Name, player.Name, Messages.GEN_HOUSE_SALE, amount);
                                    }
                                    else
                                    {
                                        NAPI.Chat.SendChatMessageToPlayer(player, Messages.ERR_HOUSE_SELL_GENERIC);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Messages.ERR_HOUSE_SELL_GENERIC);
                                    }
                                }
                                else
                                {
                                    String message = String.Format(Messages.ERR_CARSHOP_NO_MONEY, amount);
                                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ERROR + message);
                                }
                            }
                        }
                        break;
                    case Messages.ARG_STATE_HOUSE:
                        if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_SELLING_HOUSE_STATE) == true)
                        {
                            HouseModel house = House.GetHouseById(NAPI.Data.GetEntityData(player, EntityData.PLAYER_SELLING_HOUSE_STATE));
                            int amount = (int)Math.Round(house.price * Constants.HOUSE_SALE_STATE);

                            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
                            {
                                if (house.owner == player.Name)
                                {
                                    house.locked = true;
                                    house.owner = String.Empty;
                                    house.status = Constants.HOUSE_STATE_BUYABLE;
                                    NAPI.TextLabel.SetTextLabelText(house.houseLabel, House.GetHouseLabelText(house));
                                    NAPI.World.RemoveIpl(house.ipl);
                                    Database.KickTenantsOut(house.id);
                                    house.tenants = 2;
                                    Database.UpdateHouse(house);
                                    
                                    int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK);
                                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, playerMoney + amount);
                                    
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + String.Format(Messages.SUC_HOUSE_SOLD, amount));

                                    // Log the payment into the database
                                    Database.LogPayment(player.Name, Messages.GEN_STATE, Messages.GEN_HOUSE_SALE, amount);
                                }
                                else
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.ERR_HOUSE_SELL_GENERIC);
                                }
                            }
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.GEN_GLOBALS_ACCEPT_COMMAND);
                        break;
                }
            }
        }

        [Command(Messages.COM_PICK_UP)]
        public void PickUpCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_OCCUPIED);
            }
            else if (NAPI.Data.HasEntitySharedData(player, EntityData.PLAYER_WEAPON_CRATE) == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_BOTH_HAND_OCCUPIED);
            }
            else
            {
                ItemModel item = GetClosestItem(player);
                if (item != null)
                {
                    // Get the item on the ground
                    ItemModel playerItem = GetPlayerItemModelFromHash(player.Value, item.hash);
                    
                    NAPI.Entity.DeleteEntity(item.objectHandle);

                    if (playerItem != null)
                    {
                        NAPI.Entity.DeleteEntity(item.objectHandle);
                        playerItem.amount += item.amount;
                        Database.RemoveItem(item.id);
                        itemList.Remove(item);
                    }
                    else
                    {
                        playerItem = item;
                    }

                    // Get the new owner of the item
                    playerItem.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                    playerItem.ownerIdentifier = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    Database.UpdateItem(playerItem);
                    
                    // Play the animation
                    NAPI.Player.PlayPlayerAnimation(player, 0, "random@domestic", "pickup_low");
                    
                    BusinessItemModel businessItem = Business.GetBusinessItemFromHash(playerItem.hash);
                    playerItem.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(playerItem.hash), playerItem.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)playerItem.dimension);
                    NAPI.Entity.AttachEntityToEntity(playerItem.objectHandle, player, "PH_R_Hand", businessItem.position, businessItem.rotation);
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_RIGHT_HAND, playerItem.id);

                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_PLAYER_PICKED_ITEM);
                }
                else
                {
                    WeaponCrateModel weaponCrate = Weapons.GetClosestWeaponCrate(player);
                    if (weaponCrate != null)
                    {
                        int index = Weapons.weaponCrateList.IndexOf(weaponCrate);
                        weaponCrate.carriedEntity = Constants.ITEM_ENTITY_PLAYER;
                        weaponCrate.carriedIdentifier = player.Value;
                        NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.Loop | Constants.AnimationFlags.OnlyAnimateUpperBody | Constants.AnimationFlags.AllowPlayerControl), "anim@heists@box_carry@", "idle");
                        NAPI.Entity.AttachEntityToEntity(weaponCrate.crateObject, player, "PH_R_Hand", new Vector3(0.0f, -0.5f, -0.25f), new Vector3(0.0f, 0.0f, 0.0f));
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_WEAPON_CRATE, index);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_ITEMS_NEAR);
                    }
                }
            }
        }

        [Command(Messages.COM_DROP)]
        public void DropCommand(Client player)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
            {
                int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                ItemModel item = GetItemModelFromId(itemId);
                BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);
                
                item.amount--;
                Database.UpdateItem(item);
                
                ItemModel closestItem = GetClosestItemWithHash(player, item.hash);

                if (closestItem != null)
                {
                    closestItem.amount++;
                    Database.UpdateItem(item);
                }
                else
                {
                    closestItem = item.Copy();
                    closestItem.amount = 1;
                    closestItem.ownerEntity = Constants.ITEM_ENTITY_GROUND;
                    closestItem.dimension = player.Dimension;
                    closestItem.position = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 0.8f);
                    closestItem.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(closestItem.hash), closestItem.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)closestItem.dimension);
                    closestItem.id = Database.AddNewItem(closestItem);
                    itemList.Add(closestItem);
                }
                
                if (item.amount == 0)
                {
                    // Remove the item from the hand
                    NAPI.Entity.DetachEntity(item.objectHandle);
                    NAPI.Entity.DeleteEntity(item.objectHandle);
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                    
                    // Remove the item
                    Database.RemoveItem(item.id);
                    itemList.Remove(item);
                }
                
                String message = String.Format(Messages.INF_PLAYER_INVENTORY_DROP, businessItem.description.ToLower());
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
            }
            else if (NAPI.Data.HasEntitySharedData(player, EntityData.PLAYER_WEAPON_CRATE) == true)
            {
                WeaponCrateModel weaponCrate = Weapons.GetPlayerCarriedWeaponCrate(player.Value);

                if (weaponCrate != null)
                {
                    weaponCrate.position = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 1.0f);
                    weaponCrate.carriedEntity = String.Empty;
                    weaponCrate.carriedIdentifier = 0;

                    // Place the crate on the ground
                    NAPI.Entity.DetachEntity(weaponCrate.crateObject);
                    NAPI.Entity.SetEntityPosition(weaponCrate.crateObject, weaponCrate.position);
                    
                    String message = String.Format(Messages.INF_PLAYER_INVENTORY_DROP, Messages.GEN_WEAPON_CRATE);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_RIGHT_HAND_EMPTY);
            }
        }

        [Command(Messages.COM_TICKET, Messages.GEN_HELP_REQUEST, GreedyArg = true)]
        public void TicketCommand(Client player, String message)
        {
            foreach (AdminTicketModel ticket in adminTicketList)
            {
                if (player.Value == ticket.playerId)
                {
                    ticket.question = message;
                    return;
                }
            }

            // Create a new ticket
            AdminTicketModel adminTicket = new AdminTicketModel();
            adminTicket.playerId = player.Value;
            adminTicket.question = message;
            adminTicketList.Add(adminTicket);

            // Send the message to the staff online
            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(target, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(target, EntityData.PLAYER_ADMIN_RANK) > 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_ADMIN_INFO + Messages.ADM_NEW_ADMIN_TICKET);
                }
                else if (target == player)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_HELP_REQUEST_SENT);
                }
            }
        }

        [Command(Messages.COM_DOOR)]
        public void DoorCommand(Client player)
        {
            // Check if the player's in his house
            foreach (HouseModel house in House.houseList)
            {
                if ((player.Position.DistanceTo(house.position) <= 1.5f && player.Dimension == house.dimension) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) == house.id)
                {
                    if (House.HasPlayerHouseKeys(player, house) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_HOUSE_OWNER);
                    }
                    else
                    {
                        house.locked = !house.locked;
                        Database.UpdateHouse(house);
                        
                        NAPI.Chat.SendChatMessageToPlayer(player, house.locked ? Constants.COLOR_INFO + Messages.INF_HOUSE_LOCKED : Constants.COLOR_INFO + Messages.INF_HOUSE_OPENED);
                    }
                    return;
                }
            }

            // Check if the player's in his business
            foreach (BusinessModel business in Business.businessList)
            {
                if ((player.Position.DistanceTo(business.position) <= 1.5f && player.Dimension == business.dimension) || NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED) == business.id)
                {
                    if (Business.HasPlayerBusinessKeys(player, business) == false)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_BUSINESS_OWNER);
                    }
                    else
                    {
                        business.locked = !business.locked;
                        Database.UpdateBusiness(business);
                        
                        NAPI.Chat.SendChatMessageToPlayer(player, business.locked ? Constants.COLOR_INFO + Messages.INF_BUSINESS_LOCKED : Constants.COLOR_INFO + Messages.INF_BUSINESS_OPENED);
                    }
                    return;
                }
            }

            // He's not in any house or business
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_HOUSE_BUSINESS);
        }

        [Command(Messages.COM_COMPLEMENT, Messages.GEN_COMPLEMENT_COMMAND)]
        public void ComplementCommand(Client player, String type, String action)
        {
            ClothesModel clothes = null;
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

            if (action.ToLower() == Messages.ARG_WEAR || action.ToLower() == Messages.ARG_REMOVE)
            {
                switch (type.ToLower())
                {
                    case Messages.ARG_MASK:
                        clothes = GetDressedClothesInSlot(playerId, 0, Constants.CLOTHES_MASK);
                        if (action.ToLower() == Messages.ARG_WEAR)
                        {
                            if (clothes == null)
                            {
                                clothes = GetPlayerClothes(playerId).Where(c => c.slot == Constants.CLOTHES_MASK && c.type == 0).First();
                                if (clothes == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_MASK_BOUGHT);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerClothes(player, clothes.slot, clothes.drawable, clothes.texture);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_MASK_EQUIPED);
                            }
                        }
                        else
                        {
                            if (clothes == null)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_MASK_EQUIPED);
                            }
                            else
                            {
                                NAPI.Player.SetPlayerClothes(player, Constants.CLOTHES_MASK, 0, 0);
                                UndressClothes(playerId, 0, Constants.CLOTHES_MASK);
                            }
                        }
                        break;
                    case Messages.ARG_BAG:
                        clothes = GetDressedClothesInSlot(playerId, 0, Constants.CLOTHES_BAGS);
                        if (action.ToLower() == Messages.ARG_WEAR)
                        {
                            if (clothes == null)
                            {
                                clothes = GetPlayerClothes(playerId).Where(c => c.slot == Constants.CLOTHES_BAGS && c.type == 0).First();
                                if (clothes == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_BAG_BOUGHT);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerClothes(player, clothes.slot, clothes.drawable, clothes.texture);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_BAG_EQUIPED);
                            }
                        }
                        else
                        {
                            if (clothes == null)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_BAG_EQUIPED);
                            }
                            else
                            {
                                NAPI.Player.SetPlayerClothes(player, Constants.CLOTHES_BAGS, 0, 0);
                                UndressClothes(playerId, 0, Constants.CLOTHES_BAGS);
                            }
                        }
                        break;
                    case Messages.ARG_ACCESSORY:
                        clothes = GetDressedClothesInSlot(playerId, 0, Constants.CLOTHES_ACCESSORIES);
                        if (action.ToLower() == Messages.ARG_WEAR)
                        {
                            if (clothes == null)
                            {
                                clothes = GetPlayerClothes(playerId).Where(c => c.slot == Constants.CLOTHES_ACCESSORIES && c.type == 0).First();
                                if (clothes == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_ACCESSORY_BOUGHT);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerClothes(player, clothes.slot, clothes.drawable, clothes.texture);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_ACCESSORY_EQUIPED);
                            }
                        }
                        else
                        {
                            if (clothes == null)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_ACCESSORY_EQUIPED);
                            }
                            else
                            {
                                NAPI.Player.SetPlayerClothes(player, Constants.CLOTHES_ACCESSORIES, 0, 0);
                                UndressClothes(playerId, 0, Constants.CLOTHES_ACCESSORIES);
                            }
                        }
                        break;
                    case Messages.ARG_HAT:
                        clothes = GetDressedClothesInSlot(playerId, 1, Constants.ACCESSORY_HATS);
                        if (action.ToLower() == Messages.ARG_WEAR)
                        {
                            if (clothes == null)
                            {
                                clothes = GetPlayerClothes(playerId).Where(c => c.slot == Constants.ACCESSORY_HATS && c.type == 1).First();
                                if (clothes == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_HAT_BOUGHT);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerAccessory(player, clothes.slot, clothes.drawable, clothes.texture);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HAT_EQUIPED);
                            }
                        }
                        else
                        {
                            if (clothes == null)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_HAT_EQUIPED);
                            }
                            else
                            {
                                if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_FEMALE)
                                {
                                    NAPI.Player.SetPlayerAccessory(player, Constants.ACCESSORY_HATS, 57, 0);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerAccessory(player, Constants.ACCESSORY_HATS, 8, 0);
                                }
                                UndressClothes(playerId, 1, Constants.ACCESSORY_HATS);
                            }
                        }
                        break;
                    case Messages.ARG_GLASSES:
                        clothes = GetDressedClothesInSlot(playerId, 1, Constants.ACCESSORY_GLASSES);
                        if (action.ToLower() == Messages.ARG_WEAR)
                        {
                            if (clothes == null)
                            {
                                clothes = GetPlayerClothes(playerId).Where(c => c.slot == Constants.ACCESSORY_GLASSES && c.type == 1).First();
                                if (clothes == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_GLASSES_BOUGHT);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerAccessory(player, clothes.slot, clothes.drawable, clothes.texture);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_GLASSES_EQUIPED);
                            }
                        }
                        else
                        {
                            if (clothes == null)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_GLASSES_EQUIPED);
                            }
                            else
                            {
                                if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_FEMALE)
                                {
                                    NAPI.Player.SetPlayerAccessory(player, Constants.ACCESSORY_GLASSES, 5, 0);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerAccessory(player, Constants.ACCESSORY_GLASSES, 0, 0);
                                }
                                UndressClothes(playerId, 1, Constants.ACCESSORY_GLASSES);
                            }
                        }
                        break;
                    case Messages.ARG_EARRINGS:
                        clothes = GetDressedClothesInSlot(playerId, 1, Constants.ACCESSORY_EARS);
                        if (action.ToLower() == Messages.ARG_WEAR)
                        {
                            if (clothes == null)
                            {
                                clothes = GetPlayerClothes(playerId).Where(c => c.slot == Constants.ACCESSORY_EARS && c.type == 1).First();
                                if (clothes == null)
                                {
                                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_EAR_BOUGHT);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerAccessory(player, clothes.slot, clothes.drawable, clothes.texture);
                                }
                            }
                            else
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_EAR_EQUIPED);
                            }
                        }
                        else
                        {
                            if (clothes == null)
                            {
                                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_EAR_EQUIPED);
                            }
                            else
                            {
                                if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX) == Constants.SEX_FEMALE)
                                {
                                    NAPI.Player.SetPlayerAccessory(player, Constants.ACCESSORY_EARS, 12, 0);
                                }
                                else
                                {
                                    NAPI.Player.SetPlayerAccessory(player, Constants.ACCESSORY_EARS, 3, 0);
                                }
                                UndressClothes(playerId, 1, Constants.ACCESSORY_EARS);
                            }
                        }
                        break;
                    default:
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_COMPLEMENT_COMMAND);
                        break;
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_COMPLEMENT_COMMAND);
            }
        }

        [Command(Messages.COM_PLAYER)]
        public void PlayerCommand(Client player)
        {
            // Get players basic data
            GetPlayerBasicData(player, player);
        }
    }
}
