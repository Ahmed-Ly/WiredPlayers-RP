using GTANetworkAPI;
using WiredPlayers.business;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.vehicles;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WiredPlayers.mechanic
{
    class Mechanic : Script
    {
        public static List<TunningModel> tunningList;

        public Mechanic()
        {
            Event.OnClientEventTrigger += OnClientEventTrigger;
        }

        public static void AddTunningToVehicle(NetHandle vehicle)
        {
            foreach(TunningModel tunning in tunningList)
            {
                if (NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID) == tunning.vehicle)
                {
                    NAPI.Vehicle.SetVehicleMod(vehicle, tunning.slot, tunning.component);
                }
            }
        }

        public static bool PlayerInValidRepairPlace(Client player)
        {
            // Miramos si está en algún taller
            foreach(BusinessModel business in Business.businessList)
            {
                if(business.type == Constants.BUSINESS_TYPE_MECHANIC && player.Position.DistanceTo(business.position) < 25.0f)
                {
                    return true;
                }
            }

            // Miramos si tiene una grúa cerca
            foreach(NetHandle vehicle in NAPI.Pools.GetAllVehicles())
            {
                VehicleHash vehicleHash = (VehicleHash)NAPI.Entity.GetEntityModel(vehicle);
                if (vehicleHash == VehicleHash.TowTruck || vehicleHash == VehicleHash.TowTruck2)
                {
                    return true;
                }
            }

            return false;
        }

        private TunningModel GetVehicleTunningComponent(int vehicleId, int slot, int component)
        {
            TunningModel tunning = null;
            foreach(TunningModel tunningModel in tunningList)
            {
                if(tunningModel.vehicle == vehicleId && tunningModel.slot == slot && tunningModel.component == component)
                {
                    tunning = tunningModel;
                    break;
                }
            }
            return tunning;
        }

        private void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            // Declaramos la variable de vehículo
            NetHandle vehicle = new NetHandle();
            ItemModel item = null;
            int playerId = 0;
            int vehicleId = 0;
            int totalProducts = 0;

            switch (eventName)
            {
                case "repaintVehicle":
                    int colorType = Int32.Parse(arguments[0].ToString());
                    String firstColor = arguments[1].ToString();
                    String secondColor = arguments[2].ToString();
                    int pearlescentColor = Int32.Parse(arguments[3].ToString());
                    int vehiclePaid = Int32.Parse(arguments[4].ToString());
                    vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE);

                    // Miramos el tipo de color
                    switch (colorType)
                    {
                        case 0:
                            // Color predefinido
                            NAPI.Vehicle.SetVehiclePrimaryColor(vehicle, Int32.Parse(firstColor));
                            NAPI.Vehicle.SetVehicleSecondaryColor(vehicle, Int32.Parse(secondColor));

                            // Miramos si lleva nacarado
                            if(pearlescentColor >= 0)
                            {
                                NAPI.Vehicle.SetVehiclePearlescentColor(vehicle, pearlescentColor);
                            }
                            break;
                        case 1:
                            // Color personalizado
                            String[] firstColorArray = firstColor.Split(',');
                            String[] secondColorArray = secondColor.Split(',');
                            NAPI.Vehicle.SetVehicleCustomPrimaryColor(vehicle, Int32.Parse(firstColorArray[0]), Int32.Parse(firstColorArray[1]), Int32.Parse(firstColorArray[2]));
                            NAPI.Vehicle.SetVehicleCustomSecondaryColor(vehicle, Int32.Parse(secondColorArray[0]), Int32.Parse(secondColorArray[1]), Int32.Parse(secondColorArray[2]));
                            break;
                    }
                    
                    // Miramos si ha aceptado
                    if(vehiclePaid > 0)
                    {
                        // Miramos si tiene suficientes productos
                        playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        item = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                        if(item != null && item.amount >= 250)
                        {
                            // Cargamos la facción del vehículo
                            int vehicleFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);

                            // Buscamos a alguien con llaves del vehículo
                            foreach (Client target in NAPI.Pools.GetAllPlayers())
                            {
                                if (Vehicles.HasPlayerVehicleKeys(target, vehicle) || (vehicleFaction > 0 && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == vehicleFaction))
                                {
                                    if (target.Position.DistanceTo(player.Position) < 4.0f)
                                    {
                                        // Mandamos el cambio de pintura al jugador objetivo
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_VEHICLE, vehicle);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_COLOR_TYPE, colorType);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_FIRST_COLOR, firstColor);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_SECOND_COLOR, secondColor);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAINT_PEARLESCENT, pearlescentColor);
                                        NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, vehiclePaid);
                                        NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRODUCTS, 250);

                                        // Enviamos los mensajes a ambos jugadores
                                        String playerMessage = String.Format(Messages.INF_MECHANIC_REPAINT_OFFER, target.Name, vehiclePaid);
                                        String targetMessage = String.Format(Messages.INF_MECHANIC_REPAINT_ACCEPT, player.Name, vehiclePaid);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        return;
                                    }
                                }
                            }

                            // Mandamos el mensaje de que el jugador está lejos
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                        }
                        else
                        {
                            String message = String.Format(Messages.ERR_NOT_REQUIRED_PRODUCTS, 250);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                        }
                    }
                    break;
                case "cancelVehicleRepaint":
                    // Obtenemos el vehículo
                    vehicle = NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE);

                    // Obtenemos los antiguos colores
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
                    break;
                case "calculateTunningCost":
                    // Inicializamos productos
                    totalProducts = 0;

                    // Obtenemos el vehículo
                    vehicle = NAPI.Player.GetPlayerVehicle(player);
                    vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);

                    for (int i = 0; i < 49; i++)
                    {
                        // Obtenemos el componente
                        int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);

                        if (vehicleMod > 0)
                        {
                            TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);
                            if(tunningModel == null)
                            {
                                totalProducts += Constants.TUNNING_PRICE_LIST.Where(x => x.slot == i).First().products;
                            }
                        }
                    }

                    // Enviamos al jugador el precio en productos
                    String priceMessage = String.Format(Messages.INF_TUNNING_PRODUCTS, totalProducts);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + priceMessage);
                    break;
                case "modifyVehicle":
                    int slot = Int32.Parse(arguments[0].ToString());
                    int component = Int32.Parse(arguments[1].ToString());
                    vehicle = NAPI.Player.GetPlayerVehicle(player);
                    if (component > 0)
                    {
                        NAPI.Vehicle.SetVehicleMod(vehicle, slot, component);
                    }
                    else
                    {
                        NAPI.Vehicle.RemoveVehicleMod(vehicle, slot);
                    }
                    break;
                case "cancelVehicleModification":
                    // Obtenemos el vehículo
                    vehicle = NAPI.Player.GetPlayerVehicle(player);
                    vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);

                    // Obtenemos el gasto
                    for (int i = 0; i < 49; i++)
                    {
                        // Obtenemos el componente
                        int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);
                        TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);

                        if (tunningModel == null)
                        {
                            NAPI.Vehicle.RemoveVehicleMod(vehicle, i);
                        }
                        else
                        {
                            NAPI.Vehicle.SetVehicleMod(vehicle, i, vehicleMod);
                        }
                    }
                    break;
                case "confirmVehicleModification":
                    // Inicializamos productos
                    totalProducts = 0;

                    // Obtenemos el vehículo
                    vehicle = NAPI.Player.GetPlayerVehicle(player);
                    vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);

                    // Obtenemos los productos del jugador
                    playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    item = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                    // Obtenemos el gasto
                    for (int i = 0; i < 49; i++)
                    {
                        // Obtenemos el componente
                        int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);

                        if (vehicleMod > 0)
                        {
                            TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);
                            if (tunningModel == null)
                            {
                                totalProducts += Constants.TUNNING_PRICE_LIST.Where(x => x.slot == i).First().products;
                            }
                        }
                    }

                    if (item != null && item.amount >= totalProducts)
                    {
                        for (int i = 0; i < 49; i++)
                        {
                            // Obtenemos el componente
                            int vehicleMod = NAPI.Vehicle.GetVehicleMod(vehicle, i);

                            if (vehicleMod > 0)
                            {
                                TunningModel tunningModel = GetVehicleTunningComponent(vehicleId, i, vehicleMod);
                                if (tunningModel == null)
                                {
                                    // Añadimos la pieza
                                    tunningModel = new TunningModel();
                                    tunningModel.slot = i;
                                    tunningModel.component = vehicleMod;
                                    tunningModel.vehicle = vehicleId;
                                    tunningModel.id = Database.AddTunning(tunningModel);
                                    tunningList.Add(tunningModel);
                                }
                            }
                        }

                        // Descontamos los productos
                        item.amount -= totalProducts;
                        Database.UpdateItem(item);

                        // Cerramos la ventana de tunning
                        NAPI.ClientEvent.TriggerClientEvent(player, "closeTunningMenu");

                        // Mandamos el mensaje
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_VEHICLE_TUNNING);
                    }
                    else
                    {
                        String message = String.Format(Messages.ERR_NOT_REQUIRED_PRODUCTS, totalProducts);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                    }
                    break;
            }
        }
        
        [Command("reparar", Messages.GEN_MECHANIC_REPAIR_COMMAND)]
        public void RepararCommand(Client player, int vehicleId, String type, int price = 0)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_MECHANIC)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_MECHANIC);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_ON_DUTY) == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_ON_DUTY);
            }
            else if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_KILLED) != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_IS_DEAD);
            }
            else if(PlayerInValidRepairPlace(player) == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_VALID_REPAIR_PLACE);
            }
            else
            {
                Vehicle vehicle = Vehicles.GetVehicleById(vehicleId);
                if(vehicle == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                }
                else if(NAPI.Entity.GetEntityPosition(vehicle).DistanceTo(player.Position) > 5.0f)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_WANTED_VEHICLE_FAR);
                }
                else
                {
                    // Calculamos los productos que cuesta
                    int spentProducts = 0;

                    switch (type.ToLower())
                    {
                        case "chasis":
                            spentProducts = Constants.PRICE_VEHICLE_CHASSIS;
                            break;
                        case "puertas":
                            for(int i = 0; i < 6; i++)
                            {
                                if(NAPI.Vehicle.IsVehicleDoorBroken(vehicle, i) == true)
                                {
                                    spentProducts += Constants.PRICE_VEHICLE_DOORS;
                                }
                            }
                            break;
                        case "ruedas":
                            for (int i = 0; i < 4; i++)
                            {
                                if (NAPI.Vehicle.IsVehicleTyrePopped(vehicle, i) == true)
                                {
                                    spentProducts += Constants.PRICE_VEHICLE_TYRES;
                                }
                            }
                            break;
                        case "lunas":
                            for (int i = 0; i < 4; i++)
                            {
                                if (NAPI.Vehicle.IsVehicleWindowBroken(vehicle, i) == true)
                                {
                                    spentProducts += Constants.PRICE_VEHICLE_WINDOWS;
                                }
                            }
                            break;
                        default:
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_HELP + Messages.GEN_MECHANIC_REPAIR_COMMAND);
                            return;
                    }

                    if(price > 0)
                    {
                        // Obtenemos los productos del mecánico
                        int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                        ItemModel item = Globals.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                        if(item != null && item.amount >= spentProducts)
                        {
                            // Obtenemos la facción del vehículo
                            int vehicleFaction = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_FACTION);

                            // Buscamos a alguien con llaves del vehículo
                            foreach (Client target in NAPI.Pools.GetAllPlayers())
                            {
                                if (Vehicles.HasPlayerVehicleKeys(target, vehicle) || (vehicleFaction > 0 && NAPI.Data.GetEntityData(target, EntityData.PLAYER_FACTION) == vehicleFaction))
                                {
                                    if (target.Position.DistanceTo(player.Position) < 4.0f)
                                    {
                                        // Mandamos la reparación al jugador objetivo
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_JOB_PARTNER, player);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAIR_VEHICLE, vehicle);
                                        NAPI.Data.SetEntityData(target, EntityData.PLAYER_REPAIR_TYPE, type);
                                        NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRODUCTS, spentProducts);
                                        NAPI.Data.SetEntityData(target, EntityData.JOB_OFFER_PRICE, price);

                                        // Enviamos los mensajes a ambos jugadores
                                        String playerMessage = String.Format(Messages.INF_MECHANIC_REPAIR_OFFER, target.Name, price);
                                        String targetMessage = String.Format(Messages.INF_MECHANIC_REPAIR_ACCEPT, player.Name, price);
                                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + playerMessage);
                                        NAPI.Chat.SendChatMessageToPlayer(target, Constants.COLOR_INFO + targetMessage);
                                        return;
                                    }
                                }
                            }

                            // Mandamos el mensaje de que el jugador está lejos
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_TOO_FAR);
                        }
                        else
                        {
                            String message = String.Format(Messages.ERR_NOT_REQUIRED_PRODUCTS, spentProducts);
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + message);
                        }
                    }
                    else
                    {
                        // Mandamos información al mecánico
                        String message = String.Format(Messages.INF_REPAIR_PRICE, spentProducts);
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + message);
                    }
                }
            }
        }

        [Command("repintar", Messages.GEN_MECHANIC_REPAINT_COMMAND)]
        public void RepintarCommand(Client player, int vehicleId)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_MECHANIC)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_MECHANIC);
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
                foreach(BusinessModel business in Business.businessList)
                {
                    if(business.type == Constants.BUSINESS_TYPE_MECHANIC && player.Position.DistanceTo(business.position) < 25.0f)
                    {
                        Vehicle vehicle = Vehicles.GetVehicleById(vehicleId);
                        if(vehicle != null)
                        {
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE, vehicle);
                            NAPI.ClientEvent.TriggerClientEvent(player, "showRepaintMenu");
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_VEHICLE_NOT_EXISTS);
                        }
                        return;
                    }
                }

                // Avisamos de que no está en ningún taller
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_MECHANIC_WORKSHOP);
            }
        }

        [Command("tunning")]
        public void TunningCommand(Client player)
        {
            if (NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB) != Constants.JOB_MECHANIC)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_NOT_MECHANIC);
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
                foreach (BusinessModel business in Business.businessList)
                {
                    if (business.type == Constants.BUSINESS_TYPE_MECHANIC && player.Position.DistanceTo(business.position) < 25.0f)
                    {
                        NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                        if (!vehicle.IsNull)
                        {
                            NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE, vehicle);
                            NAPI.ClientEvent.TriggerClientEvent(player, "showTunningMenu");
                        }
                        else
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_VEHICLE);
                        }
                        return;
                    }
                }

                // Avisamos de que no está en ningún taller
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_NOT_IN_MECHANIC_WORKSHOP);
            }
        }
    }
}
