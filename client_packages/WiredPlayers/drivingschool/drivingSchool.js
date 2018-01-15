let licenseBrowser = null;
let licenseBlip = null;
let questionsArray = [];
let answersArray = [];

mp.events.add('startLicenseExam', (questionsJson, answersJson) => {
	// Recogemos el tipo de examen
	questionsArray = JSON.parse(questionsJson);
	answersArray = JSON.parse(answersJson);

	// Mostramos la ventana del examen
	licenseBrowser = mp.browsers.new('package://WiredPlayers/statics/html/licenseExam.html');
	mp.gui.cursor.visible = true;
	mp.gui.chat.activate(false);
});

mp.events.add('getNextTestQuestion', () => {
	// Buscamos la posición de la pregunta a cargar
	let index = mp.players.local.getVariable('PLAYER_LICENSE_QUESTION');

	// Cargamos la pregunta e inicializamos las respuestas
	let questionText = questionsArray[index].text;
	let answers = [];

	// Insertamos todas las respuestas a la pregunta
	for(let i = 0; i < answersArray.length; i++) {
		// Comprobamos si corresponde a la pregunta
		if(answersArray[i].question == questionsArray[index].id) {
			answers.push(answersArray[i]);
		}
	}

	// Rellenamos los datos en el navegador
	let answersJson = JSON.stringify(answers);
    licenseBrowser.execute(`populateQuestionAnswers('${questionText}', '${answersJson}');`);
});

mp.events.add('submitAnswer', (answerId) => {
	// Comprobamos si la respuesta es correcta
	mp.events.callRemote('checkAnswer', answerId);
});

mp.events.add('finishLicenseExam', () => {
	// Borramos la pantalla de examen
	licenseBrowser.destroy();
	mp.gui.cursor.visible = false;
	mp.gui.chat.activate(true);
	licenseBrowser = null;
});

mp.events.add('showLicenseCheckpoint', (position) => {
	if(licenseBlip == null) {
		// Creamos una marca en el mapa
		licenseBlip = mp.blips.new(1, position, {color: 1});
	} else {
		// Cambiamos la posición de la marca
		licenseBlip.position = position;
	}
});

mp.events.add('deleteLicenseCheckpoint', () => {
	// Borramos la marca del mapa
	licenseBlip.destroy();
	licenseBlip = null;
});