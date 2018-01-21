let resolution = null;
let repaintBrowser = null;
let selected = 0;
let indexArray = [];
let slotsArray = [
	{slot: 0, desc: 'Alerón', products: 250}, {slot: 1, desc: 'Parachoques frontal', products: 250}, {slot: 2, desc: 'Parachoques trasero', products: 250}, {slot: 3, desc: 'Faldón lateral', products: 250}, 
	{slot: 4, desc: 'Tubo de escape', products: 100}, {slot: 5, desc: 'Antivuelco', products: 500}, {slot: 6, desc: 'Parrilla', products: 200}, {slot: 7, desc: 'Capó', products: 300}, {slot: 8, desc: 'Aleta', products: 100},
	{slot: 9, desc: 'Aleta trasera', products: 100}, {slot: 10, desc: 'Techo', products: 400}, {slot: 14, desc: 'Claxon', products: 100}, {slot: 15, desc: 'Suspensión', products: 900}, {slot: 22, desc: 'Xenon', products: 150}, 
	{slot: 23, desc: 'Ruedas delanteras', products: 100}, {slot: 24, desc: 'Ruedas traseras', products: 100}, {slot: 25, desc: 'Matrícula', products: 100}, {slot: 27, desc: 'Diseño tapicería', products: 800},
	{slot: 28, desc: 'Adornos', products: 150}, {slot: 33, desc: 'Volante', products: 100}, {slot: 34, desc: 'Palanca de cambios', products: 100}, {slot: 38, desc: 'Suspensión hidráulica', products: 1200}
];

mp.events.add('showTunningMenu', () => {
	// Obtenemos el vehículo en el que está subido
	let vehicle = mp.players.local.vehicle;
	
	// Inicializamos el array con los grupos
	let componentGroups = [];
	
	for(let i = 0; i < slotsArray.length; i++) {
		// Miramos el número de modificaciones
		let modNumber = vehicle.getNumMods(slotsArray[i].slot);
		
		// Si tiene modificaciones, añadimos la opción al menú
		if(modNumber > 0) {
			// Inicializamos el array de componentes y el grupo
			let group = {'slot': slotsArray[i].slot, 'desc': slotsArray[i].desc, 'products': slotsArray[i].products};
			let components = [];
			
			for(let m = 0; m < modNumber; m++) {
				let component = {'id': m, 'desc': slotsArray[i].desc + " tipo " + (m + 1)};
				components.push(component);
			}
			
			// Añadimos la lista de componentes a la lista
			group.components = components;
			componentGroups.push(group);
		}
	}
	
	// Creamos el menú de tunning
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateTunningMenu', JSON.stringify(componentGroups)]);
});

mp.events.add('addVehicleComponent', (slot, component) => {
	// Añadimos el componente al vehículo
	mp.players.local.vehicle.setMod(slot, component);
});

/*
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
}*/