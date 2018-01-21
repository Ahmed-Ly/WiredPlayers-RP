// Variables del personaje
let faceModel = {
	'faceFirstShape': 0, 'faceSecondShape': 0, 'skinFirstId': 0, 'skinSecondId': 0, 'faceMix': 0.5, 'skinMix': 0.5, 'hairModel': 0, 'firstHairColor': 0, 'secondHairColor': 0,
	'beardModel': 0, 'bearColor': 0, 'chestModel': 0, 'chestColor': 0, 'blemishesModel': -1, 'ageingModel': -1, 'complexionModel': -1, 'sundamageModel': -1, 'frecklesModel': -1,
	'eyesColor': 0, 'eyebrowsModel': 0, 'eyebrowsColor': 0, 'makeupModel': -1, 'blushModel': -1, 'blushColor': 0, 'lipstickModel': -1, 'lipstickColor': 0, 'noseWidth': 0.0,
	'noseHeight': 0.0, 'noseLength': 0.0, 'noseBridge': 0.0, 'noseTip': 0.0, 'noseShift': 0.0, 'browHeight': 0.0, 'browWidth': 0.0, 'cheekboneHeight': 0.0, 'cheekboneWidth': 0.0,
	'cheeksWidth': 0.0, 'eyes': 0.0, 'lips': 0.0, 'jawWidth': 0.0, 'jawHeight': 0.0, 'chinLength': 0.0, 'chinPosition': 0.0, 'chinWidth': 0.0, 'chinShape': 0.0, 'neckWidth': 0.0
};

// Variables genéricas
let browser = null;
let camera = null;

mp.events.add('showCharacterCreationMenu', () => {
	// Inicializamos las variables del personaje
	initializeCharacterCreation();
	
	// Ponemos la cámara enfocando al personaje
	camera = mp.cameras.new('default', new mp.Vector3(152.6008, -1003.25, -98), new mp.Vector3(-20.0, 0.0, 0.0), 2);
    camera.setActive(true);
	mp.game.cam.renderScriptCams(true, false, 0, true, false);
	
	// Cargamos el menú de creación
	browser = mp.browsers.new('package://WiredPlayers/statics/html/characterCreator.html');
	
	// Deshabilitamos la interfaz
	mp.gui.cursor.visible = true;
	mp.game.ui.displayHud(false);
	mp.gui.chat.activate(false);
	mp.gui.chat.show(false);
});

mp.events.add('updatePlayerSex', (sex) => {
	// Cambiamos el sexo del personaje
	initializeCharacterCreation();
	mp.events.callRemote('changeCharacterSex', sex);
});

mp.events.add('updatePlayerCreation', (partName, value, isPercentage) => {
	// Obtenemos el jugador
	let player = mp.players.local;
	
	if(percentage) {
		// Es un porcentaje, calculamos el valor
		value = parseFloat(value / 100);
	}
	
	// Actualizamos la apariencia del personaje
	faceModel[" + partName + "] = value;
	updatePlayerFace(player, faceModel);
});

mp.events.add('cameraPointTo', (bodyPart) => {
	if(bodyPart == 0) {
		// Enfocamos la cámara al cuerpo
		camera.setCoord(152.6008, -1003.25, -98);
	} else {
		// Enfocamos la cámara a la cara
		camera.setCoord(152.3708, -1001.75, -98.45);
	}
});

mp.events.add('rotateCharacter', (rotation) => {
	// Rotamos al personaje
	mp.players.local.setHeading(rotation);
});

mp.events.add('characterNameDuplicated', () => {
	// Avisamos del error
	browser.execute(`showPlayerDuplicatedWarn();`);
});

mp.events.add('acceptCharacterCreation', (name, age) => {
	// Llamamos a la función para crear el personaje
	let skinJson = JSON.stringify(faceModel);
	mp.events.callRemote('createCharacter', name, age, skinJson);
});

mp.events.add('cancelCharacterCreation', () => {
	// Ponemos la cámara por defecto
	mp.game.cam.renderScriptCams(false, false, 0, true, false);
	camera.destroy();
	camera = null;

	// Habilitamos la interfaz
	mp.gui.cursor.visible = false;
	mp.game.ui.displayHud(true);
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);

	// Eliminamos el menú de creación
	browser.destroy();
    browser = null;

	// Obtenemos la lista de personajes
	mp.events.callRemote('getPlayerCharacters');
});

mp.events.add('characterCreatedSuccessfully', () => {
	// Ponemos la cámara por defecto
	mp.game.cam.renderScriptCams(false, false, 0, true, false);
	camera.destroy();
	camera = null;

	// Habilitamos la interfaz
	mp.gui.cursor.visible = false;
	mp.game.ui.displayHud(true);
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);

	// Eliminamos el menú de creación
	browser.destroy();
    browser = null;
});

mp.events.add('entityStreamIn', (entity) => {
	// Comprobamos que sea una persona
	if(entity.getType() === 4) {
		// Miramos el modelo
		let model = entity.getModel();
        if (mp.game.joaat("mp_m_freemode_01") == model || mp.game.joaat("mp_f_freemode_01") == model) {
			// Obtenemos la cara y tatuajes del jugador
			mp.events.callRemote('getPlayerCustomSkin', entity);
			
			// Miramos si el jugador está borracho
			let walkingStyle = entity.getVariable('PLAYER_WALKING_STYLE');
			if(walkingStyle !== undefined) {
				// Añadimos el estilo de caminar
				entity.setMovementClipset(walkingStyle, 0.1);
			}
		}
	}
});

mp.events.add('updatePlayerCustomSkin', (player, skinJson, tattooJsonArray) => {
	// Obtenemos los objetos recibidos
	let skin = JSON.parse(skinJson);
	let tattooArray = JSON.parse(tattooJsonArray);
	
	// Actualizamos la apariencia del personaje
	updatePlayerFace(player, skin);
	updatePlayerTattoos(player, tattooArray);
});

function initializeCharacterCreation() {
	// Rasgos básicos
	faceModel.faceFirstShape = 0;
	faceModel.faceSecondShape = 0;
	faceModel.skinFirstId = 0;
	faceModel.skinSecondId = 0;
	faceModel.faceMix = 0.5;
	faceModel.skinMix = 0.5;
	faceModel.hairModel = 0;
	faceModel.firstHairColor = 0;
	faceModel.secondHairColor = 0;
	faceModel.beardModel = 0;
	faceModel.bearColor = 0;
	faceModel.chestModel = 0;
	faceModel.chestColor = 0;
	faceModel.blemishesModel = -1;
	faceModel.ageingModel = -1;
	faceModel.complexionModel = -1;
	faceModel.sundamageModel = -1;
	faceModel.frecklesModel = -1;
	faceModel.eyesColor = 0;
	faceModel.eyebrowsModel = 0;
	faceModel.eyebrowsColor = 0;
	faceModel.makeupModel = -1;
	faceModel.blushModel = -1;
	faceModel.blushColor = 0;
	faceModel.lipstickModel = -1;
	faceModel.lipstickColor = 0;
	faceModel.noseWidth = 0.0;
	faceModel.noseHeight = 0.0;
	faceModel.noseLength = 0.0;
	faceModel.noseBridge = 0.0;
	faceModel.noseTip = 0.0;
	faceModel.noseShift = 0.0;
	faceModel.browHeight = 0.0;
	faceModel.browWidth = 0.0;
	faceModel.cheekboneHeight = 0.0;
	faceModel.cheekboneWidth = 0.0;
	faceModel.cheeksWidth = 0.0;
	faceModel.eyes = 0.0;
	faceModel.lips = 0.0;
	faceModel.jawWidth = 0.0;
	faceModel.jawHeight = 0.0;
	faceModel.chinLength = 0.0;
	faceModel.chinPosition = 0.0;
	faceModel.chinWidth = 0.0;
	faceModel.chinShape = 0.0;
	faceModel.neckWidth = 0.0;
}

function updatePlayerFace(player, face) {
	// Actualizamos la apariencia del personaje
	player.setHeadBlendData(face.firstHeadShape, face.secondHeadShape, 0, face.firstSkinTone, face.secondSkinTone, 0, face.headMix, face.skinMix, 0, false);
	player.setComponentVariation(2, face.hairModel, 0, 0);
	player.setHairColor(face.firstHairColor, face.secondHairColor);
	player.setHeadOverlay(1, face.beardModel, 0.99);
	player.setHeadOverlayColor(1, 1, face.beardColor, 0);
	player.setHeadOverlay(10, face.chestModel, 0.99);
	player.setHeadOverlayColor(10, 1, face.chestColor, 0);
	player.setHeadOverlay(0, face.blemishesModel, 0.99);
	player.setHeadOverlay(3, face.ageingModel, 0.99);
	player.setHeadOverlay(6, face.complexionModel, 0.99);
	player.setHeadOverlay(7, face.sundamageModel, 0.99);
	player.setHeadOverlay(9, face.frecklesModel, 0.99);
	player.setEyeColor(face.eyesColor);
	player.setHeadOverlay(2, face.eyebrowsModel, 0.99);
	player.setHeadOverlayColor(2, 1, face.eyebrowsColor, 0);
	player.setHeadOverlay(4, face.makeupModel, 0.99);
	player.setHeadOverlay(5, face.blushModel, 0.99);
	player.setHeadOverlayColor(5, 2, face.blushColor, 0);
	player.setHeadOverlay(8, face.lipstickModel, 0.99);
	player.setHeadOverlayColor(8, 2, face.lipstickColor, 0);
	player.setFaceFeature(0, face.noseWidth);
	player.setFaceFeature(1, face.noseHeight);
	player.setFaceFeature(2, face.noseLength);
	player.setFaceFeature(3, face.noseBridge);
	player.setFaceFeature(4, face.noseTip);
	player.setFaceFeature(5, face.noseShift);
	player.setFaceFeature(6, face.browHeight);
	player.setFaceFeature(7, face.browWidth);
	player.setFaceFeature(8, face.cheekboneHeight);
	player.setFaceFeature(9, face.cheekboneWidth);
	player.setFaceFeature(10, face.cheeksWidth);
	player.setFaceFeature(11, face.eyes);
	player.setFaceFeature(12, face.lips);
	player.setFaceFeature(13, face.jawWidth);
	player.setFaceFeature(14, face.jawHeight);
	player.setFaceFeature(15, face.chinLength);
	player.setFaceFeature(16, face.chinPosition);
	player.setFaceFeature(17, face.chinWidth);
	player.setFaceFeature(18, face.chinShape);
	player.setFaceFeature(19, face.neckWidth);
}

function updatePlayerTattoos(player, tattooArray) {
	// Cargamos todos los tatuajes
	for (let i = 0; i < tattooArray.length; i++) {
		// Añadimos el tatuaje al jugador
		let library = mp.game.joaat(tattooArray[i].library);
		let hash = mp.game.joaat(tattooArray[i].hash);
		player.setDecoration(library, hash);
	}
}