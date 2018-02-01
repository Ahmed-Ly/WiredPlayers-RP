const maleFaceOptions = [
	{'desc': 'Peinado', 'minValue': 0, 'maxValue': 36}, {'desc': 'Color principal del pelo', 'minValue': 0, 'maxValue': 63}, {'desc': 'Color secundario del pelo', 'minValue': 0, 'maxValue': 63}, 
	{'desc': 'Barba', 'minValue': -1, 'maxValue': 36}, {'desc': 'Color de la barba', 'minValue': 0, 'maxValue': 63}, 
	{'desc': 'Cejas', 'minValue': 0, 'maxValue': 33}, {'desc': 'Color de las cejas', 'minValue': 0, 'maxValue': 63}
];

const femaleFaceOptions = [
	{'desc': 'Peinado', 'minValue': 0, 'maxValue': 38}, {'desc': 'Color principal del pelo', 'minValue': 0, 'maxValue': 63}, {'desc': 'Color secundario del pelo', 'minValue': 0, 'maxValue': 63}, 
	{'desc': 'Cejas', 'minValue': 0, 'maxValue': 33}, {'desc': 'Color principal del pelo', 'minValue': 0, 'maxValue': 63}
];

let faceHairArray = [];

mp.events.add('showHairdresserMenu', (businessName) => {
	// Obtenemos el jugador y su sexo
	let player = mp.players.local;
	let sex = player.getVariable('PLAYER_SEX');
	
	// Añadimos las opciones
	let faceOptions = JSON.stringify(sex === 0 ? maleFaceOptions : femaleFaceOptions);
	
	// Inicializamos los valores
	faceHairArray.push(player.getVariable('HAIR_MODEL'));
	faceHairArray.push(player.getVariable('FIRST_HAIR_COLOR'));
	faceHairArray.push(player.getVariable('SECOND_HAIR_COLOR'));
	faceHairArray.push(player.getVariable('EYEBROWS_MODEL'));
	faceHairArray.push(player.getVariable('EYEBROWS_COLOR'));
	faceHairArray.push(player.getVariable('BEARD_MODEL'));
	faceHairArray.push(player.getVariable('BEARD_COLOR'));
	
	// Creamos el menú de peluquería
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateHairdresserMenu', faceOptions, JSON.stringify(faceHairArray), businessName]);
});

mp.events.add('updateFacialHair', (slot, value) => {
	// Obtenemos el jugador
	let player = mp.players.local;
	
	// Guardamos el nuevo valor
	faceHairArray[slot] = value;
	
	// Actualizamos el estado
	player.setComponentVariation(2, faceHairArray[0], 0, 0);
	player.setHairColor(faceHairArray[1], faceHairArray[2]);
	player.setHeadOverlay(1, faceHairArray[5], 0.99, faceHairArray[6], 0);
	player.setHeadOverlay(2, faceHairArray[3], 0.99, faceHairArray[4], 0);
});

mp.events.add('applyHairdresserChanges', () => {
	// Generamos el array con los datos
	let generatedFace = new Array();
	generatedFace['hairModel'] = faceHairArray[0];
	generatedFace['firstHairColor'] = faceHairArray[1];
	generatedFace['secondHairColor'] = faceHairArray[2];
	generatedFace['eyebrowsModel'] = faceHairArray[3];
	generatedFace['eyebrowsColor'] = faceHairArray[4];
	generatedFace['beardModel'] = faceHairArray[5];
	generatedFace['beardColor'] = faceHairArray[6];

	// Aplicamos el cambio de apariencia
	mp.events.callRemote('changeHairStyle', JSON.stringify(generatedFace));
});

mp.events.add('cancelHairdresserChanges', () => {
	// Obtenemos el jugador
	let player = mp.players.local;

	// Recogemos las antiguas variables
	let hairModel = player.getVariable('HAIR_MODEL');
	let firstHairColor = player.getVariable('FIRST_HAIR_COLOR');
	let secondHairColor = player.getVariable('SECOND_HAIR_COLOR');
	let eyebrowsModel = player.getVariable('EYEBROWS_MODEL');
	let eyebrowsColor = player.getVariable('EYEBROWS_COLOR');
	let beardModel = player.getVariable('BEARD_MODEL');
	let beardColor = player.getVariable('BEARD_COLOR');

	// Volvemos a poner todo como estaba
	player.setComponentVariation(2, hairModel, 0, 0);
	player.setHairColor(firstHairColor, secondHairColor);
	player.setHeadOverlay(1, beardModel, 0.99, beardColor, 0);
	player.setHeadOverlay(2, eyebrowsModel, 0.99, eyebrowsColor, 0);
});