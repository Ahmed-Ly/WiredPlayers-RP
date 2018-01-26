using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.database;
using WiredPlayers.globals;
using System.Collections.Generic;
using System.Threading;
using System;

namespace WiredPlayers.login
{
    public class Login : Script
    {
        private static Dictionary<String, Timer> spawnTimerList = new Dictionary<String, Timer>();
        private static Dictionary<String, Timer> loginTimerList = new Dictionary<String, Timer>();

        public Login()
        {
            Event.OnPlayerConnected += OnPlayerConnected;
            Event.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        private void OnPlayerConnected(Client player, CancelEventArgs cancel)
        {
            InitializePlayerData(player);
            AccountModel account = Database.GetAccount(player.SocialClubName);
            
            switch (account.status)
            {
                case -1:
                    NAPI.Player.KickPlayer(player, Messages.INF_ACCOUNT_DISABLED);
                    break;
                case 0:
                    NAPI.Player.KickPlayer(player, Messages.INF_ACCOUNT_NEW);
                    break;
                default:
                    // Mandamos los mensajes de bienvenida
                    NAPI.Chat.SendChatMessageToPlayer(player, "Bienvenido a WiredPlayers, " + player.SocialClubName);
                    NAPI.Chat.SendChatMessageToPlayer(player, "Utiliza el comando ~b~/bienvenida ~w~para saber como puedes empezar tu vida en Los Santos.");
                    NAPI.Chat.SendChatMessageToPlayer(player, "Utiliza el comando ~b~/ayuda ~w~siempre que quieras para obtener información general.");
                    NAPI.Chat.SendChatMessageToPlayer(player, "Utiliza el comando ~b~/duda ~w~para solicitar ayuda de algún miembro del staff.");

                    // Miramos si tiene seleccionado algún personaje
                    if (account.lastCharacter > 0)
                    {
                        PlayerModel character = Database.LoadCharacterInformationById(account.lastCharacter);
                        SkinModel skin = Database.GetCharacterSkin(account.lastCharacter);

                        // Carga de skin hombre/mujer
                        String pedModel = character.sex == 0 ? Constants.MALE_PED_MODEL : Constants.FEMALE_PED_MODEL;
                        PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                        NAPI.Player.SetPlayerName(player, character.realName);
                        NAPI.Player.SetPlayerSkin(player, pedHash);

                        // Cargamos los datos básicos del personaje
                        LoadCharacterData(player, character);

                        // Generación del modelo del personaje
                        PopulateCharacterSkin(player, skin);

                        // Generación de la ropa del personaje
                        Globals.PopulateCharacterClothes(player);

                        // Añadimos la vida y chaleco
                        NAPI.Player.SetPlayerHealth(player, character.health);
                        NAPI.Player.SetPlayerArmor(player, character.armor);
                    }
                    else
                    {
                        PedHash pedHash = NAPI.Util.PedNameToModel(Constants.DEFAULT_PED_MODEL);
                        NAPI.Player.SetPlayerSkin(player, pedHash);
                    }

                    // Cerramos las puertas de comisaría
                    //NAPI.Native.SendNativeToPlayer(player, Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, 320433149, 434.7479f, -983.2151f, 30.83926f, true, 0f, false);
                    //NAPI.Native.SendNativeToPlayer(player, Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, -1215222675, 434.7479f, -980.6184f, 30.83926f, true, 0f, false);
                    //NAPI.Native.SendNativeToPlayer(player, Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, -2023754432, 469.9679f, -1014.452f, 26.53623f, true, 0f, false);
                    //NAPI.Native.SendNativeToPlayer(player, Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, -2023754432, 467.3716f, -1014.452f, 26.53623f, true, 0f, false);

                    // Cerramos las celdas de comisaria
                    //NAPI.Native.SendNativeToPlayer(player, Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, 631614199, 461.8065f, -994.4086f, 25.06443f, true, 0f, false);
                    //NAPI.Native.SendNativeToPlayer(player, Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, 631614199, 461.8065f, -997.6583f, 25.06443f, true, 0f, false);
                    //NAPI.Native.SendNativeToPlayer(player, Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE, 631614199, 461.8065f, -1001.302f, 25.06443f, true, 0f, false);
                    
                    // Hacemos al personaje visible
                    NAPI.Entity.SetEntityTransparency(player, 255);

                    // Mostramos el login
                    NAPI.ClientEvent.TriggerClientEvent(player, "accountLoginForm");

                    // Añadimos el timer por si no carga el CEF
                    Timer loginTimer = new Timer(OnPlayerLoginTimeoutTimer, player, 7500, Timeout.Infinite);
                    loginTimerList.Add(player.SocialClubName, loginTimer);
                    break;
            }

            // Cancelamos el spawn automático
            cancel.Spawn = false;
        }

        private void OnPlayerDisconnected(Client player, byte type, string reason)
        {
            if (spawnTimerList.TryGetValue(player.SocialClubName, out Timer spawnTimer) == true)
            {
                // Eliminamos el timer
                spawnTimer.Dispose();
                spawnTimerList.Remove(player.SocialClubName);
            }

            if (loginTimerList.TryGetValue(player.SocialClubName, out Timer loginTimer) == true)
            {
                // Eliminamos el timer
                loginTimer.Dispose();
                loginTimerList.Remove(player.SocialClubName);
            }
        }

        private void InitializePlayerData(Client player)
        {
            Vector3 spawn = new Vector3(152.26, -1004.47, -99.00);
            Vector3 worldSpawn = new Vector3(200.6641f, -932.0939f, 30.68681f);
            Vector3 rotation = new Vector3(0.0f, 0.0f, 0.0f);
            uint dimension = Convert.ToUInt32(player.Value);
            NAPI.Entity.SetEntityPosition(player, spawn);
            NAPI.Entity.SetEntityDimension(player, dimension);

            NAPI.Player.SetPlayerHealth(player, 100);
            NAPI.Player.SetPlayerArmor(player, 0);
            NAPI.Player.FreezePlayer(player, true);
            NAPI.Entity.SetEntityTransparency(player, 0);
            NAPI.Entity.SetEntityInvincible(player, true);
            
            // Eliminamos el id del jugador
            NAPI.Data.ResetEntityData(player, EntityData.PLAYER_ID);

            // Limpiamos las armas que pueda tener
            NAPI.Player.RemoveAllPlayerWeapons(player);

            // Inicialización de los entity data sincronizados
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SEX, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_AGE, 14);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, 3500);

            // Inicialización de los entity data del servidor
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_NAME, String.Empty);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ADMIN_RANK, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ADMIN_NAME, String.Empty);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_SPAWN_POS, worldSpawn);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_SPAWN_ROT, rotation);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RADIO, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_KILLED, 0); 
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JAILED, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JAIL_TYPE, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_FACTION, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RANK, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ON_DUTY, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RENT_HOUSE, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DOCUMENTATION, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS, "0,0,0,0,0");
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_POINTS, "0,0,0,0,0,0,0");
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSES, "-1,-1,-1");
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_WEAPON_LICENSE, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_DELIVER, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PLAYED, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_STATUS, 0);
        }

        private void LoadCharacterData(Client player, PlayerModel character)
        {
            // Cargamos el tipo y tiempo de cárcel
            String[] jail = character.jailed.Split(',');

            // Carga de los datos básicos del personaje compartidos
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, character.money);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, character.bank);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_AGE, character.age);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SEX, character.sex);

            // Carga de los datos básicos del personaje normales
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_SQL_ID, character.id);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_NAME, character.realName);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ADMIN_RANK, character.adminRank);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ADMIN_NAME, character.adminName);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_SPAWN_POS, character.position);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_SPAWN_ROT, character.rotation);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PHONE, character.phone);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RADIO, character.radio);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_KILLED, character.killed);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JAIL_TYPE, Int32.Parse(jail[0]));
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JAILED, Int32.Parse(jail[1]));
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_FACTION, character.faction);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB, character.job);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RANK, character.rank);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ON_DUTY, character.duty);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_VEHICLE_KEYS, character.carKeys);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_DOCUMENTATION, character.documentation);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_LICENSES, character.licenses);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE, character.insurance);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_WEAPON_LICENSE, character.weaponLicense);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_RENT_HOUSE, character.houseRent);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_HOUSE_ENTERED, character.houseEntered);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_BUSINESS_ENTERED, character.businessEntered);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN, character.employeeCooldown);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN, character.jobCooldown);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_DELIVER, character.jobDeliver);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_POINTS, character.jobPoints);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PLAYED, character.played);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_STATUS, character.status);
        }
        
        private void PopulateCharacterSkin(Client player, SkinModel skinModel)
        {
            // Generación de la cara
            NAPI.Data.SetEntityData(player, EntityData.GTAO_SHAPE_FIRST_ID, skinModel.firstHeadShape);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_SHAPE_SECOND_ID, skinModel.secondHeadShape);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_SKIN_FIRST_ID, skinModel.firstSkinTone);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_SKIN_SECOND_ID, skinModel.secondSkinTone);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_FACE_MIX, skinModel.headMix);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_SKIN_MIX, skinModel.skinMix);

            // Generación del peinado
            NAPI.Data.SetEntityData(player, EntityData.GTAO_HAIR_MODEL, skinModel.hairModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_HAIR_FIRST_COLOR, skinModel.firstHairColor);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_HAIR_SECOND_COLOR, skinModel.secondHairColor);

            // Generación de la barba
            NAPI.Data.SetEntityData(player, EntityData.GTAO_BEARD_MODEL, skinModel.beardModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_BEARD_COLOR, skinModel.beardColor);

            // Generación del vello del pecho
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHEST_MODEL, skinModel.chestModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHEST_COLOR, skinModel.chestColor);

            // Generación del vello del pecho
            NAPI.Data.SetEntityData(player, EntityData.GTAO_BLEMISHES_MODEL, skinModel.blemishesModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_AGEING_MODEL, skinModel.ageingModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_COMPLEXION_MODEL, skinModel.complexionModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_SUNDAMAGE_MODEL, skinModel.sundamageModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_FRECKLES_MODEL, skinModel.frecklesModel);

            // Generación de ojos y cejas
            NAPI.Data.SetEntityData(player, EntityData.GTAO_EYES_COLOR, skinModel.eyesColor);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_EYEBROWS_MODEL, skinModel.eyebrowsModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_EYEBROWS_COLOR, skinModel.eyebrowsColor);

            // Generación del maquillaje
            NAPI.Data.SetEntityData(player, EntityData.GTAO_MAKEUP_MODEL, skinModel.makeupModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_BLUSH_MODEL, skinModel.blushModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_BLUSH_COLOR, skinModel.blushColor);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_LIPSTICK_MODEL, skinModel.lipstickModel);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_LIPSTICK_COLOR, skinModel.lipstickColor);

            // Generación de rasgos faciales avanzados
            NAPI.Data.SetEntityData(player, EntityData.GTAO_NOSEWIDTH_MODEL, skinModel.noseWidth);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_NOSEHEIGHT_MODEL, skinModel.noseHeight);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_NOSELENGTH_MODEL, skinModel.noseLength);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_NOSEBRIDGE_MODEL, skinModel.noseBridge);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_NOSETIP_MODEL, skinModel.noseTip);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_NOSESHIFT_MODEL, skinModel.noseShift);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_BROWHEIGHT_MODEL, skinModel.browHeight);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_BROWWIDTH_MODEL, skinModel.browWidth);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHEEKBONEHEIGHT_MODEL, skinModel.cheekboneHeight);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHEEKBONEWIDTH_MODEL, skinModel.cheekboneWidth);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHEEKSWIDTH_MODEL, skinModel.cheeksWidth);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_EYES_MODEL, skinModel.eyes);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_LIPS_MODEL, skinModel.lips);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_JAWWIDTH_MODEL, skinModel.jawWidth);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_JAWHEIGHT_MODEL, skinModel.jawHeight);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHINLENGTH_MODEL, skinModel.chinLength);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHINPOSITION_MODEL, skinModel.chinPosition);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHINWIDTH_MODEL, skinModel.chinWidth);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_CHINSHAPE_MODEL, skinModel.chinShape);
            NAPI.Data.SetEntityData(player, EntityData.GTAO_NECKWIDTH_MODEL, skinModel.neckWidth);

            // Llamamos a la función para actualizar el modelo
            Timer spawnTimer = new Timer(OnPlayerUpdateTimer, player, 350, Timeout.Infinite);
            spawnTimerList.Add(player.SocialClubName, spawnTimer);
        }

        private SkinModel GetPlayerCustomSkin(Client player)
        {
            SkinModel skinModel = new SkinModel();

            // Obtención de la cara
            skinModel.firstHeadShape = NAPI.Data.GetEntityData(player, EntityData.GTAO_SHAPE_FIRST_ID);
            skinModel.secondHeadShape = NAPI.Data.GetEntityData(player, EntityData.GTAO_SHAPE_SECOND_ID);
            skinModel.firstSkinTone = NAPI.Data.GetEntityData(player, EntityData.GTAO_SKIN_FIRST_ID);
            skinModel.secondSkinTone = NAPI.Data.GetEntityData(player, EntityData.GTAO_SKIN_SECOND_ID);
            skinModel.headMix = NAPI.Data.GetEntityData(player, EntityData.GTAO_FACE_MIX);
            skinModel.skinMix = NAPI.Data.GetEntityData(player, EntityData.GTAO_SKIN_MIX);

            // Obtención del peinado
            skinModel.hairModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_HAIR_MODEL);
            skinModel.firstHairColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_HAIR_FIRST_COLOR);
            skinModel.secondHairColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_HAIR_SECOND_COLOR);

            // Obtención de la barba
            skinModel.beardModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_BEARD_MODEL);
            skinModel.beardColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_BEARD_COLOR);

            // Obtención del vello del pecho
            skinModel.chestModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHEST_MODEL);
            skinModel.chestColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHEST_COLOR);

            // Obtención del vello del pecho
            skinModel.blemishesModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_BLEMISHES_MODEL);
            skinModel.ageingModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_AGEING_MODEL);
            skinModel.complexionModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_COMPLEXION_MODEL);
            skinModel.sundamageModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_SUNDAMAGE_MODEL);
            skinModel.frecklesModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_FRECKLES_MODEL);

            // Obtención de ojos y cejas
            skinModel.eyesColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_EYES_COLOR);
            skinModel.eyebrowsModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_EYEBROWS_MODEL);
            skinModel.eyebrowsColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_EYEBROWS_COLOR);

            // Obtención del maquillaje
            skinModel.makeupModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_MAKEUP_MODEL);
            skinModel.blushModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_BLUSH_MODEL);
            skinModel.blushColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_BLUSH_COLOR);
            skinModel.lipstickModel = NAPI.Data.GetEntityData(player, EntityData.GTAO_LIPSTICK_MODEL);
            skinModel.lipstickColor = NAPI.Data.GetEntityData(player, EntityData.GTAO_LIPSTICK_COLOR);

            // Obtención de rasgos faciales avanzados
            skinModel.noseWidth = NAPI.Data.GetEntityData(player, EntityData.GTAO_NOSEWIDTH_MODEL);
            skinModel.noseHeight = NAPI.Data.GetEntityData(player, EntityData.GTAO_NOSEHEIGHT_MODEL);
            skinModel.noseLength = NAPI.Data.GetEntityData(player, EntityData.GTAO_NOSELENGTH_MODEL);
            skinModel.noseBridge = NAPI.Data.GetEntityData(player, EntityData.GTAO_NOSEBRIDGE_MODEL);
            skinModel.noseTip = NAPI.Data.GetEntityData(player, EntityData.GTAO_NOSETIP_MODEL);
            skinModel.noseShift = NAPI.Data.GetEntityData(player, EntityData.GTAO_NOSESHIFT_MODEL);
            skinModel.browHeight = NAPI.Data.GetEntityData(player, EntityData.GTAO_BROWHEIGHT_MODEL);
            skinModel.browWidth = NAPI.Data.GetEntityData(player, EntityData.GTAO_BROWWIDTH_MODEL);
            skinModel.cheekboneHeight = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHEEKBONEHEIGHT_MODEL);
            skinModel.cheekboneWidth = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHEEKBONEWIDTH_MODEL);
            skinModel.cheeksWidth = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHEEKSWIDTH_MODEL);
            skinModel.eyes = NAPI.Data.GetEntityData(player, EntityData.GTAO_EYES_MODEL);
            skinModel.lips = NAPI.Data.GetEntityData(player, EntityData.GTAO_LIPS_MODEL);
            skinModel.jawWidth = NAPI.Data.GetEntityData(player, EntityData.GTAO_JAWWIDTH_MODEL);
            skinModel.jawHeight = NAPI.Data.GetEntityData(player, EntityData.GTAO_JAWHEIGHT_MODEL);
            skinModel.chinLength = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHINLENGTH_MODEL);
            skinModel.chinPosition = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHINPOSITION_MODEL);
            skinModel.chinWidth = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHINWIDTH_MODEL);
            skinModel.chinShape = NAPI.Data.GetEntityData(player, EntityData.GTAO_CHINSHAPE_MODEL);
            skinModel.neckWidth = NAPI.Data.GetEntityData(player, EntityData.GTAO_NECKWIDTH_MODEL);

            return skinModel;
        }
        
        public void OnPlayerUpdateTimer(object playerObject)
        {
            try
            {
                Client player = (Client)playerObject;
                SkinModel playerCustomSkin = GetPlayerCustomSkin(player);
                int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
                List<TattooModel> playerTattooList = Globals.GetPlayerTattoos(playerId);

                // Borramos el timer de la lista
                Timer spawnTimer = spawnTimerList[player.SocialClubName];
                if(spawnTimer != null)
                {
                    spawnTimer.Dispose();
                    spawnTimerList.Remove(player.SocialClubName);
                }

                // Llamamos al evento del cliente
                NAPI.ClientEvent.TriggerClientEvent(player, "updatePlayerCustomSkin", player, NAPI.Util.ToJson(playerCustomSkin), NAPI.Util.ToJson(playerTattooList));
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnPlayerUpdateTimer] " + ex.Message);
            }
        }

        public void OnPlayerLoginTimeoutTimer(object playerObject)
        {
            try
            {
                Client player = (Client)playerObject;
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_COMMAND_LOGIN, true);
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + "Si aún no te ha aparecido la pantalla de login, puedes usar el comando /login para acceder.");

                // Borramos el timer de la lista
                Timer loginTimer = loginTimerList[player.SocialClubName];
                if (loginTimer != null)
                {
                    loginTimer.Dispose();
                    loginTimerList.Remove(player.SocialClubName);
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("[EXCEPTION OnPlayerLoginTimeoutTimer] " + ex.Message);
            }
        }

        [RemoteEvent("registerAccount")]
        public void RegisterAccountEvent(Client player, params object[] arguments)
        {
            String forumName = (String)arguments[0];
            String password = (String)arguments[1];
            NAPI.Player.FreezePlayer(player, false);
            NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_SUCCESS + Messages.SUC_ACCOUNT_REGISTER);
        }

        [RemoteEvent("loginAccount")]
        public void LoginAccountEvent(Client player, params object[] arguments)
        {
            String password = (String)arguments[0];
            bool login = Database.LoginAccount(player.SocialClubName, password);
            if (login)
            {
                // Borramos el timer de la lista
                if (loginTimerList.TryGetValue(player.SocialClubName, out Timer loginTimer) == true)
                {
                    loginTimer.Dispose();
                    loginTimerList.Remove(player.SocialClubName);
                }

                NAPI.Player.FreezePlayer(player, false);
                NAPI.ClientEvent.TriggerClientEvent(player, "clearLoginWindow");
            }
            else
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "showLoginError");
            }
        }

        [RemoteEvent("getPlayerCharacters")]
        public void GetPlayerCharactersEvent(Client player, params object[] arguments)
        {
            List<String> playerList = Database.GetAccountCharacters(player.SocialClubName);
            String jsonList = NAPI.Util.ToJson(playerList);
            NAPI.Player.FreezePlayer(player, true);
            NAPI.Player.StopPlayerAnimation(player);
            NAPI.Player.SetPlayerDefaultClothes(player);
            NAPI.ClientEvent.TriggerClientEvent(player, "showPlayersMenu", jsonList);
        }

        [RemoteEvent("unfreezePlayer")]
        public void UnfreezePlayerEvent(Client player, params object[] arguments)
        {
            // Descongelamos al jugador
            NAPI.Player.FreezePlayer(player, false);
        }

        [RemoteEvent("changeCharacterSex")]
        public void ChangeCharacterSexEvent(Client player, params object[] arguments)
        {
            String pedModel = arguments[0].ToString() == "1" ? Constants.FEMALE_PED_MODEL : Constants.MALE_PED_MODEL;
            PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
            NAPI.Player.SetPlayerSkin(player, pedHash);

            // Eliminamos la ropa
            NAPI.Player.SetPlayerClothes(player, 11, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 3, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 8, 15, 0);
        }

        [RemoteEvent("createCharacter")]
        public void CreateCharacterEvent(Client player, params object[] arguments)
        {
            // Recuperamos el nombre y edad
            String playerName = arguments[0].ToString();
            int playerAge = Int32.Parse(arguments[1].ToString());

            PlayerModel playerModel = new PlayerModel();
            SkinModel skinModel = (SkinModel)NAPI.Util.FromJson(arguments[2].ToString());

            // Generación del modelo del personaje
            playerModel.realName = playerName;
            playerModel.age = playerAge;
            playerModel.sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);

            // Generación del modelo del personaje
            PopulateCharacterSkin(player, skinModel);

            int playerId = Database.CreateCharacter(player, playerModel, skinModel);
            if (playerId > 0)
            {
                InitializePlayerData(player);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_SQL_ID, playerId);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_NAME, playerName);
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_AGE, playerAge);
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SPAWN_POS, new Vector3(200.6641f, -932.0939f, 30.6868f));
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SPAWN_ROT, new Vector3(0.0f, 0.0f, 0.0f));
                Database.UpdateLastCharacter(player.SocialClubName, playerId);
                NAPI.ClientEvent.TriggerClientEvent(player, "characterCreatedSuccessfully");
                NAPI.Player.FreezePlayer(player, false);
            }
        }

        [RemoteEvent("setCharacterIntoCreator")]
        public void SetCharacterIntoCreatorEvent(Client player, params object[] arguments)
        {
            // Cambiamos el skin del personaje al por defecto
            NAPI.Player.SetPlayerSkin(player, PedHash.FreemodeMale01);

            // Eliminamos la ropa
            NAPI.Player.SetPlayerClothes(player, 11, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 3, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 8, 15, 0);

            // Establecemos su posición
            NAPI.Entity.SetEntityRotation(player, new Vector3(0.0f, 0.0f, 180.0f));
            NAPI.Entity.SetEntityPosition(player, new Vector3(152.3787f, -1000.644f, -99f));

            // Aplicamos la animación para que se esté quieto
            NAPI.Player.PlayPlayerAnimation(player, (int)(Constants.AnimationFlags.StopOnLastFrame), "mp_ped_interaction", "hugs_guy_a");

            // Cargamos el menú de edición
            NAPI.ClientEvent.TriggerClientEvent(player, "showCharacterCreationMenu");
        }

        [RemoteEvent("loadCharacter")]
        public void LoadCharacterEvent(Client player, params object[] arguments)
        {
            PlayerModel playerModel = Database.LoadCharacterInformationByName(arguments[0].ToString());
            SkinModel skinModel = Database.GetCharacterSkin(playerModel.id);

            // Carga de skin hombre/mujer
            String pedModel = playerModel.sex == 0 ? Constants.MALE_PED_MODEL : Constants.FEMALE_PED_MODEL;
            PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
            NAPI.Player.SetPlayerName(player, playerModel.realName);
            NAPI.Player.SetPlayerSkin(player, pedHash);

            // Cargamos los datos básicos del personaje
            LoadCharacterData(player, playerModel);

            // Generación del modelo del personaje
            PopulateCharacterSkin(player, skinModel);

            // Generación de la ropa del personaje
            Globals.PopulateCharacterClothes(player);

            // Añadimos la vida y chaleco
            NAPI.Player.SetPlayerHealth(player, playerModel.health);
            NAPI.Player.SetPlayerArmor(player, playerModel.armor);

            Database.UpdateLastCharacter(player.SocialClubName, playerModel.id);
            NAPI.Player.FreezePlayer(player, false);
        }

        [RemoteEvent("getPlayerCustomSkin")]
        public void GetPlayerCustomSkinEvent(Client player, params object[] arguments)
        {
            Client target = (Client)arguments[0];

            SkinModel targetCustomSkin = GetPlayerCustomSkin(target);
            int targetId = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
            List<TattooModel> targetTattooList = Globals.GetPlayerTattoos(targetId);

            // Llamamos al evento del cliente
            NAPI.ClientEvent.TriggerClientEvent(player, "updatePlayerCustomSkin", target, NAPI.Util.ToJson(targetCustomSkin), NAPI.Util.ToJson(targetTattooList));
        }

        [Command("login", Messages.GEN_LOGIN_COMMAND)]
        public void LoginCommand(Client player, String password)
        {
            if(NAPI.Data.HasEntityData(player, EntityData.PLAYER_COMMAND_LOGIN) == true)
            {
                bool login = Database.LoginAccount(player.SocialClubName, password);
                if (login)
                {
                    NAPI.Player.FreezePlayer(player, false);
                    NAPI.Data.ResetEntityData(player, EntityData.PLAYER_COMMAND_LOGIN);
                    NAPI.ClientEvent.TriggerClientEvent(player, "clearLoginWindow");
                }
                else
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "showLoginError");
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_ERROR + Messages.ERR_PLAYER_CANT_LOGIN);
            }
        }
    }
}