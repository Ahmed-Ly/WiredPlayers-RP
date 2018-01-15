let browser = null;
let camera = null;
let resolution = null;

NAPI.OnResourceStart.connect(function () {
    resolution = NAPI.GetScreenResolution();
});

NAPI.OnServerEventTrigger.connect(function (name, args) {
	// Buscamos si se ha llamado a algún evento
	switch(name) {
		case "showCharacterCreationMenu":
			// Inicializamos las variables del personaje
			NAPI.Data.SetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_CREATING_CHARACTER", true);
			NAPI.Data.SetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_SEX", 0);
			initializeCharacterCreation();

			// Ponemos la cámara enfocando al personaje
			camera = NAPI.CreateCamera(new Vector3(152.6008, -1003.25, -98), new Vector3(0.0, 0.0, 0.0));
			NAPI.SetActiveCamera(camera);

			// Deshabilitamos la interfaz
			NAPI.SetChatVisible(false);
			NAPI.SetHudVisible(false);

			// Cargamos el menú de creación
			browser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
			NAPI.SetCefBrowserPosition(browser, 0, 0);
			NAPI.LoadPageCefBrowser(browser, "statics/html/characterCreator.html");
			NAPI.WaitUntilCefBrowserLoaded(browser);
			NAPI.SetCanOpenChat(false);
			NAPI.ShowCursor(true);
			break;
		case "updatePlayerFace":
			// Actualizamos la apariencia del personaje
			updatePlayerFace(args[0]);
			break;
		case "updatePlayerTattoos":
			// Actualizamos los tatuajes del jugador
			updatePlayerTattoos(JSON.parse(args[0]), args[1]);
			break;
		case "characterNameDuplicated":
			// Avisamos del error
			browser.call("showPlayerDuplicatedWarn");
			break;
		case "characterCreatedSuccessfully":
			// Limpiamos la variable de creación de personaje
			NAPI.Data.ResetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_CREATING_CHARACTER");
	
			// Ponemos la cámara por defecto
			NAPI.SetActiveCamera(null);

			// Habilitamos la interfaz
			NAPI.SetCanOpenChat(true);
			NAPI.SetChatVisible(true);
			NAPI.SetHudVisible(true);
			NAPI.ShowCursor(false);

			// Eliminamos el menú de creación
			NAPI.DestroyCefBrowser(browser);
			browser = null;
			break;
		case "changePlayerWalkingStyle":
			NAPI.SetPlayerMovementClipset(args[0], args[1], 0.1);
			break;
		case "resetPlayerWalkingStyle":
			NAPI.ResetPlayerMovementClipset(args[0]);
			break;
	}
});

NAPI.OnEntityStreamIn.connect(function (ent, entType) {
    if (entType === 6 || entType === 8) {
        var model = NAPI.GetEntityModel(ent);
        if (model == 1885233650 || model == -1667301416) {
			// Actualizamos la cara del personaje
            updatePlayerFace(ent);

			// Obtenemos los tatuajes del jugador
			NAPI.TriggerServerEvent("getPlayerTattoos", ent);

			// Miramos si el jugador está borracho
			if(NAPI.Data.HasEntitySharedData(ent, "PLAYER_WALKING_STYLE") == true) {
				let walkingStyle = NAPI.Data.GetEntitySharedData(ent, "PLAYER_WALKING_STYLE");
				NAPI.SetPlayerMovementClipset(ent, walkingStyle, 0.1);
			}
        }
    }
});

NAPI.OnResourceStop.connect(function () {
    if (browser != null) {
        NAPI.DestroyCefBrowser(browser);
        browser = null;
    }
});

function initializeCharacterCreation() {
	// Obtenemos el personaje
	let player = NAPI.GetLocalPlayer();

	// Datos básicos
	NAPI.Data.SetEntitySharedData(player, "PLAYER_AGE", 18);

	// Rasgos básicos
	NAPI.Data.SetEntitySharedData(player, "GTAO_SHAPE_FIRST_ID", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_SHAPE_SECOND_ID", 21);
	NAPI.Data.SetEntitySharedData(player, "GTAO_SKIN_FIRST_ID", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_SKIN_SECOND_ID", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_FACE_MIX", 0.5);
	NAPI.Data.SetEntitySharedData(player, "GTAO_SKIN_MIX", 0.5);

	// Pelo y vello facial
	NAPI.Data.SetEntitySharedData(player, "GTAO_HAIR_MODEL", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_HAIR_FIRST_COLOR", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_HAIR_SECOND_COLOR", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_BEARD_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_BEARD_COLOR", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_CHEST_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_CHEST_COLOR", 0);

	// Generación de marcas de piel
	NAPI.Data.SetEntitySharedData(player, "GTAO_BLEMISHES_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_AGEING_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_COMPLEXION_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_SUNDAMAGE_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_FRECKLES_MODEL", -1);
	
	// Generación de rasgos faciales avanzados
	NAPI.Data.SetEntitySharedData(player, "GTAO_A_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_B_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_C_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_D_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_E_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_F_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_G_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_H_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_I_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_J_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_K_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_L_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_M_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_N_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_O_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_P_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_Q_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_R_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_S_MODEL", -1);
	NAPI.Data.SetEntitySharedData(player, "GTAO_T_MODEL", -1);

	// Generación de ojos y cejas
	NAPI.Data.SetEntitySharedData(player, "GTAO_EYES_COLOR", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_EYEBROWS_MODEL", 0);
	NAPI.Data.SetEntitySharedData(player, "GTAO_EYEBROWS_COLOR", 0);

	// Maquillaje y pintalabios
	var makeupModel = NAPI.Data.SetEntitySharedData(player, "GTAO_MAKEUP_MODEL", -1);
	var blushModel = NAPI.Data.SetEntitySharedData(player, "GTAO_BLUSH_MODEL", -1);
	var blushColor = NAPI.Data.SetEntitySharedData(player, "GTAO_BLUSH_COLOR", 0);
	var lipstickModel = NAPI.Data.SetEntitySharedData(player, "GTAO_LIPSTICK_MODEL", -1);
	var lipstickColor = NAPI.Data.SetEntitySharedData(player, "GTAO_LIPSTICK_COLOR", 0);
}

function updatePlayerFace(player) {
    // Generación de la cara
    var shapeFirstId = NAPI.Data.GetEntitySharedData(player, "GTAO_SHAPE_FIRST_ID");
    var shapeSecondId = NAPI.Data.GetEntitySharedData(player, "GTAO_SHAPE_SECOND_ID");
    var skinFirstId = NAPI.Data.GetEntitySharedData(player, "GTAO_SKIN_FIRST_ID");
    var skinSecondId = NAPI.Data.GetEntitySharedData(player, "GTAO_SKIN_SECOND_ID");
    var shapeMix = NAPI.Data.GetEntitySharedData(player, "GTAO_FACE_MIX");
    var skinMix = NAPI.Data.GetEntitySharedData(player, "GTAO_SKIN_MIX");

    // Generación del peinado
    var hairModel = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_MODEL");
	var hairFirstColor = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_FIRST_COLOR");
	var hairSecondColor = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_SECOND_COLOR");

	// Generación de la barba
	var beardModel = NAPI.Data.GetEntitySharedData(player, "GTAO_BEARD_MODEL");
	var beardColor = NAPI.Data.GetEntitySharedData(player, "GTAO_BEARD_COLOR");

	// Generación del vello del pecho
	var chestModel = NAPI.Data.GetEntitySharedData(player, "GTAO_CHEST_MODEL");
	var chestColor = NAPI.Data.GetEntitySharedData(player, "GTAO_CHEST_COLOR");

	// Generación de marcas de piel
	var blemishesModel = NAPI.Data.GetEntitySharedData(player, "GTAO_BLEMISHES_MODEL");
	var ageingModel = NAPI.Data.GetEntitySharedData(player, "GTAO_AGEING_MODEL");
	var complexionModel = NAPI.Data.GetEntitySharedData(player, "GTAO_COMPLEXION_MODEL");
	var sundamageModel = NAPI.Data.GetEntitySharedData(player, "GTAO_SUNDAMAGE_MODEL");
	var frecklesModel = NAPI.Data.GetEntitySharedData(player, "GTAO_FRECKLES_MODEL");
	
	// Generación de rasgos faciales avanzados
	var a = NAPI.Data.GetEntitySharedData(player, "GTAO_A_MODEL");
	var b = NAPI.Data.GetEntitySharedData(player, "GTAO_B_MODEL");
	var c = NAPI.Data.GetEntitySharedData(player, "GTAO_C_MODEL");
	var d = NAPI.Data.GetEntitySharedData(player, "GTAO_D_MODEL");
	var e = NAPI.Data.GetEntitySharedData(player, "GTAO_E_MODEL");
	var f = NAPI.Data.GetEntitySharedData(player, "GTAO_F_MODEL");
	var g = NAPI.Data.GetEntitySharedData(player, "GTAO_G_MODEL");
	var h = NAPI.Data.GetEntitySharedData(player, "GTAO_H_MODEL");
	var i = NAPI.Data.GetEntitySharedData(player, "GTAO_I_MODEL");
	var j = NAPI.Data.GetEntitySharedData(player, "GTAO_J_MODEL");
	var k = NAPI.Data.GetEntitySharedData(player, "GTAO_K_MODEL");
	var l = NAPI.Data.GetEntitySharedData(player, "GTAO_L_MODEL");
	var m = NAPI.Data.GetEntitySharedData(player, "GTAO_M_MODEL");
	var n = NAPI.Data.GetEntitySharedData(player, "GTAO_N_MODEL");
	var o = NAPI.Data.GetEntitySharedData(player, "GTAO_O_MODEL");
	var p = NAPI.Data.GetEntitySharedData(player, "GTAO_P_MODEL");
	var q = NAPI.Data.GetEntitySharedData(player, "GTAO_Q_MODEL");
	var r = NAPI.Data.GetEntitySharedData(player, "GTAO_R_MODEL");
	var s = NAPI.Data.GetEntitySharedData(player, "GTAO_S_MODEL");
	var t = NAPI.Data.GetEntitySharedData(player, "GTAO_T_MODEL");

	// Generación de ojos y cejas
	var eyesColor = NAPI.Data.GetEntitySharedData(player, "GTAO_EYES_COLOR");
	var eyebrowsModel = NAPI.Data.GetEntitySharedData(player, "GTAO_EYEBROWS_MODEL");
	var eyebrowsColor = NAPI.Data.GetEntitySharedData(player, "GTAO_EYEBROWS_COLOR");

	// Generación del maquillaje
	var makeupModel = NAPI.Data.GetEntitySharedData(player, "GTAO_MAKEUP_MODEL");
	var blushModel = NAPI.Data.GetEntitySharedData(player, "GTAO_BLUSH_MODEL");
	var blushColor = NAPI.Data.GetEntitySharedData(player, "GTAO_BLUSH_COLOR");
	var lipstickModel = NAPI.Data.GetEntitySharedData(player, "GTAO_LIPSTICK_MODEL");
	var lipstickColor = NAPI.Data.GetEntitySharedData(player, "GTAO_LIPSTICK_COLOR");

    // Funciones nativas para actualizar la apariencia del personaje
    NAPI.CallNative("SET_PED_HEAD_BLEND_DATA", player, shapeFirstId, shapeSecondId, 0, skinFirstId, skinSecondId, 0, shapeMix, skinMix, 0, false);
    NAPI.CallNative("SET_PED_COMPONENT_VARIATION", player, 2, hairModel, 0, 0);
	NAPI.CallNative("_SET_PED_HAIR_COLOR", player, hairFirstColor, hairSecondColor);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 1, beardModel, 0.99);
	NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 1, 1, beardColor, 0);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 10, chestModel, 0.99);
	NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 10, 1, chestColor, 0);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 0, blemishesModel, 0.99);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 3, ageingModel, 0.99);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 6, complexionModel, 0.99);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 7, sundamageModel, 0.99);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 9, frecklesModel, 0.99);
	NAPI.CallNative("_SET_PED_EYE_COLOR", player, eyesColor);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 2, eyebrowsModel, 0.99);
	NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 2, 1, eyebrowsColor, 0);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 4, makeupModel, 0.99);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 5, blushModel, 0.99);
	NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 5, 2, blushColor, 0);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 8, lipstickModel, 0.99);
	NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 8, 2, lipstickColor, 0);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 0, a);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 1, b);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 2, c);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 3, d);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 4, e);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 5, f);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 6, g);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 7, h);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 8, i);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 9, j);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 10, k);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 11, l);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 12, m);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 13, n);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 14, o);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 15, p);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 16, q);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 17, r);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 18, s);
	NAPI.CallNative("_SET_PED_FACE_FEATURE", player, 19, t);
}

function updatePlayerTattoos(tattooArray, player) {
	// Cargamos todos los tatuajes
	for (let i = 0; i < tattooArray.length; i++) {
		// Añadimos el tatuaje al jugador
		let tattoo = tattooArray[i];
		NAPI.CallNative("_SET_PED_DECORATION", player, NAPI.GetHashKey(tattoo.library), NAPI.GetHashKey(tattoo.hash));
	}
}

function acceptCharacterCreation(name, age) {
	// Llamamos a la función para crear el personaje
    NAPI.TriggerServerEvent("createCharacter", name, age);
}

function cancelCharacterCreation() {
	// Ponemos la cámara por defecto
	NAPI.SetActiveCamera(null);

	// Habilitamos la interfaz
	NAPI.SetCanOpenChat(true);
	NAPI.SetChatVisible(true);
    NAPI.SetHudVisible(true);
	NAPI.ShowCursor(false);

	// Eliminamos el menú de creación
    NAPI.DestroyCefBrowser(browser);
    browser = null;

	// Obtenemos la lista de personajes
	NAPI.TriggerServerEvent("getPlayerCharacters");
	NAPI.Data.ResetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_CREATING_CHARACTER");
}

function updatePlayerSex(sex) {
	let player = NAPI.GetLocalPlayer();
	NAPI.Data.SetEntitySharedData(player, "PLAYER_SEX", sex);
	NAPI.TriggerServerEvent("changeCharacterSex", sex);
	initializeCharacterCreation();
	updatePlayerFace(player);
}

function updatePlayerCreation(entityData, value, percentage) {
	let player = NAPI.GetLocalPlayer();
	if(percentage) {
		value = parseFloat(value / 100);
	}
	NAPI.Data.SetEntitySharedData(player, entityData, value);
	updatePlayerFace(player);
}

function cameraPointTo(part) {
	if(part == 0) {
		// Enfocamos la cámara al cuerpo
        camera = NAPI.CreateCamera(new Vector3(152.6008, -1003.25, -98), new Vector3(0.0, 0.0, 0.0));
	} else {
		// Enfocamos la cámara a la cara
        camera = NAPI.CreateCamera(new Vector3(152.3708, -1001.75, -98.45), new Vector3(0.0, 0.0, 0.0));
	}
	
	// Ponemos la cámara activa
	NAPI.SetActiveCamera(camera);
}

function rotateCharacter(rotation) {
    NAPI.Entity.SetEntityRotation(NAPI.GetLocalPlayer(), new Vector3(0.0, 0.0, rotation));
}