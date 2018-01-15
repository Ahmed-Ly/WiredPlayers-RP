let tattooZoneArray = [
	{zone: 0, desc: 'Torso'}, {zone: 1, desc: 'Cabeza'}, {zone: 2, desc: 'Brazo izquierdo'}, {zone: 3, desc: 'Brazo derecho'}, {zone: 4, desc: 'Pierna izquierda'}, {zone: 5, desc: 'Pierna derecha'}
];
let tattooZoneSelected = -1;
let tattooListArray = [];
let playerTattoosArray = [];
let priceMultiplier = 0;

// Menú de tatuajes
var tattooMenu = NAPI.CreateMenu("Tatuajes", "", 0, 0, 6);
NAPI.SetMenuBannerRectangle(tattooMenu, 255, 0, 0, 255);
tattooMenu.ResetKey(menuControl.Back);
tattooMenu.OnItemSelect.connect(function (sender, item, index) {
    if (tattooZoneSelected == -1) {
		// Miramos la opción seleccionada
		if (index == tattooMenu.Size - 1) {
			// Cerramos el menú
			tattooMenu.Visible = false;

			// Vestimos al personaje
			NAPI.TriggerServerEvent("loadCharacterClothes");
		} else {
			// Mostramos la lista de tatuajes
			tattooZoneSelected = index;
			NAPI.TriggerServerEvent("loadZoneTattoos", index);
		}
	} else {
		// Compramos el tatuaje
		NAPI.TriggerServerEvent("purchaseTattoo", tattooZoneSelected, index);
	}
});
tattooMenu.OnIndexChange.connect(function (sender, index) {
	if(tattooZoneSelected >= 0) {
		// Obtenemos el jugador
		let player = NAPI.GetLocalPlayer();

		// Cargamos los tatuajes que lleva
		loadPlayerTattoos();
		
		// Cambiamos el tatuaje del jugador
		let playerSex = NAPI.Data.GetEntitySharedData(player, "PLAYER_SEX");
		NAPI.CallNative("_SET_PED_DECORATION", player, NAPI.GetHashKey(tattooListArray[index].library), NAPI.GetHashKey(playerSex == 0 ? tattooListArray[index].maleHash : tattooListArray[index].femaleHash));
	}
});
tattooMenu.Visible = false;

NAPI.OnServerEventTrigger.connect(function (eventName, args) {
	// Miramos el evento
	switch(eventName) {
		case "showTattooMenu":
			// Obtenemos al jugador
			let player = NAPI.GetLocalPlayer();
			let playerSex = NAPI.Data.GetEntitySharedData(player, "PLAYER_SEX");

			// Quitamos la ropa al personaje
            NAPI.Player.SetPlayerClothes(player, 11, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 3, 15, 0);
            NAPI.Player.SetPlayerClothes(player, 8, 15, 0);

			// Ropa dependiente del sexo
			if(playerSex == 0) {
				NAPI.Player.SetPlayerClothes(player, 4, 61, 0);
				NAPI.Player.SetPlayerClothes(player, 6, 34, 0);
			} else {
				NAPI.Player.SetPlayerClothes(player, 4, 15, 0);
				NAPI.Player.SetPlayerClothes(player, 6, 35, 0);
			}

			// Cargamos las variables con los argumentos
			playerTattoosArray = JSON.parse(args[0]);
			priceMultiplier = args[1];

			// Limpiamos el menú previo
			tattooMenu.Clear();
			
			// Creación del menú
			populateMainTattooMenu();

			// Mostramos el menú
			tattooMenu.Visible = true;
			break;
		case "showZoneTattoos":
			// Recogemos la lista de tatuajes
			tattooListArray = JSON.parse(args[0]);

			// Limpiamos el menú previo
			tattooMenu.Clear();
			
			// Creación del menú
			populateZoneTattooMenu();
			break;
		case "addPurchasedTattoo":
			// Añadimos el nuevo tatuaje
			let tattoo = JSON.parse(args[0]);
			playerTattoosArray.push(tattoo);
			break;
	}
});

NAPI.OnKeyUp.connect(function (sender, e) {
    if (tattooZoneSelected >= 0 && e.KeyCode === Keys.Back) {
		// Vamos al menú anterior
		navigatePrevMenu();
    }
});

function populateMainTattooMenu() {
	// Generamos el menú principal
	for (let i = 0; i < tattooZoneArray.length; i++) {
        let menuItem = NAPI.CreateMenuItem(tattooZoneArray[i].desc, "");
		tattooMenu.AddItem(menuItem);
	}
    var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    tattooMenu.AddItem(cancelItem);
	tattooMenu.RefreshIndex();
}

function populateZoneTattooMenu() {
	// Generamos la lista de tatuajes para la zona seleccionada
	for (let i = 0; i < tattooListArray.length; i++) {
		let price = (tattooListArray[i].price * priceMultiplier) + "$";
        let menuItem = NAPI.CreateMenuItem(tattooListArray[i].name, price);
		tattooMenu.AddItem(menuItem);
	}
}

function navigatePrevMenu() {
	// Cerramos el menú de objetos
    tattooMenu.Clear();
	populateMainTattooMenu();
	tattooZoneSelected = -1;

	// Ponemos al jugador sus tatuajes
	loadPlayerTattoos();
}

function loadPlayerTattoos() {
	// Obtenemos los datos del jugador
	let player = NAPI.GetLocalPlayer();
	let playerSex = NAPI.Data.GetEntitySharedData(player, "PLAYER_SEX");

	// Limpiamos todos los tatuajes
	NAPI.CallNative("CLEAR_PED_DECORATIONS", player);

	// Cargamos todos los tatuajes
	for (let i = 0; i < playerTattoosArray.length; i++) {
		// Añadimos el tatuaje al jugador
		let tattoo = playerTattoosArray[i];
		NAPI.CallNative("_SET_PED_DECORATION", player, NAPI.GetHashKey(tattoo.library), NAPI.GetHashKey(tattoo.hash));
	}
}