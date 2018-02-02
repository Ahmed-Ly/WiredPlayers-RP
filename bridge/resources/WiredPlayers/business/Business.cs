using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace WiredPlayers.business
{
    public class Business : Script
    {
        private Blip scrapYard = null;
        private Blip hardwareBlip = null;
        private Blip hardwareBlip2 = null;

        public static List<BusinessModel> businessList;

        public Business()
        {
            Event.OnResourceStart += OnResourceStartHandler;
        }

        private void OnResourceStartHandler()
        {
            scrapYard = NAPI.Blip.CreateBlip(new Vector3(-441.6721f, -1696.798f, 18.94322f));
            NAPI.Blip.SetBlipSprite(scrapYard, 566);
            NAPI.Blip.SetBlipName(scrapYard, "Desguace");
            NAPI.Blip.SetBlipShortRange(scrapYard, true);

            hardwareBlip = NAPI.Blip.CreateBlip(new Vector3(-582.131f, -1000.79f, 22.3297f));
            NAPI.Blip.SetBlipSprite(hardwareBlip, 402);
            NAPI.Blip.SetBlipName(hardwareBlip, "Ferreteria Little Seoul");
            NAPI.Blip.SetBlipShortRange(hardwareBlip, true);

            hardwareBlip2 = NAPI.Blip.CreateBlip(new Vector3(342.944f, -1297.85f, 32.5098f));
            NAPI.Blip.SetBlipSprite(hardwareBlip2, 402);
            NAPI.Blip.SetBlipName(hardwareBlip2, "Herramientas Bert");
            NAPI.Blip.SetBlipShortRange(hardwareBlip2, true);
        }

        public void LoadDatabaseBusiness()
        {
            businessList = Database.LoadAllBusiness();
            foreach (BusinessModel businessModel in businessList)
            {
                // Creamos la etiqueta de entrada
                businessModel.businessLabel = NAPI.TextLabel.CreateTextLabel(businessModel.name, businessModel.position, 30.0f, 0.75f, 4, new Color(255, 255, 255), false, businessModel.dimension);

                // Miramos si está entre los destacados
                foreach (BusinessBlipModel blipModel in Constants.BUSINESS_BLIP_LIST)
                {
                    if(blipModel.id == businessModel.id)
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
            foreach(BusinessModel businessModel in businessList)
            {
                if(businessModel.id == businessId)
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
            foreach(BusinessModel businessModel in businessList)
            {
                if(player.Position.DistanceTo(businessModel.position) < distance)
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
            foreach(BusinessItemModel businessItem in Constants.BUSINESS_ITEM_LIST)
            {
                if(businessItem.business == business)
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
            foreach(BusinessClothesModel clothes in Constants.BUSINESS_CLOTHES_LIST)
            {
                if(clothes.type == type && (clothes.sex == sex || Constants.SEX_NONE == clothes.sex) && clothes.bodyPart == slot)
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
            foreach(BusinessIplModel iplModel in Constants.BUSINESS_IPL_LIST)
            {
                if(iplModel.type == type)
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

            // Cargamos la lista de tatuajes
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
        public void BusinessPurchaseMadeEvent(Client player, params object[] arguments)
        {
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);
            BusinessItemModel businessItem = GetBusinessItemFromName((String)arguments[0]);
            int amount = Int32.Parse(arguments[1].ToString());

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

                // Miramos si tiene el objeto ya en el inventario
                ItemModel itemModel = Globals.GetPlayerItemModelFromHash(playerId, businessItem.hash);
                if (itemModel == null)
                {
                    // Creamos el objeto que ha comprado
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

                    // Añadimos el objeto a la base de datos y a la lista
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

                // Si tiene hash, le entregamos en mano el objeto
                if (itemModel.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND)
                {
                    itemModel.objectHandle = NAPI.Object.CreateObject(UInt32.Parse(itemModel.hash), itemModel.position, new Vector3(0.0f, 0.0f, 0.0f), (byte)player.Dimension);
                    NAPI.Entity.AttachEntityToEntity(itemModel.objectHandle, player, "PH_R_Hand", businessItem.position, businessItem.rotation);
                }
                else if (businessItem.type == Constants.ITEM_TYPE_WEAPON)
                {
                    // Damos el arma al jugador
                    WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(itemModel.hash);
                    NAPI.Player.GivePlayerWeapon(player, weaponHash, itemModel.amount);

                    // Miramos si ha comprado en el Ammu-Nation
                    if (business.type == Constants.BUSINESS_TYPE_AMMUNATION)
                    {
                        Database.AddLicensedWeapon(itemModel.id, player.Name);
                    }
                }

                // Añadimos el objeto comprado a la mano
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_RIGHT_HAND, itemModel.id);

                // Si es un teléfono, le damos el número
                if (itemModel.hash == Constants.ITEM_HASH_TELEPHONE)
                {
                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_PHONE) == 0)
                    {
                        Random random = new Random();
                        int phone = random.Next(100000, 999999);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE, phone);

                        // Informamos al jugador de su número
                        String message = String.Format(Messages.INF_PLAYER_PHONE, phone);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    }
                }

                // Si el negocio tiene dueño, le damos el dinero y descontamos los productos
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

        [RemoteEvent("clothesSlotSelected")]
        public void ClothesSlotSelectedEvent(Client player, params object[] arguments)
        {
            int type = Int32.Parse(arguments[0].ToString());
            int slot = Int32.Parse(arguments[1].ToString());
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            List<BusinessClothesModel> clothesList = GetBusinessClothesFromSlotType(sex, type, slot);
            if (clothesList.Count > 0)
            {
                BusinessModel business = GetBusinessById(NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED));
                NAPI.ClientEvent.TriggerClientEvent(player, "showClothesFromSelectedType", NAPI.Util.ToJson(clothesList), business.multiplier);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_BUSINESS_CLOTHES_NOT_AVAILABLE);
            }
        }

        [RemoteEvent("dressEquipedClothes")]
        public void DressEquipedClothesEvent(Client player, params object[] arguments)
        {
            int type = Int32.Parse(arguments[0].ToString());
            int slot = Int32.Parse(arguments[1].ToString());
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            ClothesModel clothes = Globals.GetDressedClothesInSlot(playerId, type, slot);

            // Miramos si llevaba algo o no
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
        public void ClothesItemSelectedEvent(Client player, params object[] arguments)
        {
            // Calculamos el precio de la ropa
            int clothesId = Int32.Parse(arguments[0].ToString());
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            int type = Int32.Parse(arguments[1].ToString());
            int slot = Int32.Parse(arguments[2].ToString());
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            int products = GetClothesProductsPrice(clothesId, sex, type, slot);
            BusinessModel business = GetBusinessById(businessId);
            int price = (int)Math.Round(products * business.multiplier);

            // Miramos si tiene dinero suficiente
            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);

            if (playerMoney >= price)
            {
                // Sacamos el identificador del jugador
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                // Restamos al jugador el dinero
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                // Si el negocio tiene dueño, le damos el dinero y descontamos los productos
                if (business.owner != String.Empty)
                {
                    business.funds += price;
                    business.products -= products;
                    Database.UpdateBusiness(business);
                }

                // Desequipamos la ropa anterior
                Globals.UndressClothes(playerId, type, slot);

                // Creamos el modelo de ropa
                ClothesModel clothesModel = new ClothesModel();
                clothesModel.player = playerId;
                clothesModel.type = type;
                clothesModel.slot = slot;
                clothesModel.drawable = clothesId;
                clothesModel.dressed = true;

                // Añadimos el objeto a la base de datos
                clothesModel.id = Database.AddClothes(clothesModel);
                Globals.clothesList.Add(clothesModel);

                // Enviamos el mensaje al jugador
                String purchaseMessage = String.Format(Messages.INF_BUSINESS_ITEM_PURCHASED, price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + purchaseMessage);
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
            }
        }

        [RemoteEvent("changeHairStyle")]
        public void ChangeHairStyleEvent(Client player, params object[] arguments)
        {
            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);
            int price = (int)Math.Round(business.multiplier * Constants.PRICE_BARBER_SHOP);

            if (playerMoney >= price)
            {
                // Obtenemos el id del jugador
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                // Obtenemos la nueva imagen
                dynamic skinModel = NAPI.Util.FromJson(arguments[0].ToString());

                // Creamos el modelo de datos
                SkinModel skin = new SkinModel();
                skin.hairModel = skinModel.hairModel;
                skin.firstHairColor = skinModel.firstHairColor; 
                skin.secondHairColor = skinModel.secondHairColor; 
                skin.beardModel = skinModel.beardModel;
                skin.beardColor = skinModel.beardColor;
                skin.eyebrowsModel = skinModel.eyebrowsModel;
                skin.eyebrowsColor = skinModel.eyebrowsColor;
                
                // Recogemos los datos de peluquería
                Database.UpdateCharacterHair(playerId, skin);

                // Guardamos los datos para el cliente
                NAPI.Data.SetEntitySharedData(player, EntityData.HAIR_MODEL, skin.hairModel);
                NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_HAIR_COLOR, skin.firstHairColor);
                NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_HAIR_COLOR, skin.secondHairColor);
                NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_MODEL, skin.beardModel);
                NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_COLOR, skin.beardColor);
                NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_MODEL, skin.eyebrowsModel);
                NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_COLOR, skin.eyebrowsColor);

                // Restamos el dinero al jugador
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                // Si el negocio tiene dueño, le damos el dinero y descontamos los productos
                if (business.owner != String.Empty)
                {
                    business.funds += price;
                    business.products -= Constants.PRICE_BARBER_SHOP;
                    Database.UpdateBusiness(business);
                }

                // Avisamos al jugador
                String playerMessage = String.Format(Messages.INF_HAIRCUT_PURCHASED, price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
            }
            else
            {
                String message = String.Format(Messages.ERR_HAIRCUT_MONEY, price);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);

                // Volvemos a poner el estilo de pelo al jugador
                NAPI.ClientEvent.TriggerClientEvent(player, "cancelHairdressersChanges");
            }
        }

        [RemoteEvent("loadZoneTattoos")]
        public void LoadZoneTattoosEvent(Client player, params object[] arguments)
        {
            int zone = Int32.Parse(arguments[0].ToString());
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            List<BusinessTattooModel> tattooList = GetBusinessZoneTattoos(sex, zone);

            // Actualizamos el menú de tatuajes
            NAPI.ClientEvent.TriggerClientEvent(player, "showZoneTattoos", NAPI.Util.ToJson(tattooList));
        }

        [RemoteEvent("purchaseTattoo")]
        public void PurchaseTattooEvent(Client player, params object[] arguments)
        {
            // Obtenemos los datos del tatuaje
            int tattooZone = Int32.Parse(arguments[0].ToString());
            int tattooIndex = Int32.Parse(arguments[1].ToString());

            // Obtenemos los datos del negocio y sexo
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            int businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
            BusinessModel business = GetBusinessById(businessId);

            // Obtenemos el tatuaje seleccionado y su precio
            BusinessTattooModel businessTattoo = GetBusinessZoneTattoos(sex, tattooZone).ElementAt(tattooIndex);
            int playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
            int price = (int)Math.Round(business.multiplier * businessTattoo.price);

            if (price <= playerMoney)
            {
                // Creamos el modelo de datos
                TattooModel tattoo = new TattooModel();
                tattoo.player = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                tattoo.slot = tattooZone;
                tattoo.library = businessTattoo.library;
                tattoo.hash = sex == Constants.SEX_MALE ? businessTattoo.maleHash : businessTattoo.femaleHash;

                // Intentamos insertar el tatuaje en la base de datos
                bool inserted = Database.AddTattoo(tattoo);

                if (inserted)
                {
                    // Añadimos el tatuaje a la lista
                    Globals.tattooList.Add(tattoo);

                    // Restamos el dinero al jugador
                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                    // Si el negocio tiene dueño, le damos el dinero y descontamos los productos
                    if (business.owner != String.Empty)
                    {
                        business.funds += price;
                        business.products -= businessTattoo.price;
                        Database.UpdateBusiness(business);
                    }

                    // Avisamos al jugador
                    String playerMessage = String.Format(Messages.INF_TATTOO_PURCHASED, price);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);

                    // Recargamos la lista de tatuajes
                    NAPI.ClientEvent.TriggerClientEvent(player, "addPurchasedTattoo", NAPI.Util.ToJson(tattoo));
                }
                else
                {
                    // El jugador ya tenía ese tatuaje
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_TATTOO_DUPLICATED);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
            }
        }

        [RemoteEvent("loadCharacterClothes")]
        public void LoadCharacterClothesEvent(Client player, params object[] arguments)
        {
            // Generación de la ropa del personaje
            Globals.PopulateCharacterClothes(player);
        }
    }
}
