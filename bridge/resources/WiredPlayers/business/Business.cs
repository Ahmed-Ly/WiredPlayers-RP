using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WiredPlayers.business
{
    public class Business : Script
    {
        private Blip scrapYard = null;
        private Blip hardwareBlip = null;
        private Blip hardwareBlip2 = null;

        public static List<BusinessModel> businessList;

        public void LoadDatabaseBusiness()
        {
            businessList = Database.LoadAllBusiness();
            foreach (BusinessModel businessModel in businessList)
            {
                // We create the entrance TextLabel for each business
                businessModel.businessLabel = NAPI.TextLabel.CreateTextLabel(businessModel.name, businessModel.position, 30.0f, 0.75f, 4, new Color(255, 255, 255), false, businessModel.dimension);

                // We mark the blip in the map
                foreach (BusinessBlipModel blipModel in Constants.BUSINESS_BLIP_LIST)
                {
                    if (blipModel.id == businessModel.id)
                    {
                        Blip businessBlip = NAPI.Blip.CreateBlip(businessModel.position);
                        NAPI.Blip.SetBlipName(businessBlip, businessModel.name);
                        NAPI.Blip.SetBlipSprite(businessBlip, blipModel.blip);
                        NAPI.Blip.SetBlipShortRange(businessBlip, true);
                        break;
                    }
                }
            }
        }

        public static BusinessModel GetBusinessById(int businessId)
        {
            BusinessModel business = null;
            foreach (BusinessModel businessModel in businessList)
            {
                if (businessModel.id == businessId)
                {
                    business = businessModel;
                    break;
                }
            }
            return business;
        }

        public static BusinessModel GetClosestBusiness(Client player, float distance = 2.0f)
        {
            BusinessModel business = null;
            foreach (BusinessModel businessModel in businessList)
            {
                if (player.Position.DistanceTo(businessModel.position) < distance)
                {
                    business = businessModel;
                    distance = player.Position.DistanceTo(business.position);
                }
            }
            return business;
        }

        public static List<BusinessItemModel> GetBusinessSoldItems(int business)
        {
            List<BusinessItemModel> businessItems = new List<BusinessItemModel>();
            foreach (BusinessItemModel businessItem in Constants.BUSINESS_ITEM_LIST)
            {
                if (businessItem.business == business)
                {
                    businessItems.Add(businessItem);
                }
            }
            return businessItems;
        }

        public static BusinessItemModel GetBusinessItemFromName(String itemName)
        {
            BusinessItemModel item = null;
            foreach (BusinessItemModel businessItem in Constants.BUSINESS_ITEM_LIST)
            {
                if (businessItem.description == itemName)
                {
                    item = businessItem;
                    break;
                }
            }
            return item;
        }

        public static BusinessItemModel GetBusinessItemFromHash(String itemHash)
        {
            BusinessItemModel item = null;
            foreach (BusinessItemModel businessItem in Constants.BUSINESS_ITEM_LIST)
            {
                if (businessItem.hash == itemHash)
                {
                    item = businessItem;
                    break;
                }
            }
            return item;
        }

        public static List<BusinessClothesModel> GetBusinessClothesFromSlotType(int sex, int type, int slot)
        {
            List<BusinessClothesModel> businessClothesList = new List<BusinessClothesModel>();
            foreach (BusinessClothesModel clothes in Constants.BUSINESS_CLOTHES_LIST)
            {
                if (clothes.type == type && (clothes.sex == sex || Constants.SEX_NONE == clothes.sex) && clothes.bodyPart == slot)
                {
                    businessClothesList.Add(clothes);
                }
            }
            return businessClothesList;
        }

        public static int GetClothesProductsPrice(int id, int sex, int type, int slot)
        {
            int productsPrice = 0;
            foreach (BusinessClothesModel clothesModel in Constants.BUSINESS_CLOTHES_LIST)
            {
                if (clothesModel.type == type && (clothesModel.sex == sex || Constants.SEX_NONE == clothesModel.sex) && clothesModel.bodyPart == slot && clothesModel.clothesId == id)
                {
                    productsPrice = clothesModel.products;
                    break;
                }
            }
            return productsPrice;
        }

        public static String GetBusinessTypeIpl(int type)
        {
            String businessIpl = String.Empty;
            foreach (BusinessIplModel iplModel in Constants.BUSINESS_IPL_LIST)
            {
                if (iplModel.type == type)
                {
                    businessIpl = iplModel.ipl;
                    break;

                }
            }
            return businessIpl;
        }

        public static Vector3 GetBusinessExitPoint(String ipl)
        {
            Vector3 exit = null;
            foreach (BusinessIplModel businessIpl in Constants.BUSINESS_IPL_LIST)
            {
                if (businessIpl.ipl == ipl)
                {
                    exit = businessIpl.position;
                    break;
                }
            }
            return exit;
        }

        public static bool HasPlayerBusinessKeys(Client player, BusinessModel business)
        {
            return (player.Name == business.owner);
        }

        private List<BusinessTattooModel> GetBusinessZoneTattoos(int sex, int zone)
        {
            List<BusinessTattooModel> tattooList = new List<BusinessTattooModel>();

            // Loading tattoo list
            foreach (BusinessTattooModel tattoo in Constants.TATTOO_LIST)
            {
                if (tattoo.slot == zone && tattoo.maleHash.Length > 0 && sex == Constants.SEX_MALE)
                {
                    tattooList.Add(tattoo);
                }
                else if (tattoo.slot == zone && tattoo.femaleHash.Length > 0 && sex == Constants.SEX_FEMALE)
                {
                    tattooList.Add(tattoo);
                }
            }

            return tattooList;
        }

        [RemoteEvent("businessPurchaseMade")]
        public void BusinessPurchaseMadeEvent(Client player, String itemName, int amount)
        {
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);
            BusinessItemModel businessItem = GetBusinessItemFromName(itemName);

            if (business.type == Constants.BUSINESS_TYPE_AMMUNATION && businessItem.type == Constants.ITEM_TYPE_WEAPON && NAPI.Data.GetEntityData(player, EntityData.PLAYER_WEAPON_LICENSE) < Globals.GetTotalSeconds())
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_WEAPON_LICENSE_EXPIRED);
            }
            else
            {
                int hash = 0;
                int price = (int)Math.Round(businessItem.products * business.multiplier) * amount;
                int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                String purchaseMessage = String.Format(Messages.INF_BUSINESS_ITEM_PURCHASED, price);
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                // We look for the item in the inventory
                ItemModel itemModel = Globals.GetPlayerItemModelFromHash(playerId, businessItem.hash);
                if (itemModel == null)
                {
                    // We create the purchased item
                    itemModel = new ItemModel();
                    itemModel.hash = businessItem.hash;
                    if (businessItem.type == Constants.ITEM_TYPE_WEAPON)
                    {
                        itemModel.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
                    }
                    else
                    {
                        itemModel.ownerEntity = Int32.TryParse(itemModel.hash, out hash) ? Constants.ITEM_ENTITY_RIGHT_HAND : Constants.ITEM_ENTITY_PLAYER;
                    }
                    itemModel.ownerIdentifier = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    itemModel.amount = businessItem.uses * amount;
                    itemModel.position = new Vector3(0.0f, 0.0f, 0.0f);
                    itemModel.dimension = 0;

                    // Adding the item to the list and database
                    itemModel.id = Database.AddNewItem(itemModel);
                    Globals.itemList.Add(itemModel);
                }
                else
                {
                    if (Int32.TryParse(itemModel.hash, out hash) == true)
                    {
                        itemModel.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                    }
                    itemModel.amount += (businessItem.uses * amount);
                    Database.UpdateItem(itemModel);
                }

                // If the item has a valid hash, we give it in hand
                if (itemModel.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND)
                {
                    itemModel.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(itemModel.hash), itemModel.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)player.Dimension);
                    NAPI.Entity.AttachEntityToEntity(itemModel.objectHandle, player, "PH_R_Hand", businessItem.position, businessItem.rotation);
                }
                else if (businessItem.type == Constants.ITEM_TYPE_WEAPON)
                {
                    // We give the weapon to the player
                    WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(itemModel.hash);
                    NAPI.Player.GivePlayerWeapon(player, weaponHash, itemModel.amount);

                    // Checking if it's been bought in the Ammu-Nation
                    if (business.type == Constants.BUSINESS_TYPE_AMMUNATION)
                    {
                        Database.AddLicensedWeapon(itemModel.id, player.Name);
                    }
                }

                // We set the item into the hand variable
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_RIGHT_HAND, itemModel.id);

                // If it's a phone, we create a new number
                if (itemModel.hash == Constants.ITEM_HASH_TELEPHONE)
                {
                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE) == 0)
                    {
                        Random random = new Random();
                        int phone = random.Next(100000, 999999);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE, phone);

                        // Sending the message with the new number to the player
                        String message = String.Format(Messages.INF_PLAYER_PHONE, phone);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    }
                }

                // We substract the product and add funds to the business
                if (business.owner != String.Empty)
                {
                    business.funds += price;
                    business.products -= businessItem.products;
                    Database.UpdateBusiness(business);
                }

                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + purchaseMessage);
            }
        }

        [RemoteEvent("getClothesByType")]
        public void GetClothesByTypeEvent(Client player, int type, int slot)
        {
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            List<BusinessClothesModel> clothesList = GetBusinessClothesFromSlotType(sex, type, slot);
            if (clothesList.Count > 0)
            {
                BusinessModel business = GetBusinessById(NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED));
                NAPI.ClientEvent.TriggerClientEvent(player, "showTypeClothes", NAPI.Util.ToJson(clothesList));
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_BUSINESS_CLOTHES_NOT_AVAILABLE);
            }
        }

        [RemoteEvent("dressEquipedClothes")]
        public void DressEquipedClothesEvent(Client player, int type, int slot)
        {
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            ClothesModel clothes = Globals.GetDressedClothesInSlot(playerId, type, slot);
            
            if (clothes != null)
            {
                if (type == 0)
                {
                    NAPI.Player.SetPlayerClothes(player, slot, clothes.drawable, clothes.texture);
                }
                else
                {
                    NAPI.Player.SetPlayerAccessory(player, slot, clothes.drawable, clothes.texture);
                }
            }
            else
            {
                if (type == 0)
                {
                    NAPI.Player.SetPlayerClothes(player, slot, 0, 0);
                }
                else
                {
                    NAPI.Player.SetPlayerAccessory(player, slot, 0, 0);
                }
            }
        }

        [RemoteEvent("clothesItemSelected")]
        public void ClothesItemSelectedEvent(Client player, int clothesId, int type, int slot)
        {
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            int products = GetClothesProductsPrice(clothesId, sex, type, slot);
            BusinessModel business = GetBusinessById(businessId);
            int price = (int)Math.Round(products * business.multiplier);

            // We check whether the player has enough money
            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

            if (playerMoney >= price)
            {
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                // Substracting paid money
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                // We substract the product and add funds to the business
                if (business.owner != String.Empty)
                {
                    business.funds += price;
                    business.products -= products;
                    Database.UpdateBusiness(business);
                }

                // Undress previous clothes
                Globals.UndressClothes(playerId, type, slot);

                // We make the new clothes model
                ClothesModel clothesModel = new ClothesModel();
                clothesModel.player = playerId;
                clothesModel.type = type;
                clothesModel.slot = slot;
                clothesModel.drawable = clothesId;
                clothesModel.dressed = true;

                // Storing the clothes into database
                clothesModel.id = Database.AddClothes(clothesModel);
                Globals.clothesList.Add(clothesModel);

                // Confirmation message sent to the player
                String purchaseMessage = String.Format(Messages.INF_BUSINESS_ITEM_PURCHASED, price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + purchaseMessage);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
            }
        }

        [RemoteEvent("changeHairStyle")]
        public void ChangeHairStyleEvent(Client player, String skinJson)
        {
            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);
            int price = (int)Math.Round(business.multiplier * Constants.PRICE_BARBER_SHOP);

            if (playerMoney >= price)
            {
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                // Getting the new hairstyle from the JSON
                dynamic skinModel = NAPI.Util.FromJson(skinJson);
                
                SkinModel skin = new SkinModel();
                skin.hairModel = skinModel.hairModel;
                skin.firstHairColor = skinModel.firstHairColor;
                skin.secondHairColor = skinModel.secondHairColor;
                skin.beardModel = skinModel.beardModel;
                skin.beardColor = skinModel.beardColor;
                skin.eyebrowsModel = skinModel.eyebrowsModel;
                skin.eyebrowsColor = skinModel.eyebrowsColor;

                // We update values in the database
                Database.UpdateCharacterHair(playerId, skin);

                // Saving new entity data
                NAPI.Data.SetEntitySharedData(player, EntityData.HAIR_MODEL, skin.hairModel);
                NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_HAIR_COLOR, skin.firstHairColor);
                NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_HAIR_COLOR, skin.secondHairColor);
                NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_MODEL, skin.beardModel);
                NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_COLOR, skin.beardColor);
                NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_MODEL, skin.eyebrowsModel);
                NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_COLOR, skin.eyebrowsColor);

                // Substract money to the player
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                // We substract the product and add funds to the business
                if (business.owner != String.Empty)
                {
                    business.funds += price;
                    business.products -= Constants.PRICE_BARBER_SHOP;
                    Database.UpdateBusiness(business);
                }

                // Confirmation message sent to the player
                String playerMessage = String.Format(Messages.INF_HAIRCUT_PURCHASED, price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
            }
            else
            {
                String message = String.Format(Messages.ERR_HAIRCUT_MONEY, price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);

                // Setting the old hairstyle to the player
                NAPI.ClientEvent.TriggerClientEvent(player, "cancelHairdressersChanges");
            }
        }

        [RemoteEvent("loadZoneTattoos")]
        public void LoadZoneTattoosEvent(Client player, int zone)
        {
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            List<BusinessTattooModel> tattooList = GetBusinessZoneTattoos(sex, zone);

            // We update the menu with the tattoos
            NAPI.ClientEvent.TriggerClientEvent(player, "showZoneTattoos", NAPI.Util.ToJson(tattooList));
        }

        [RemoteEvent("purchaseTattoo")]
        public void PurchaseTattooEvent(Client player, int tattooZone, int tattooIndex)
        {
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);

            // Getting the tattoo and its price
            BusinessTattooModel businessTattoo = GetBusinessZoneTattoos(sex, tattooZone).ElementAt(tattooIndex);
            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
            int price = (int)Math.Round(business.multiplier * businessTattoo.price);

            if (price <= playerMoney)
            {
                TattooModel tattoo = new TattooModel();
                tattoo.player = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                tattoo.slot = tattooZone;
                tattoo.library = businessTattoo.library;
                tattoo.hash = sex == Constants.SEX_MALE ? businessTattoo.maleHash : businessTattoo.femaleHash;

                if (Database.AddTattoo(tattoo) == true)
                {
                    // We add the tattoo to the list
                    Globals.tattooList.Add(tattoo);

                    // Substract player money
                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                    // We substract the product and add funds to the business
                    if (business.owner != String.Empty)
                    {
                        business.funds += price;
                        business.products -= businessTattoo.price;
                        Database.UpdateBusiness(business);
                    }

                    // Confirmation message sent to the player
                    String playerMessage = String.Format(Messages.INF_TATTOO_PURCHASED, price);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);

                    // Reload client tattoo list
                    NAPI.ClientEvent.TriggerClientEvent(player, "addPurchasedTattoo", NAPI.Util.ToJson(tattoo));
                }
                else
                {
                    // Player already had that tattoo
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_TATTOO_DUPLICATED);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
            }
        }

        [RemoteEvent("loadCharacterClothes")]
        public void LoadCharacterClothesEvent(Client player)
        {
            // Generate player's clothes
            Globals.PopulateCharacterClothes(player);
        }
    }
}
