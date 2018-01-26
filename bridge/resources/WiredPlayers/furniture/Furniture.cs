using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.house;
using WiredPlayers.model;
using System.Collections.Generic;
using System;

namespace WiredPlayers.furniture
{
    public class Furniture : Script
    {
        private static List<FurnitureModel> furnitureList;

        public Furniture()
        {
        }

        public void LoadDatabaseFurniture()
        {
            furnitureList = Database.LoadAllFurniture();
            foreach (FurnitureModel furnitureModel in furnitureList)
            {
                furnitureModel.handle = NAPI.Object.CreateObject(furnitureModel.hash, furnitureModel.position, furnitureModel.rotation, (byte)furnitureModel.house);
            }
        }

        public List<FurnitureModel> GetFurnitureInHouse(int houseId)
        {
            List<FurnitureModel> list = new List<FurnitureModel>();
            foreach (FurnitureModel furniture in furnitureList)
            {
                if (furniture.house == houseId)
                {
                    list.Add(furniture);
                }
            }
            return list;
        }

        public FurnitureModel GetFurnitureById(int id)
        {
            FurnitureModel furniture = null;
            foreach (FurnitureModel furnitureModel in furnitureList)
            {
                if (furnitureModel.id == id)
                {
                    furniture = furnitureModel;
                    break;
                }
            }
            return furniture;
        }

        [RemoteEvent("moveFurniture")]
        public void MoveFurnitureEvent(Client player, params object[] arguments)
        {
            int furnitureId = Int32.Parse((String)arguments[0]);
            float posX = float.Parse(((String)arguments[1]).Replace(",", "."));
            float posY = float.Parse(((String)arguments[2]).Replace(",", "."));
            float posZ = float.Parse(((String)arguments[3]).Replace(",", "."));
            FurnitureModel furniture = GetFurnitureById(furnitureId);
            Vector3 position = new Vector3(posX, posY, posZ);
            NAPI.Entity.SetEntityPosition(furniture.handle, position);
        }

        [Command("muebles")]
        public void MueblesCommand(Client player, String action)
        {
            if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_HOUSE_ENTERED) == true)
            {
                int houseId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED);
                HouseModel house = House.GetHouseById(houseId);
                if(house != null && house.owner == player.Name)
                {
                    switch(action.ToLower())
                    {
                        case "colocar":
                            FurnitureModel furniture = new FurnitureModel();
                            furniture.hash = 1251197000;
                            furniture.house = Convert.ToUInt32(houseId);
                            furniture.position = player.Position;
                            furniture.rotation = player.Rotation;
                            furniture.handle = NAPI.Object.CreateObject(furniture.hash, furniture.position, furniture.rotation, (byte)furniture.house);
                            furnitureList.Add(furniture);
                            break;
                        case "mover":
                            String furnitureJson = NAPI.Util.ToJson(GetFurnitureInHouse(houseId));
                            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MOVING_FURNITURE, true);
                            NAPI.ClientEvent.TriggerClientEvent(player, "moveFurniture", furnitureJson);
                            break;
                        case "quitar":
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_FURNITURE_COMMAND);
                            break;
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
