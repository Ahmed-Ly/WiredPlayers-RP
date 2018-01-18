using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.weapons
{
    class Weapons : Script
    {
        private static Timer weaponTimer;
        private static List<Timer> vehicleWeaponTimer;
        public static List<WeaponCrateModel> weaponCrateList;

        public Weapons()
        {
            Event.OnResourceStart += OnResourceStart;
            Event.OnClientEventTrigger += OnClientEventHandler;
            Event.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            Event.OnPlayerExitVehicle += OnPlayerExitVehicle;
            Event.OnEntityEnterCheckpoint += OnEntityEnterCheckpoint;
            Event.OnPlayerWeaponSwitch += OnPlayerWeaponSwitch;
            Event.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        public static void GivePlayerWeaponItems(Client player)
        {
            int itemId = 0;
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            foreach(ItemModel item in Globals.itemList)
            {
                if (!Int32.TryParse(item.hash, out itemId) && item.ownerIdentifier == playerId && item.ownerEntity == Constants.ITEM_ENTITY_WHEEL)
                {
                    WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);
                    NAPI.Player.GivePlayerWeapon(player, weaponHash, 0);
                    NAPI.Player.SetPlayerWeaponAmmo(player, weaponHash, item.amount);
                }
            }
        }

        public static void GivePlayerNewWeapon(Client player, WeaponHash weapon, int bullets, bool licensed)
        {
            // Creamos el objeto
            ItemModel weaponModel = new ItemModel();
            weaponModel.hash = weapon.ToString();
            weaponModel.amount = bullets;
            weaponModel.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
            weaponModel.ownerIdentifier = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            weaponModel.position = new Vector3(0.0f, 0.0f, 0.0f);
            weaponModel.dimension = 0;
            weaponModel.id = Database.AddNewItem(weaponModel);
            Globals.itemList.Add(weaponModel);

            // Damos el arma al jugador
            NAPI.Player.GivePlayerWeapon(player, weapon, 0);
            NAPI.Player.SetPlayerWeaponAmmo(player, weapon, bullets);

            // Miramos si es un arma registrada
            if(licensed)
            {
                Database.AddLicensedWeapon(weaponModel.id, player.Name);
            }
        }

        public static String GetGunAmmunitionType(WeaponHash weapon)
        {
            String type = String.Empty;
            foreach (GunModel gun in Constants.GUN_LIST)
            {
                if (weapon == gun.weapon)
                {
                    type = gun.ammunition;
                    break;
                }
            }
            return type;
        }

        public static int GetGunAmmunitionCapacity(WeaponHash weapon)
        {
            int amount = 0;
            foreach (GunModel gun in Constants.GUN_LIST)
            {
                if (weapon == gun.weapon)
                {
                    amount = gun.capacity;
                    break;
                }
            }
            return amount;
        }

        public static ItemModel GetEquippedWeaponItemModelByHash(int playerId, WeaponHash weapon)
        {
            ItemModel item = null;
            foreach(ItemModel itemModel in Globals.itemList)
            {
                if(itemModel.ownerIdentifier == playerId && (itemModel.ownerEntity == Constants.ITEM_ENTITY_WHEEL || itemModel.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND) && weapon.ToString() == itemModel.hash)
                {
                    item = itemModel;
                    break;
                }
            }
            return item;
        }

        public static WeaponCrateModel GetClosestWeaponCrate(Client player, float distance = 1.5f)
        {
            WeaponCrateModel weaponCrate = null;
            foreach(WeaponCrateModel weaponCrateModel in weaponCrateList)
            {
                if (player.Position.DistanceTo(weaponCrateModel.position) < distance && weaponCrateModel.carriedEntity == String.Empty)
                {
                    weaponCrate = weaponCrateModel;
                    break;
                }
            }
            return weaponCrate;
        }

        public static WeaponCrateModel GetPlayerCarriedWeaponCrate(int playerId)
        {
            WeaponCrateModel weaponCrate = null;
            foreach (WeaponCrateModel weaponCrateModel in weaponCrateList)
            {
                if (weaponCrateModel.carriedEntity == Constants.ITEM_ENTITY_PLAYER && weaponCrateModel.carriedIdentifier == playerId)
                {
                    weaponCrate = weaponCrateModel;
                    break;
                }
            }
            return weaponCrate;
        }

        public static void WeaponsPrewarn()
        {
            // Avisamos a todos los jugadores de facciones conectados
            foreach(Client player in NAPI.Pools.GetAllPlayers())
            {
                if(NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) > Constants.LAST_STATE_FACTION)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WEAPON_PREWARN);
                }
            }

            // Creamos un temporizador para el siguiente aviso
            weaponTimer = new Timer(OnWeaponPrewarn, null, 600000, Timeout.Infinite);
        }

        private void OnResourceStart()
        {
            vehicleWeaponTimer = new List<Timer>();
            weaponCrateList = new List<WeaponCrateModel>();
        }

        private void OnClientEventHandler(Client player, String eventName, params object[] arguments)
        {
            if (eventName == "reloadPlayerWeapon")
            {
                WeaponHash weapon = NAPI.Player.GetPlayerCurrentWeapon(player);
                int maxCapacity = GetGunAmmunitionCapacity(weapon);
                int currentBullets = NAPI.Player.GetPlayerWeaponAmmo(player, weapon);
                if (currentBullets < maxCapacity)
                {
                    String bulletType = GetGunAmmunitionType(weapon);
                    int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                    ItemModel bulletItem = Globals.GetPlayerItemModelFromHash(playerId, bulletType);
                    if (bulletItem != null)
                    {
                        int bulletsLeft = maxCapacity - currentBullets;
                        if (bulletsLeft >= bulletItem.amount)
                        {
                            currentBullets += bulletItem.amount;
                            Database.RemoveItem(bulletItem.id);
                            Globals.itemList.Remove(bulletItem);
                        }
                        else
                        {
                            currentBullets += bulletsLeft;
                            bulletItem.amount -= bulletsLeft;
                            Database.UpdateItem(bulletItem);
                        }

                        // Añadimos la munición al objeto arma
                        ItemModel weaponItem = GetEquippedWeaponItemModelByHash(playerId, weapon);
                        weaponItem.amount = currentBullets;
                        Database.UpdateItem(weaponItem);

                        // Recargamos el arma
                        NAPI.Player.SetPlayerWeaponAmmo(player, weapon, currentBullets);
                        //NAPI.Native.SendNativeToPlayer(player, Hash.MAKE_PED_RELOAD, player);
                    }
                }
            }
        }
        
        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle, sbyte seat)
        {
            if(NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_ID) && NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
            {
                int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                if(!NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_WEAPON_UNPACKING) && GetVehicleWeaponCrates(vehicleId) > 0)
                {
                    // Avisamos de la posición de entrega
                    Vector3 weaponPosition = new Vector3(-2085.543f, 2600.857f, -0.4712417f);
                    Checkpoint weaponCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, weaponPosition, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE, weaponCheckpoint);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WEAPON_POSITION_MARK);
                    NAPI.ClientEvent.TriggerClientEvent(player, "showWeaponCheckpoint", weaponPosition);
                }
            }
        }

        private void OnPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if(NAPI.Data.HasEntityData(vehicle, EntityData.VEHICLE_ID) == true)
            {
                int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_JOB_COLSHAPE) && GetVehicleWeaponCrates(vehicleId) > 0)
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "deleteWeaponCheckpoint");
                }
            }
        }

        private void OnEntityEnterCheckpoint(Checkpoint checkpoint, NetHandle entity)
        {
            if (NAPI.Entity.GetEntityType(entity) == EntityType.Player && NAPI.Data.HasEntityData(entity, EntityData.PLAYER_JOB_COLSHAPE) == true)
            {
                Client player = NAPI.Player.GetPlayerFromHandle(entity);
                if (checkpoint == NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE) && NAPI.Player.GetPlayerVehicleSeat(player) == Constants.VEHICLE_SEAT_DRIVER)
                {
                    NetHandle vehicle = NAPI.Player.GetPlayerVehicle(player);
                    int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);
                    if(GetVehicleWeaponCrates(vehicleId) > 0)
                    {
                        // Borramos el checkpoint
                        Checkpoint weaponCheckpoint= NAPI.Data.GetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE);
                        NAPI.Data.ResetEntityData(player, EntityData.PLAYER_JOB_COLSHAPE);
                        NAPI.ClientEvent.TriggerClientEvent(player, "deleteWeaponCheckpoint");
                        NAPI.Entity.DeleteEntity(weaponCheckpoint);

                        // Paramos el vehículo mientras descargan
                        NAPI.Vehicle.SetVehicleEngineStatus(vehicle, false);
                        NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE, vehicle);
                        NAPI.Data.SetEntityData(vehicle, EntityData.VEHICLE_WEAPON_UNPACKING, true);
                        vehicleWeaponTimer.Add(new Timer(OnVehicleUnpackWeapons, vehicle, 60000, Timeout.Infinite));

                        // Mandamos un aviso al jugador
                        NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WAIT_FOR_WEAPONS);
                    }
                }
            }
        }

        private void OnPlayerWeaponSwitch(Client player, WeaponHash oldWeapon, WeaponHash newWeapon)
        {
            if(NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                // Obtenemos el identificador del jugador
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_RIGHT_HAND) == true)
                {
                    int itemId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                    ItemModel item = Globals.GetItemModelFromId(itemId);
                    if (Int32.TryParse(item.hash, out int itemHash) == true)
                    {
                        ItemModel weaponItem = GetEquippedWeaponItemModelByHash(playerId, newWeapon);
                        NAPI.Player.GivePlayerWeapon(player, WeaponHash.Unarmed, 1);
                        return;
                    }
                }
                
                // Obtenemos los modelos de armas antiguo y nuevos
                ItemModel oldWeaponModel = GetEquippedWeaponItemModelByHash(playerId, oldWeapon);
                ItemModel currentWeaponModel = GetEquippedWeaponItemModelByHash(playerId, newWeapon);

                if (oldWeaponModel != null)
                {
                    // Desequipamos el arma antigua
                    oldWeaponModel.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
                    Database.UpdateItem(oldWeaponModel);
                }

                if (currentWeaponModel != null)
                {
                    // Equipamos el arma nueva
                    currentWeaponModel.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                    Database.UpdateItem(currentWeaponModel);
                }

                // Miramos si es un arma o el puño
                if (newWeapon == WeaponHash.Unarmed)
                {
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_RIGHT_HAND);
                }
                else
                {
                    NAPI.Data.SetEntityData(player, EntityData.PLAYER_RIGHT_HAND, currentWeaponModel.id);
                }
            }
        }

        private void OnPlayerDisconnected(Client player, byte type, string reason)
        {
            if(NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) == true)
            {
                // Obtenemos el id del personaje
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_ID);
                WeaponCrateModel weaponCrate = GetPlayerCarriedWeaponCrate(playerId);

                if(weaponCrate != null)
                {
                    weaponCrate.position = new Vector3(player.Position.X, player.Position.Y, player.Position.X - 1.0f);
                    weaponCrate.carriedEntity = String.Empty;
                    weaponCrate.carriedIdentifier = 0;

                    // Colocamos el objeto en su posición
                    NAPI.Entity.DetachEntity(weaponCrate.crateObject);
                    NAPI.Entity.SetEntityPosition(weaponCrate.crateObject, weaponCrate.position);
                }
            }
        }

        private static List<Vector3> GetRandomWeaponSpawns(int spawnPosition)
        {
            // Inicializamos la lista y obtenemos las posiciones
            Random random = new Random();
            List<Vector3> weaponSpawns = new List<Vector3>();
            List<CrateSpawnModel> cratesInSpawn = GetSpawnsInPosition(spawnPosition);

            while(weaponSpawns.Count < Constants.MAX_CRATES_SPAWN)
            {
                Vector3 crateSpawn = cratesInSpawn[random.Next(cratesInSpawn.Count)].position;
                if (weaponSpawns.Contains(crateSpawn) == false)
                {
                    weaponSpawns.Add(crateSpawn);
                }
            }
            return weaponSpawns;
        }

        private static List<CrateSpawnModel> GetSpawnsInPosition(int spawnPosition)
        {
            List<CrateSpawnModel> crateSpawnList = new List<CrateSpawnModel>();
            foreach(CrateSpawnModel crateSpawn in Constants.CRATE_SPAWN_LIST)
            {
                if(crateSpawn.spawnPoint == spawnPosition)
                {
                    crateSpawnList.Add(crateSpawn);
                }
            }
            return crateSpawnList;
        }
        
        private static CrateContentModel GetRandomCrateContent(int type, int chance)
        {
            CrateContentModel crateContent = new CrateContentModel();

            // Miramos el objeto a crear
            foreach(WeaponProbabilityModel weaponAmmo in Constants.WEAPON_CHANCE_LIST)
            {
                if(weaponAmmo.type == type && weaponAmmo.minChance <= chance && weaponAmmo.maxChance >= chance)
                {
                    crateContent.item = weaponAmmo.hash;
                    crateContent.amount = weaponAmmo.amount;
                    break;
                }
            }

            return crateContent;
        }

        private static void OnWeaponPrewarn(object unused)
        {
            // Limpiamos el timer
            weaponTimer.Dispose();

            int currentSpawn = 0;
            weaponCrateList = new List<WeaponCrateModel>();

            // Elegimos una localización aleatoria de las existentes
            Random random = new Random();
            int spawnPosition = random.Next(Constants.MAX_WEAPON_SPAWNS);

            // Añadimos las cajas de armas y munición
            List<Vector3> weaponSpawns = GetRandomWeaponSpawns(spawnPosition);

            // Creamos las cajas para las localizaciones dadas
            foreach (Vector3 spawn in weaponSpawns)
            {
                // Obtenemos el contenido
                int type = currentSpawn % 2;
                int chance = random.Next(type == 0 ? Constants.MAX_WEAPON_CHANCE : Constants.MAX_AMMO_CHANCE);
                CrateContentModel crateContent = GetRandomCrateContent(type, chance);

                // Creamos la caja
                WeaponCrateModel weaponCrate = new WeaponCrateModel();
                weaponCrate.contentItem = crateContent.item;
                weaponCrate.contentAmount = crateContent.amount;
                weaponCrate.position = spawn;
                weaponCrate.carriedEntity = String.Empty;
                weaponCrate.crateObject = NAPI.Object.CreateObject(481432069, spawn, new Vector3(0.0f, 0.0f, 0.0f), 0);

                // Añadimos la caja a la lista
                weaponCrateList.Add(weaponCrate);

                // Incrementamos el contador
                currentSpawn++;
            }

            // Avisamos del punto a todas las facciones
            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) > Constants.LAST_STATE_FACTION)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WEAPON_SPAWN_ISLAND);
                }
            }

            // Creamos un temporizador para avisar a la policía
            weaponTimer = new Timer(OnPoliceCalled, null, 240000, Timeout.Infinite);
        }

        private static void OnPoliceCalled(object unused)
        {
            // Limpiamos el timer
            weaponTimer.Dispose();

            // Avisamos a la facción de policía
            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (NAPI.Data.HasEntityData(player, EntityData.PLAYER_PLAYING) && NAPI.Data.GetEntityData(player, EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WEAPON_SPAWN_ISLAND);
                }
            }

            // Creamos un temporizador para avisar a la policía
            weaponTimer = new Timer(OnWeaponEventFinished, null, 3600000, Timeout.Infinite);
        }

        private static void OnVehicleUnpackWeapons(object vehicleObject)
        {
            NetHandle vehicle = (NetHandle)vehicleObject;
            int vehicleId = NAPI.Data.GetEntityData(vehicle, EntityData.VEHICLE_ID);

            // Desempaquetamos las armas
            foreach(WeaponCrateModel weaponCrate in weaponCrateList)
            {
                if(weaponCrate.carriedEntity == Constants.ITEM_ENTITY_VEHICLE && weaponCrate.carriedIdentifier == vehicleId)
                {
                    // Creamos el arma del cajón
                    ItemModel item = new ItemModel();
                    item.hash = weaponCrate.contentItem;
                    item.amount = weaponCrate.contentAmount;
                    item.ownerEntity = Constants.ITEM_ENTITY_VEHICLE;
                    item.ownerIdentifier = vehicleId;
                    item.id = Database.AddNewItem(item);
                    Globals.itemList.Add(item);

                    // Eliminamos la caja
                    weaponCrate.carriedIdentifier = 0;
                    weaponCrate.carriedEntity = String.Empty;
                }
            }

            // Avisamos al conductor de que ya está disponible
            foreach(Client player in NAPI.Pools.GetAllPlayers())
            {
                if(NAPI.Data.GetEntityData(player, EntityData.PLAYER_VEHICLE) == vehicle)
                {
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_VEHICLE);
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_WEAPONS_UNPACKED);
                    break;
                }
            }

            // Desbloqueamos el vehículo
            NAPI.Data.ResetEntityData(vehicle, EntityData.VEHICLE_WEAPON_UNPACKING);
        }

        private static void OnWeaponEventFinished(object unused)
        {
            // Limpiamos el timer
            weaponTimer.Dispose();

            foreach (WeaponCrateModel crate in weaponCrateList)
            {
                if (NAPI.Entity.DoesEntityExist(crate.crateObject) == true)
                {
                    NAPI.Entity.DeleteEntity(crate.crateObject);
                }
            }

            // Borramos todas las cajas de armas
            weaponCrateList = new List<WeaponCrateModel>();
            weaponTimer = null;
        }

        private int GetVehicleWeaponCrates(int vehicleId)
        {
            int crates = 0;
            foreach(WeaponCrateModel weaponCrate in weaponCrateList)
            {
                if(weaponCrate.carriedEntity == Constants.ITEM_ENTITY_VEHICLE && weaponCrate.carriedIdentifier == vehicleId)
                {
                    crates++;
                }
            }
            return crates;
        }
        
        [Command("armarios")]
        public void ArmariosCommand(Client player)
        {
            if(NAPI.Data.GetEntityData(player, EntityData.PLAYER_ADMIN_RANK) > Constants.STAFF_S_GAME_MASTER)
            {
                if (weaponTimer == null)
                {
                    WeaponsPrewarn();
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ADMIN_INFO + Messages.ADM_WEAPON_EVENT_STARTED);
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_WEAPON_EVENT_ON_COURSE);
                }
            }
        }
    }
}
