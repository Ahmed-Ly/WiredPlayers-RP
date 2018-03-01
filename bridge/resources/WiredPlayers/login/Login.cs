using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.database;
using WiredPlayers.globals;
using System.Collections.Generic;
using System.Threading;
using System;
using Newtonsoft.Json;

namespace WiredPlayers.login
{
    public class Login : Script
    {
        private static Dictionary<String, Timer> spawnTimerList = new Dictionary<String, Timer>();

        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            if (spawnTimerList.TryGetValue(player.SocialClubName, out Timer spawnTimer) == true)
            {
                spawnTimer.Dispose();
                spawnTimerList.Remove(player.SocialClubName);
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
            NAPI.Entity.SetEntityTransparency(player, 0);

            // Clear weapons
            NAPI.Player.RemoveAllPlayerWeapons(player);

            // Initialize shared entity data
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SEX, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_AGE, 18);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, 3500);

            // Initialize entity data
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
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ROLE_POINTS, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_MEDICAL_INSURANCE, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_WEAPON_LICENSE, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_COOLDOWN, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_EMPLOYEE_COOLDOWN, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_JOB_DELIVER, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PLAYED, 0);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_STATUS, 0);
        }

        private void InitializePlayerSkin(Client player)
        {
            NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_HEAD_SHAPE, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_HEAD_SHAPE, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_SKIN_TONE, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_SKIN_TONE, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.HEAD_MIX, 0.5f);
            NAPI.Data.SetEntitySharedData(player, EntityData.SKIN_MIX, 0.5f);

            // Hair generation
            NAPI.Data.SetEntitySharedData(player, EntityData.HAIR_MODEL, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_HAIR_COLOR, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_HAIR_COLOR, 0);

            // Beard generation
            NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_MODEL, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_COLOR, 0);

            // Chest hair generation
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEST_MODEL, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEST_COLOR, 0);

            // Face marks generation
            NAPI.Data.SetEntitySharedData(player, EntityData.BLEMISHES_MODEL, -1);
            NAPI.Data.SetEntitySharedData(player, EntityData.AGEING_MODEL, -1);
            NAPI.Data.SetEntitySharedData(player, EntityData.COMPLEXION_MODEL, -1);
            NAPI.Data.SetEntitySharedData(player, EntityData.SUNDAMAGE_MODEL, -1);
            NAPI.Data.SetEntitySharedData(player, EntityData.FRECKLES_MODEL, -1);

            // Eyes and eyebrows generation
            NAPI.Data.SetEntitySharedData(player, EntityData.EYES_COLOR, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_MODEL, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_COLOR, 0);

            // Cosmetics generation
            NAPI.Data.SetEntitySharedData(player, EntityData.MAKEUP_MODEL, -1);
            NAPI.Data.SetEntitySharedData(player, EntityData.BLUSH_MODEL, -1);
            NAPI.Data.SetEntitySharedData(player, EntityData.BLUSH_COLOR, 0);
            NAPI.Data.SetEntitySharedData(player, EntityData.LIPSTICK_MODEL, -1);
            NAPI.Data.SetEntitySharedData(player, EntityData.LIPSTICK_COLOR, 0);

            // Advanced facial model generation
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_WIDTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_HEIGHT, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_LENGTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_BRIDGE, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_TIP, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_SHIFT, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.BROW_HEIGHT, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.BROW_WIDTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEEKBONE_HEIGHT, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEEKBONE_WIDTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEEKS_WIDTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.EYES, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.LIPS, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.JAW_WIDTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.JAW_HEIGHT, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_LENGTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_POSITION, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_WIDTH, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_SHAPE, 0.0f);
            NAPI.Data.SetEntitySharedData(player, EntityData.NECK_WIDTH, 0.0f);
        }

        private void LoadCharacterData(Client player, PlayerModel character)
        {
            String[] jail = character.jailed.Split(',');
            
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_MONEY, character.money);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_BANK, character.bank);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_AGE, character.age);
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SEX, character.sex);
            
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_SQL_ID, character.id);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_NAME, character.realName);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_HEALTH, character.health);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ARMOR, character.armor);
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
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_ROLE_POINTS, character.rolePoints);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_PLAYED, character.played);
            NAPI.Data.SetEntityData(player, EntityData.PLAYER_STATUS, character.status);
        }

        private void PopulateCharacterSkin(Client player, SkinModel skinModel)
        {
            NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_HEAD_SHAPE, skinModel.firstHeadShape);
            NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_HEAD_SHAPE, skinModel.secondHeadShape);
            NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_SKIN_TONE, skinModel.firstSkinTone);
            NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_SKIN_TONE, skinModel.secondSkinTone);
            NAPI.Data.SetEntitySharedData(player, EntityData.HEAD_MIX, skinModel.headMix);
            NAPI.Data.SetEntitySharedData(player, EntityData.SKIN_MIX, skinModel.skinMix);
            
            NAPI.Data.SetEntitySharedData(player, EntityData.HAIR_MODEL, skinModel.hairModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.FIRST_HAIR_COLOR, skinModel.firstHairColor);
            NAPI.Data.SetEntitySharedData(player, EntityData.SECOND_HAIR_COLOR, skinModel.secondHairColor);
            
            NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_MODEL, skinModel.beardModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.BEARD_COLOR, skinModel.beardColor);
            
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEST_MODEL, skinModel.chestModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEST_COLOR, skinModel.chestColor);
            
            NAPI.Data.SetEntitySharedData(player, EntityData.BLEMISHES_MODEL, skinModel.blemishesModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.AGEING_MODEL, skinModel.ageingModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.COMPLEXION_MODEL, skinModel.complexionModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.SUNDAMAGE_MODEL, skinModel.sundamageModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.FRECKLES_MODEL, skinModel.frecklesModel);
            
            NAPI.Data.SetEntitySharedData(player, EntityData.EYES_COLOR, skinModel.eyesColor);
            NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_MODEL, skinModel.eyebrowsModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.EYEBROWS_COLOR, skinModel.eyebrowsColor);
            
            NAPI.Data.SetEntitySharedData(player, EntityData.MAKEUP_MODEL, skinModel.makeupModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.BLUSH_MODEL, skinModel.blushModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.BLUSH_COLOR, skinModel.blushColor);
            NAPI.Data.SetEntitySharedData(player, EntityData.LIPSTICK_MODEL, skinModel.lipstickModel);
            NAPI.Data.SetEntitySharedData(player, EntityData.LIPSTICK_COLOR, skinModel.lipstickColor);
            
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_WIDTH, skinModel.noseWidth);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_HEIGHT, skinModel.noseHeight);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_LENGTH, skinModel.noseLength);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_BRIDGE, skinModel.noseBridge);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_TIP, skinModel.noseTip);
            NAPI.Data.SetEntitySharedData(player, EntityData.NOSE_SHIFT, skinModel.noseShift);
            NAPI.Data.SetEntitySharedData(player, EntityData.BROW_HEIGHT, skinModel.browHeight);
            NAPI.Data.SetEntitySharedData(player, EntityData.BROW_WIDTH, skinModel.browWidth);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEEKBONE_HEIGHT, skinModel.cheekboneHeight);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEEKBONE_WIDTH, skinModel.cheekboneWidth);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHEEKS_WIDTH, skinModel.cheeksWidth);
            NAPI.Data.SetEntitySharedData(player, EntityData.EYES, skinModel.eyes);
            NAPI.Data.SetEntitySharedData(player, EntityData.LIPS, skinModel.lips);
            NAPI.Data.SetEntitySharedData(player, EntityData.JAW_WIDTH, skinModel.jawWidth);
            NAPI.Data.SetEntitySharedData(player, EntityData.JAW_HEIGHT, skinModel.jawHeight);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_LENGTH, skinModel.chinLength);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_POSITION, skinModel.chinPosition);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_WIDTH, skinModel.chinWidth);
            NAPI.Data.SetEntitySharedData(player, EntityData.CHIN_SHAPE, skinModel.chinShape);
            NAPI.Data.SetEntitySharedData(player, EntityData.NECK_WIDTH, skinModel.neckWidth);

            // Timer to update player's face model
            Timer spawnTimer = new Timer(OnPlayerUpdateTimer, player, 350, Timeout.Infinite);
            spawnTimerList.Add(player.SocialClubName, spawnTimer);
        }

        public void OnPlayerUpdateTimer(object playerObject)
        {
            Client player = (Client)playerObject;
            int playerId = NAPI.Data.GetEntityData(player, EntityData.PLAYER_SQL_ID);
            List<TattooModel> playerTattooList = Globals.GetPlayerTattoos(playerId);
            
            Timer spawnTimer = spawnTimerList[player.SocialClubName];
            if (spawnTimer != null)
            {
                spawnTimer.Dispose();
                spawnTimerList.Remove(player.SocialClubName);
            }

            // Update player's face model
            NAPI.ClientEvent.TriggerClientEvent(player, "updatePlayerCustomSkin", player, NAPI.Util.ToJson(playerTattooList));
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Client player)
        {
            // Initialize the player
            InitializePlayerData(player);
            InitializePlayerSkin(player);

            AccountModel account = Database.GetAccount(player.SocialClubName);

            switch (account.status)
            {
                case -1:
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ACCOUNT_DISABLED);
                    NAPI.Player.KickPlayer(player, Messages.INF_ACCOUNT_DISABLED);
                    break;
                case 0:
                    NAPI.Chat.SendChatMessageToPlayer(player, Constants.COLOR_INFO + Messages.INF_ACCOUNT_NEW);
                    NAPI.Player.KickPlayer(player, Messages.INF_ACCOUNT_NEW);
                    break;
                default:
                    // Welcome message
                    String welcomeMessage = String.Format(Messages.GEN_WELCOME_MESSAGE, player.SocialClubName);
                    NAPI.Chat.SendChatMessageToPlayer(player, welcomeMessage);
                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_WELCOME_HINT);
                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_HELP_HINT);
                    NAPI.Chat.SendChatMessageToPlayer(player, Messages.GEN_TICKET_HINT);
                    
                    if (account.lastCharacter > 0)
                    {
                        // Load selected character
                        PlayerModel character = Database.LoadCharacterInformationById(account.lastCharacter);
                        SkinModel skin = Database.GetCharacterSkin(account.lastCharacter);
                        
                        String pedModel = character.sex == 0 ? Constants.MALE_PED_MODEL : Constants.FEMALE_PED_MODEL;
                        PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                        NAPI.Player.SetPlayerName(player, character.realName);
                        NAPI.Player.SetPlayerSkin(player, pedHash);
                        
                        LoadCharacterData(player, character);
                        
                        PopulateCharacterSkin(player, skin);
                        
                        Globals.PopulateCharacterClothes(player);
                    }
                    else
                    {
                        PedHash pedHash = NAPI.Util.PedNameToModel(Constants.DEFAULT_PED_MODEL);
                        NAPI.Player.SetPlayerSkin(player, pedHash);
                    }

                    // Make the player invisible
                    NAPI.Entity.SetEntityTransparency(player, 255);

                    // Show login window and synchronize the time
                    TimeSpan currentTime = TimeSpan.FromTicks(DateTime.Now.Ticks);
                    NAPI.ClientEvent.TriggerClientEvent(player, "accountLoginForm", currentTime.Hours, currentTime.Minutes, currentTime.Seconds);
                    break;
            }
        }

        [RemoteEvent("loginAccount")]
        public void LoginAccountEvent(Client player, String password)
        {
            bool login = Database.LoginAccount(player.SocialClubName, password);
            NAPI.ClientEvent.TriggerClientEvent(player, login ? "clearLoginWindow" : "showLoginError");
        }

        [RemoteEvent("changeCharacterSex")]
        public void ChangeCharacterSexEvent(Client player, int sex)
        {
            String pedModel = sex == 1 ? Constants.FEMALE_PED_MODEL : Constants.MALE_PED_MODEL;
            PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
            NAPI.Player.SetPlayerSkin(player, pedHash);

            // Remove player's clothes
            NAPI.Player.SetPlayerClothes(player, 11, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 3, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 8, 15, 0);

            // Save sex entity shared data
            NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SEX, sex);
        }

        [RemoteEvent("createCharacter")]
        public void CreateCharacterEvent(Client player, String playerName, int playerAge, String skinJson)
        {
            PlayerModel playerModel = new PlayerModel();
            SkinModel skinModel = JsonConvert.DeserializeObject<SkinModel>(skinJson);
            
            playerModel.realName = playerName;
            playerModel.age = playerAge;
            playerModel.sex = NAPI.Data.GetEntitySharedData(player, EntityData.PLAYER_SEX);
            
            PopulateCharacterSkin(player, skinModel);

            int playerId = Database.CreateCharacter(player, playerModel, skinModel);

            if (playerId > 0)
            {
                InitializePlayerData(player);
                NAPI.Entity.SetEntityTransparency(player, 255);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_SQL_ID, playerId);
                NAPI.Data.SetEntityData(player, EntityData.PLAYER_NAME, playerName);
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_AGE, playerAge);
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SPAWN_POS, new Vector3(200.6641f, -932.0939f, 30.6868f));
                NAPI.Data.SetEntitySharedData(player, EntityData.PLAYER_SPAWN_ROT, new Vector3(0.0f, 0.0f, 0.0f));
                Database.UpdateLastCharacter(player.SocialClubName, playerId);
                NAPI.ClientEvent.TriggerClientEvent(player, "characterCreatedSuccessfully");
            }
        }

        [RemoteEvent("setCharacterIntoCreator")]
        public void SetCharacterIntoCreatorEvent(Client player)
        {
            // Change player's skin
            NAPI.Player.SetPlayerSkin(player, PedHash.FreemodeMale01);

            // Initialize player's face mode
            InitializePlayerData(player);

            // Remove clothes
            NAPI.Player.SetPlayerClothes(player, 11, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 3, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 8, 15, 0);

            // Set player's position
            NAPI.Entity.SetEntityTransparency(player, 255);
            NAPI.Entity.SetEntityRotation(player, new Vector3(0.0f, 0.0f, 180.0f));
            NAPI.Entity.SetEntityPosition(player, new Vector3(152.3787f, -1000.644f, -99f));
        }

        [RemoteEvent("loadCharacter")]
        public void LoadCharacterEvent(Client player, String name)
        {
            PlayerModel playerModel = Database.LoadCharacterInformationByName(name);
            SkinModel skinModel = Database.GetCharacterSkin(playerModel.id);

            // Load player's model
            String pedModel = playerModel.sex == 0 ? Constants.MALE_PED_MODEL : Constants.FEMALE_PED_MODEL;
            PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
            NAPI.Player.SetPlayerName(player, playerModel.realName);
            NAPI.Player.SetPlayerSkin(player, pedHash);
            
            LoadCharacterData(player, playerModel);
            PopulateCharacterSkin(player, skinModel);
            Globals.PopulateCharacterClothes(player);

            // Update last selected character
            Database.UpdateLastCharacter(player.SocialClubName, playerModel.id);
        }

        [RemoteEvent("getPlayerCustomSkin")]
        public void GetPlayerCustomSkinEvent(Client player, Client target)
        {
            int targetId = NAPI.Data.GetEntityData(target, EntityData.PLAYER_SQL_ID);
            List<TattooModel> targetTattooList = Globals.GetPlayerTattoos(targetId);

            // Update the model clientside
            NAPI.ClientEvent.TriggerClientEvent(player, "updatePlayerCustomSkin", target, NAPI.Util.ToJson(targetTattooList));
        }
    }
}