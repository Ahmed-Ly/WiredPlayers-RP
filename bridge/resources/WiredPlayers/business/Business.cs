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

        public Business()
        {
            Event.OnClientEventTrigger += OnClientEventTrigger;
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

        private void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            // Declaramos las variables genéricas
            int sex = 0;
            int type = 0;
            int slot = 0;
            int price = 0;
            int playerId = 0;
            int businessId = 0;
            int playerMoney = 0;
            BusinessModel business = null;

            switch (eventName)
            {
                case "businessPurchaseMade":
                    businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                    business = getBusinessById(businessId);
                    BusinessItemModel businessItem = getBusinessItemFromName((String)arguments[0]);
                    int amount = Int32.Parse(arguments[1].ToString());

                    if (business.type == Constants.BUSINESS_TYPE_AMMUNATION && businessItem.type == Constants.ITEM_TYPE_WEAPON && NAPI.Data.GetEntityData(player, EntityData.PLAYER_WEAPON_LICENSE) < Globals.getTotalSeconds())
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_WEAPON_LICENSE_EXPIRED);
                    }
                    else
                    {
                        int hash = 0;
                        price = (int)Math.Round(businessItem.products * business.multiplier) * amount;
                        int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                        String purchaseMessage = String.Format(Messages.INF_BUSINESS_ITEM_PURCHASED, price);
                        playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                        // Miramos si tiene el objeto ya en el inventario
                        ItemModel itemModel = Globals.getPlayerItemModelFromHash(playerId, businessItem.hash);
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
                            itemModel.id = Database.addNewItem(itemModel);
                            Globals.itemList.Add(itemModel);
                        }
                        else
                        {
                            if (Int32.TryParse(itemModel.hash, out hash) == true)
                            {
                                itemModel.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                            }
                            itemModel.amount += (businessItem.uses * amount);
                            Database.updateItem(itemModel);
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
                                Database.addLicensedWeapon(itemModel.id, player.Name);
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
                            Database.updateBusiness(business);
                        }

                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money - price);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + purchaseMessage);
                    }
                    break;
                case "clothesSlotSelected":
                    type = Int32.Parse(arguments[0].ToString());
                    slot = Int32.Parse(arguments[1].ToString());
                    sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
                    List<BusinessClothesModel> clothesList = getBusinessClothesFromSlotType(sex, type, slot);
                    if (clothesList.Count > 0)
                    {
                        business = getBusinessById(NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED));
                        NAPI.ClientEvent.TriggerClientEvent(player, "showClothesFromSelectedType", NAPI.Util.ToJson(clothesList), business.multiplier);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_BUSINESS_CLOTHES_NOT_AVAILABLE);
                    }
                    break;
                case "dressEquipedClothes":
                    type = Int32.Parse(arguments[0].ToString());
                    slot = Int32.Parse(arguments[1].ToString());
                    playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    ClothesModel clothes = Globals.getDressedClothesInSlot(playerId, type, slot);

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
                    break;
                case "clothesItemSelected":
                    // Calculamos el precio de la ropa
                    int clothesId = Int32.Parse(arguments[0].ToString());
                    businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                    type = Int32.Parse(arguments[1].ToString());
                    slot = Int32.Parse(arguments[2].ToString());
                    sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
                    int products = getClothesProductsPrice(clothesId, sex, type, slot);
                    business = getBusinessById(businessId);
                    price = (int)Math.Round(products * business.multiplier);

                    // Miramos si tiene dinero suficiente
                    playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                    if (playerMoney >= price)
                    {
                        // Sacamos el identificador del jugador
                        playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                        // Restamos al jugador el dinero
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                        // Si el negocio tiene dueño, le damos el dinero y descontamos los productos
                        if (business.owner != String.Empty)
                        {
                            business.funds += price;
                            business.products -= products;
                            Database.updateBusiness(business);
                        }

                        // Desequipamos la ropa anterior
                        Globals.undressClothes(playerId, type, slot);

                        // Creamos el modelo de ropa
                        ClothesModel clothesModel = new ClothesModel();
                        clothesModel.player = playerId;
                        clothesModel.type = type;
                        clothesModel.slot = slot;
                        clothesModel.drawable = clothesId;
                        clothesModel.dressed = true;

                        // Añadimos el objeto a la base de datos
                        clothesModel.id = Database.addClothes(clothesModel);
                        Globals.clothesList.Add(clothesModel);

                        // Enviamos el mensaje al jugador
                        String purchaseMessage = String.Format(Messages.INF_BUSINESS_ITEM_PURCHASED, price);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + purchaseMessage);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ENOUGH_MONEY);
                    }
                    break;
                case "hairStyleChanged":
                    playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                    businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                    business = getBusinessById(businessId);
                    price = (int)Math.Round(business.multiplier * Constants.PRICE_BARBER_SHOP);

                    if (playerMoney >= price)
                    {
                        // Obtenemos el id del jugador
                        playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                        // Creamos el modelo de datos
                        SkinModel skin = new SkinModel();
                        skin.hairModel = NAPI.Data.GetEntitySharedData(player, EntityData.GTAO_HAIR_MODEL);
                        skin.firstHairColor = NAPI.Data.GetEntitySharedData(player, EntityData.GTAO_HAIR_FIRST_COLOR);
                        skin.secondHairColor = NAPI.Data.GetEntitySharedData(player, EntityData.GTAO_HAIR_SECOND_COLOR);
                        skin.beardModel = NAPI.Data.GetEntitySharedData(player, EntityData.GTAO_BEARD_MODEL);
                        skin.beardColor = NAPI.Data.GetEntitySharedData(player, EntityData.GTAO_BEARD_COLOR);
                        skin.eyebrowsModel = NAPI.Data.GetEntitySharedData(player, EntityData.GTAO_EYEBROWS_MODEL);
                        skin.eyebrowsColor = NAPI.Data.GetEntitySharedData(player, EntityData.GTAO_EYEBROWS_COLOR);

                        // Recogemos los datos de peluquería
                        Database.updateCharacterHair(playerId, skin);

                        // Restamos el dinero al jugador
                        NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, playerMoney - price);

                        // Si el negocio tiene dueño, le damos el dinero y descontamos los productos
                        if (business.owner != String.Empty)
                        {
                            business.funds += price;
                            business.products -= Constants.PRICE_BARBER_SHOP;
                            Database.updateBusiness(business);
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
                    break;
                case "loadZoneTattoos":
                    int zone = Int32.Parse(arguments[0].ToString());
                    sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
                    List<BusinessTattooModel> tattooList = getBusinessZoneTattoos(sex, zone);
                    
                    // Actualizamos el menú de tatuajes
                    NAPI.ClientEvent.TriggerClientEvent(player, "showZoneTattoos", NAPI.Util.ToJson(tattooList));
                    break;
                case "purchaseTattoo":
                    // Obtenemos los datos del tatuaje
                    int tattooZone = Int32.Parse(arguments[0].ToString());
                    int tattooIndex = Int32.Parse(arguments[1].ToString());

                    // Obtenemos los datos del negocio y sexo
                    sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
                    businessId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED);
                    business = getBusinessById(businessId);

                    // Obtenemos el tatuaje seleccionado y su precio
                    BusinessTattooModel businessTattoo = getBusinessZoneTattoos(sex, tattooZone).ElementAt(tattooIndex);
                    playerMoney = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY);
                    price = (int)Math.Round(business.multiplier * businessTattoo.price);

                    if (price <= playerMoney)
                    {
                        // Creamos el modelo de datos
                        TattooModel tattoo = new TattooModel();
                        tattoo.player = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        tattoo.slot = tattooZone;
                        tattoo.library = businessTattoo.library;
                        tattoo.hash = sex == Constants.SEX_MALE ? businessTattoo.maleHash : businessTattoo.femaleHash;

                        // Intentamos insertar el tatuaje en la base de datos
                        bool inserted = Database.addTattoo(tattoo);

                        if(inserted)
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
                                Database.updateBusiness(business);
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
                    break;
                case "loadCharacterClothes":
                    // Generación de la ropa del personaje
                    Globals.populateCharacterClothes(player);
                    break;
            }
        }

        public void loadDatabaseBusiness()
        {
            businessList = Database.loadAllBusiness();
            foreach (BusinessModel businessModel in businessList)
            {
                // Creamos la etiqueta de entrada
                businessModel.businessLabel = NAPI.TextLabel.CreateTextLabel(businessModel.name, businessModel.position, 30.0f, 0.75f, 0, new Color(255, 255, 255), false, businessModel.dimension);

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

        public static BusinessModel getBusinessById(int businessId)
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

        public static BusinessModel getClosestBusiness(Client player, float distance = 2.0f)
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

        public static List<BusinessItemModel> getBusinessSoldItems(int business)
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

        public static BusinessItemModel getBusinessItemFromName(String itemName)
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

        public static BusinessItemModel getBusinessItemFromHash(String itemHash)
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

        public static List<BusinessClothesModel> getBusinessClothesFromSlotType(int sex, int type, int slot)
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

        public static int getClothesProductsPrice(int id, int sex, int type, int slot)
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

        public static String getBusinessTypeIpl(int type)
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

        public static Vector3 getBusinessExitPoint(String ipl)
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

        public static bool hasPlayerBusinessKeys(Client player, BusinessModel business)
        {
            return (player.Name == business.owner);
        }

        private List<BusinessTattooModel> getBusinessZoneTattoos(int sex, int zone)
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
    }
}
