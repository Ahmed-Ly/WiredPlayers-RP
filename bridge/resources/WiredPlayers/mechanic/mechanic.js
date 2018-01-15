let resolution = null;
let repaintBrowser = null;
let selected = 0;
let indexArray = [];
let slotsArray = [
	{slot: 0, desc: 'Alerón'}, {slot: 1, desc: 'Parachoques frontal'}, {slot: 2, desc: 'Parachoques trasero'}, {slot: 3, desc: 'Faldón lateral'}, 
	{slot: 4, desc: 'Tubo de escape'}, {slot: 5, desc: 'Antivuelco'}, {slot: 6, desc: 'Parrilla'}, {slot: 7, desc: 'Capó'}, {slot: 8, desc: 'Aleta'},
	{slot: 9, desc: 'Aleta trasera'}, {slot: 10, desc: 'Techo'}, {slot: 14, desc: 'Claxon'}, {slot: 15, desc: 'Suspensión'}, {slot: 22, desc: 'Xenon'}, 
	{slot: 23, desc: 'Ruedas delanteras'}, {slot: 24, desc: 'Ruedas traseras'}, {slot: 25, desc: 'Matrícula'}, {slot: 27, desc: 'Diseño tapicería'},
	{slot: 28, desc: 'Adornos'}, {slot: 33, desc: 'Volante'}, {slot: 34, desc: 'Palanca de cambios'}, {slot: 38, desc: 'Suspensión hidráulica'}
];

// Menú de selección de tunning
var tunningMenu = NAPI.CreateMenu("Modificaciones", "", 0, 0, 6);
tunningMenu.ResetKey(menuControl.Back);
tunningMenu.OnItemSelect.connect(function (sender, item, index) {
	switch(index) {
		case tunningMenu.Size - 1:
			// Se ha pulsado la opción de cancelar
			NAPI.TriggerServerEvent("cancelVehicleModification");
			tunningMenu.Visible = false;
			break;
		case tunningMenu.Size - 2:
			// Se ha pulsado la opción de aceptar
			NAPI.TriggerServerEvent("confirmVehicleModification");
			break;
		case tunningMenu.Size - 3:
			// Se ha pulsado la opción de consultar precio
			NAPI.TriggerServerEvent("calculateTunningCost");
			break;
	}
});
tunningMenu.OnIndexChange.connect(function (sender, index) {
	selected = index;
});
tunningMenu.Visible = false;

NAPI.OnResourceStart.connect(function () {
    resolution = NAPI.GetScreenResolution();
});

NAPI.OnServerEventTrigger.connect(function (name, args) {
	// Miramos el evento que se ha llamado
	switch(name) {
		case "showRepaintMenu":
			// Cargamos la opción de repintar
            repaintBrowser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
            NAPI.SetCefBrowserPosition(repaintBrowser, 0, 0);
            NAPI.LoadPageCefBrowser(repaintBrowser, "statics/html/repaintVehicle.html");
            NAPI.WaitUntilCefBrowserLoaded(repaintBrowser);
			NAPI.SetHudVisible(false);
            NAPI.ShowCursor(true);
			break;
		case "closeRepaintWindow":
			cancelVehicleRepaint();
			break;
		case "showTunningMenu":
			// Cargamos la opción de tunning
			indexArray = [];
			tunningMenu.Clear();
			populateTunningMenu();
			tunningMenu.Visible = true;
			break;
	}
});

NAPI.OnResourceStop.connect(function () {
    if (repaintBrowser != null) {
        NAPI.DestroyCefBrowser(repaintBrowser);
        repaintBrowser = null;
    }

	NAPI.SetHudVisible(true);
});

function repaintVehicle(colorType, firstColor, secondColor, pearlescentColor, paid) {
	// Repintamos el vehículo
	NAPI.TriggerServerEvent("repaintVehicle", colorType, firstColor, secondColor, pearlescentColor, paid);
}

function cancelVehicleRepaint() {
	// Destruímos el navegador
    NAPI.DestroyCefBrowser(repaintBrowser);
    repaintBrowser = null;

	// Restablecemos la interfaz
	NAPI.SetHudVisible(true);
    NAPI.ShowCursor(false);

	// Volvemos a poner los colores
	NAPI.TriggerServerEvent("cancelVehicleRepaint");
}

function populateTunningMenu() {
	// Obtenemos el jugador y el vehículo en el que está subido
	let player = NAPI.GetLocalPlayer();
	let vehicle = NAPI.Player.GetPlayerVehicle(player);

	// Añadimos los componentes al menú
	for(let i = 0; i < slotsArray.length; i++) {
		// Miramos el número de modificaciones
		let modNumber = NAPI.ReturnNative("GET_NUM_VEHICLE_MODS", 0, vehicle, slotsArray[i].slot);
		
		// Si tiene modificaciones, añadimos la opción al menú
		if(modNumber > 1) {
			// Añadimos la posición al array de índices
			indexArray.push(i);

			// Creamos la lista de opciones
			let componentList = new List(String);
			componentList.Add("0");
			for(let m = 0; m < modNumber - 1; m++) {
				componentList.Add("" + (m+1));
			}
			let menuItem = NAPI.CreateListItem(slotsArray[i].desc, "", componentList, 0);
			tunningMenu.AddItem(menuItem);

			menuItem.OnListChanged.connect(function(sender, index) {
				let selectedIndex = indexArray[selected];
				NAPI.TriggerServerEvent("modifyVehicle", slotsArray[selectedIndex].slot, index);
			});
		}
	}

	// Añadimos las opciones de aceptar y cancelar
	let calculateItem = NAPI.CreateColoredItem("Calcular", "", "#E0E0E0", "#BDBDBD");
	let acceptItem = NAPI.CreateColoredItem("Modificar", "", "#558B2F", "#33691E");
    let cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    tunningMenu.AddItem(calculateItem);
    tunningMenu.AddItem(acceptItem);
    tunningMenu.AddItem(cancelItem);
}

function closeTunningMenu() {
	// Cerramos el menú de tunning
	tunningMenu.Visible = false;
}