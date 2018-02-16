let licenseBlip = null;
let questionsArray = [];
let answersArray = [];

mp.events.add('startLicenseExam', (questionsJson, answersJson) => {
	// Recogemos el tipo de examen
	questionsArray = JSON.parse(questionsJson);
	answersArray = JSON.parse(answersJson);

	// Desactivamos el chat
	mp.gui.chat.activate(false);
	mp.gui.chat.show(false);
	
	// Mostramos la ventana del examen
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/licenseExam.html']);
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
	mp.events.call('executeFunction', ['populateQuestionAnswers', questionText, answersJson]);
});

mp.events.add('submitAnswer', (answerId) => {
	// Comprobamos si la respuesta es correcta
	mp.events.callRemote('checkAnswer', answerId);
});

mp.events.add('finishLicenseExam', () => {
	// Borramos la pantalla de examen
	mp.events.call('destroyBrowser');
	
	// Reactivamos el chat
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);
});

mp.events.add('showLicenseCheckpoint', (position) => {
	if(licenseBlip == null) {
		// Creamos una marca en el mapa
		licenseBlip = mp.blips.new(1, position, {color: 1});
	} else {
		// Cambiamos la posición de la marca
		licenseBlip.setCoords(position);
	}
});

mp.events.add('deleteLicenseCheckpoint', () => {
	// Borramos la marca del mapa
	licenseBlip.destroy();
	licenseBlip = null;
});