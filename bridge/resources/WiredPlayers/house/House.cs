using GTANetworkAPI;
using System.Collections.Generic;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using System;
using System.Linq;

namespace WiredPlayers.house
{
    public class House : Script
    {
        public static List<HouseModel> houseList;

        public void LoadDatabaseHouses()
        {
            houseList = Database.LoadAllHouses();
            foreach (HouseModel houseModel in houseList)
            {
                String houseLabelText = GetHouseLabelText(houseModel);
                houseModel.houseLabel = NAPI.TextLabel.CreateTextLabel(houseLabelText, houseModel.position, 20.0f, 0.75f, 4, new Color(255, 255, 255), false, houseModel.dimension);
            }
        }

        public static HouseModel GetHouseById(int id)
        {
            HouseModel house = null;
            foreach (HouseModel houseModel in houseList)
            {
                if (houseModel.id == id)
                {
                    house = houseModel;
                    break;
                }
            }
            return house;
        }

        public static HouseModel GetClosestHouse(Client player, float distance = 1.5f)
        {
            HouseModel house = null;
            foreach (HouseModel houseModel in houseList)
            {
                if (player.Position.DistanceTo(houseModel.position) < distance)
                {
                    house = houseModel;
                    distance = player.Position.DistanceTo(houseModel.position);
                }
            }
            return house;
        }

        public static Vector3 GetHouseExitPoint(String ipl)
        {
            Vector3 exit = null;
            foreach (HouseIplModel houseIpl in Constants.HOUSE_IPL_LIST)
            {
                if (houseIpl.ipl == ipl)
                {
                    exit = houseIpl.position;
                    break;
                }
            }
            return exit;
        }

        public static bool HasPlayerHouseKeys(Client player, HouseModel house)
        {
            return (player.Name == house.owner || NAPI.Data.GetEntityData(player, EntityData.PLAYER_RENT_HOUSE) == house.id);
        }

        public static String GetHouseLabelText(HouseModel house)
        {
            String label = String.Empty;

            switch (house.status)
            {
                case Constants.HOUSE_STATE_NONE:
                    label = house.name + "\n" + Messages.GEN_STATE_OCCUPIED;
                    break;
                case Constants.HOUSE_STATE_RENTABLE:
                    label = house.name + "\n" + Messages.GEN_STATE_RENT + "\n" + house.rental + "$";
                    break;
                case Constants.HOUSE_STATE_BUYABLE:
                    label = house.name + "\n" + Messages.GEN_STATE_SALE + "\n" + house.price + "$";
                    break;
            }
            return label;
        }

        public static void BuyHouse(Client player, HouseModel house)
        {
            if (house.status == Constants.HOUSE_STATE_BUYABLE)
            {
                if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK) >= house.price)
                {
                    int bank = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_BANK) - house.price;
                    String message = String.Format(Messages.INF_HOUSE_BUY, house.name, house.price);
                    String labelText = GetHouseLabelText(house);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, bank);
                    house.status = Constants.HOUSE_STATE_NONE;
                    NAPI.TextLabel.SetTextLabelText(house.houseLabel, GetHouseLabelText(house));
                    house.owner = player.Name;
                    house.locked = true;
                    Database.UpdateHouse(house);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_NOT_MONEY);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_NOT_BUYABLE);
            }
        }

        [RemoteEvent("getPlayerPurchasedClothes")]
        public void GetPlayerPurchasedClothesEvent(Client player, int type, int slot)
        {
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            int sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);

            List<ClothesModel> clothesList = Globals.GetPlayerClothes(playerId).Where(c => c.type == type && c.slot == slot).ToList();

            if(clothesList.Count > 0)
            {
                List<String> clothesNames = Globals.GetClothesNames(clothesList);

                // Show player's clothes
                NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerClothes", NAPI.Util.ToJson(clothesList), NAPI.Util.ToJson(clothesNames));
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_CLOTHES_IN_WARDROBE);
            }
        }

        [RemoteEvent("wardrobeClothesItemSelected")]
        public void WardrobeClothesItemSelectedEvent(Client player, int clothesId)
        {
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

            // Replace player clothes for the new ones
            foreach (ClothesModel clothes in Globals.clothesList)
            {
                if (clothes.id == clothesId)
                {
                    clothes.dressed = true;
                    if (clothes.type == 0)
                    {
                        NAPI.Player.SetPlayerClothes(player, clothes.slot, clothes.drawable, clothes.texture);
                    }
                    else
                    {
                        NAPI.Player.SetPlayerAccessory(player, clothes.slot, clothes.drawable, clothes.texture);
                    }

                    // Update dressed clothes into database
                    Database.UpdateClothes(clothes);
                }
                else if (clothes.id != clothesId && clothes.dressed)
                {
                    clothes.dressed = false;

                    // Update dressed clothes into database
                    Database.UpdateClothes(clothes);
                }
            }
        }

        [Command(Commands.COMMAND_RENTABLE, Messages.GEN_RENTABLE_COMMAND)]
        public void RentableCommand(Client player, int amount = 0)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_HOUSE);
            }
            else
            {
                int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                HouseModel house = GetHouseById(houseId);
                if (house == null || house.owner != player.Name)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_HOUSE_OWNER);
                }
                else if (amount > 0)
                {
                    house.rental = amount;
                    house.status = Constants.HOUSE_STATE_RENTABLE;
                    house.tenants = 2;
                    Database.UpdateHouse(house);

                    // Update house's textlabel
                    String labelText = GetHouseLabelText(house);
                    NAPI.TextLabel.SetTextLabelText(house.houseLabel, labelText);

                    // Message sent to the player
                    String message = String.Format(Messages.INF_HOUSE_STATE_RENT, amount);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                }
                else if (house.status == Constants.HOUSE_STATE_RENTABLE)
                {
                    house.status = Constants.HOUSE_STATE_NONE;
                    Database.UpdateHouse(house);

                    // Update house's textlabel
                    String labelText = GetHouseLabelText(house);
                    NAPI.TextLabel.SetTextLabelText(house.houseLabel, labelText);

                    // Message sent to the player
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_HOUSE_RENT_CANCEL);

                    Database.KickTenantsOut(house.id);
                    house.tenants = 2;
                    Database.UpdateHouse(house);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PRICE_POSITIVE);
                }
            }
        }

        [Command(Commands.COMMAND_RENT)]
        public void RentCommand(Client player)
        {
            foreach (HouseModel house in houseList)
            {
                if (player.Position.DistanceTo(house.position) <= 1.5 && player.Dimension == house.dimension)
                {
                    if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_RENT_HOUSE) == 0)
                    {
                        if (house.status != Constants.HOUSE_STATE_RENTABLE)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_HOUSE_NOT_RENTABLE);
                        }
                        else if (NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY) < house.rental)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_RENT_MONEY);
                        }
                        else
                        {
                            int money = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_MONEY) - house.rental;
                            String message = String.Format(Messages.INF_HOUSE_RENT, house.name, house.rental);
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RENT_HOUSE, house.id);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, money);
                            house.tenants--;
                            if (house.tenants == 0)
                            {
                                house.status = Constants.HOUSE_STATE_NONE;
                                String labelText = GetHouseLabelText(house);
                                NAPI.TextLabel.SetTextLabelText(house.houseLabel, labelText);
                            }

                            // Update house's tenants
                            Database.UpdateHouse(house);
                        }
                        break;
                    }
                    else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_RENT_HOUSE) == house.id)
                    {
                        // Remove player's rental
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_RENT_HOUSE, 0);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + String.Format(Messages.INF_HOUSE_RENT_STOP, house.name));
                        house.tenants++;
                        Database.UpdateHouse(house);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_HOUSE_RENTED);
                    }
                }
            }
        }

        [Command(Commands.COMMAND_WARDROBE)]
        public void WardrobeCommand(Client player)
        {
            int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
            if (houseId > 0)
            {
                HouseModel house = GetHouseById(houseId);
                if (HasPlayerHouseKeys(player, house) == true)
                {
                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);

                    if (Globals.GetPlayerClothes(playerId).Count > 0)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(player, "showPlayerWardrobe");
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NO_CLOTHES_IN_WARDROBE);
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_HOUSE_OWNER);
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_IN_HOUSE);
            }
        }
    }
}
