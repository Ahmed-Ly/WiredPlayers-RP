const tattooZoneArray = ['Torso', 'Cabeza', 'Brazo izquierdo', 'Brazo derecho', 'Pierna izquierda', 'Pierna derecha'];

let playerTattooArray = [];
let tattooList = [];
let zoneTattoos = [];

mp.events.add('showTattooMenu', (playerTattoos, tattoosJson, business, price) => {
	// Guardamos las variables
	playerTattooArray = JSON.parse(playerTattoos);
	tattooList = JSON.parse(tattoosJson);
	
	// Mostramos el menú de tatuajes
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateTattooMenu', JSON.stringify(tattooZoneArray), business, price]);
});

mp.events.add('getZoneTattoos', (zone) => {
	// Obtenemos la lista de tatuajes de la zona
	zoneTattoos = [];
	
	for(let i = 0; i < tattooList.length; i++) {
		if(tattooList[i].slot === zone) {
			// Añadimos el tatuaje
			zoneTattoos.push(tattooList[i]);
		}
	}
	
	// Mostramos la lista de tatuajes en el menú
	let zoneTattoosJson = JSON.stringify(zoneTattoos);
	mp.events.call('executeFunction', ['populateZoneTattoos', zoneTattoosJson]);
});

mp.events.add('addPlayerTattoo', (index) => {
	// Obtenemos el jugador
	let player = mp.players.local;

	// Cargamos los tatuajes que lleva
	loadPlayerTattoos();
	
	// Cambiamos el tatuaje del jugador
	let playerSex = player.getVariable('PLAYER_SEX');
	player.setDecoration(mp.game.joaat(zoneTattoos[index].library), playerSex === 0 ? mp.game.joaat(zoneTattoos[index].maleHash) : mp.game.joaat(zoneTattoos[index].femaleHash));
});

mp.events.add('clearTattoos', () => {
	// Restablecemos los tatuajes del jugador
	loadPlayerTattoos();
});

mp.events.add('purchaseTattoo', (slot, index) => {
	// Obtenemos el sexo del jugador
	let playerSex = mp.players.local.getVariable('PLAYER_SEX');
	
	// Añadimos al array el nuevo tatuaje
	let tattoo = {};
	tattoo.slot = slot;
	tattoo.library = zoneTattoos[i].library;
	tattoo.library = playerSex === 0 ? zoneTattoos[i].maleHash : zoneTattoos[i].femaleHash;
	
	// Añadimos el tatuaje a la lista
	playerTattooArray.push(tattoo);
	
	// Mandamos la acción de compra de tatuaje
	mp.events.callRemote('purchaseTattoo', slot, index);
});

mp.events.add('exitTattooShop', () => {
	// Cerramos la ventana de compra
	mp.events.call('destroyBrowser');
	
	// Vestimos al personaje
	mp.events.callRemote('loadCharacterClothes');
});

function loadPlayerTattoos() {
	// Obtenemos los datos del jugador
	let player = mp.players.local;
	let playerSex = player.getVariable('PLAYER_SEX');

	// Limpiamos todos los tatuajes
	player.clearDecorations();

	// Cargamos todos los tatuajes
	for (let i = 0; i < playerTattooArray.length; i++) {
		// Añadimos el tatuaje al jugador
		let tattoo = playerTattooArray[i];
		player.setDecoration(mp.game.joaat(tattoo.library), mp.game.joaat(tattoo.hash));
	}
}