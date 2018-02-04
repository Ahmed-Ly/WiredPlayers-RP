let crimesJson = null;
let crimesArray = null;
let selectedControl = null;
let reinforces = [];

mp.events.add('showCrimesMenu', (crimes) => {
	// Guardamos la lista de delitos
	selectedControl = null;
	crimesJson = crimes;
	
	// Creamos el menú con la lista de delitos
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateCrimesMenu', crimes, '']);
});

mp.events.add('applyCrimes', (crimes) => {
	// Guardamos los delitos a aplicar
	crimesArray = crimes;
	
	// Eliminamos el menú de delitos
	mp.events.call('destroyBrowser');
	
	// Mostramos la ventana de confirmación
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/crimesConfirm.html', 'populateCrimesConfirmMenu', crimesArray]);
});

mp.events.add('executePlayerCrimes', () => {
	// Eliminamos el menú de confirmación
	mp.events.call('destroyBrowser');
	
	// Aplicamos la condena al jugador
	mp.events.callRemote('applyCrimesToPlayer', crimesArray);
});

mp.events.add('backCrimesMenu', () => {
	// Eliminamos el menú de confirmación
	mp.events.call('destroyBrowser');
	
	// Mostramos la ventana de confirmación
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateCrimesMenu', crimesJson, crimesArray]);
});

mp.events.add('loadPoliceControlList', (policeControls) => {
	// Creamos el menú con la lista de controles policiales
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populatePoliceControlMenu', policeControls]);
});

mp.events.add('proccessPoliceControlAction', (control) => {
	// Miramos qué opción había seleccionado
	let controlOption = mp.players.local.getVariable('PLAYER_POLICE_CONTROL');
	
	switch(controlOption) {
		case 1:
			if(control === undefined) {
				// Guardamos el control con un nombre nuevo
				mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/policeControlName.html']);				
			} else {
				// Sobrescribimos el control existente
				mp.events.callRemote('policeControlSelected', control);
			}
			break;
		case 2:
			// Mostramos la ventana para cambiar el nombre
			mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/policeControlName.html']);	
			selectedControl = control;
			break;
		default:
			// Ejecutamos la opción sobre el control seleccionado
			mp.events.callRemote('policeControlSelected', control);
			break;
	}
});

mp.events.add('policeControlSelectedName', (name) => {
	// Ponemos nombre al control creado
	mp.events.callRemote('updatePoliceControlName', selectedControl, name);
});

mp.events.add('updatePoliceReinforces', (reinforcesJson) => {
	let updatedReinforces = JSON.parse(reinforcesJson);

	// Miramos quienes tienen refuerzos activos
	for(let i = 0; i < updatedReinforces.length; i++) {
		// Obtenemos el identificador
		let police = updatedReinforces[i].playerId;
		let position = new mp.Vector3(updatedReinforces[i].position.X, updatedReinforces[i].position.Y, updatedReinforces[i].position.Z);

		if(reinforces[police] === undefined) {
			// Creamos la nueva marca en el mapa
			let reinforcesBlip = mp.blips.new(487, position, {color: 38, alpha: 255, shortRange: false});

			// El miembro no estaba en la lista, lo añadimos
			reinforces[police] = reinforcesBlip;
		} else {
			// Actualizamos la posición de la marca
			reinforces[police].position = position;
		}
	}
});

mp.events.add('reinforcesRemove', (officer) => {
	// Eliminamos el refuerzo de la persona
	reinforces[officer].destroy();
	reinforces[officer] = undefined;
});