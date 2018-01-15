let carShopBrowser = null;
let carShopVehicles = null;
let carShopTestBlip = null;
let previewBrowser = null;
let previewVehicle = null;
let previewCamera = null;
let dealership = null;

mp.events.add('showVehicleCatalog', (vehicles, dealer) => {
	// Recogemos la lista de vehículos y concesionario
	carShopVehicles = vehicles;
	dealership = dealer;

	// Mostramos la ventana del catálogo
	carShopBrowser = mp.browsers.new('package://WiredPlayers/statics/html/vehicleCatalog.html');
	mp.gui.chat.activate(false);
	mp.gui.chat.show(false);
});

mp.events.add('getCarShopVehicleList', () => {
	// Rellenamos la lista de vehículos
	mp.gui.cursor.visible = true;
    carShopBrowser.execute(`populateVehicleList('${dealership}', '${carShopVehicles}');`);
});

mp.events.add('previewCarShopVehicle', (model) => {
	if (previewVehicle != null) {
		previewVehicle.destroy();
    }
	
	// Colocamos el vehículo y la cámara en función del concesionario
	switch(dealership) {
		case 2:
			previewVehicle = mp.vehicles.new(mp.game.joaat(model), new mp.Vector3(-878.5726, -1353.408, 0.1741), {heading: 90.0});
			previewCamera = mp.cameras.new('default', new mp.Vector3(-882.3361, -1342.628, 5.0783), new mp.Vector3(-20.0, 0.0, 200.0), 90);
			break;
		default:
			previewVehicle = mp.vehicles.new(mp.game.joaat(model), new mp.Vector3(-31.98111, -1090.434, 26.42225), {heading: 180.0});
			previewCamera = mp.cameras.new('default', new mp.Vector3(-37.83527, -1088.096, 27.92234), new mp.Vector3(-20.0, 0.0, 250.0), 90);
			break;
	}

    // Nueva cámara dirigida al vehículo
    previewCamera.setActive(true);
	mp.game.cam.renderScriptCams(true, false, 0, true, false);

    // Eliminamos el catálogo
    carShopBrowser.destroy();
    carShopBrowser = null;

    // Deshabilitamos el HUD
	mp.game.ui.displayHud(false);

	// Creación del menú de previsualización
	previewBrowser = mp.browsers.new('package://WiredPlayers/statics/html/vehiclePreview.html');
});

mp.events.add('rotatePreviewVehicle', (rotation) => {
	// Aplicamos la rotación al vehículo
    previewVehicle.rotation = new mp.Vector3(0.0, 0.0, rotation);
});

mp.events.add('previewVehicleChangeColor', (color, colorMain) => {
    if (colorMain) {
		previewVehicle.setCustomPrimaryColour(hexToRgb(color).r, hexToRgb(color).g, hexToRgb(color).b);
    } else {
		previewVehicle.setCustomSecondaryColour(hexToRgb(color).r, hexToRgb(color).g, hexToRgb(color).b);
    }
});

mp.events.add('showCatalog', () => {
    // Eliminamos el menú de previsualización
    previewBrowser.destroy();
    previewBrowser = null;

    // Eliminamos el vehículo creado
    previewVehicle.destroy();
    previewVehicle = null;

    // Habilitamos el HUD
	mp.game.ui.displayHud(true);

    // Devolvemos la cámara al personaje
	mp.game.cam.renderScriptCams(false, false, 0, true, false);
	previewCamera.destroy();
	previewCamera = null;
	
	// Mostramos la ventana del catálogo
	carShopBrowser = mp.browsers.new('package://WiredPlayers/statics/html/vehicleCatalog.html');
});

mp.events.add('closeCatalog', () => {
    carShopBrowser.destroy();
	mp.gui.cursor.visible = false;
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);
    carShopBrowser = null;
});

mp.events.add('checkVehiclePayable', () => {
	// Obtenemos la lista de vehículos
    let vehicleArray = JSON.parse(carShopVehicles);
	
    for (var i = 0; i < vehicleArray.length; i++) {
        if (mp.game.joaat(vehicleArray[i].model) == previewVehicle.model) {
			// Miramos si tiene dinero suficiente en el banco
			if(mp.players.local.getVariable('PLAYER_BANK') >= vehicleArray[i].price) {
				// Activamos el botón de compra
				previewBrowser.execute(`showVehiclePurchaseButton();`);
			}
			break;
        }
    }
});

mp.events.add('purchaseVehicle', () => {
    // Obtenemos los datos del vehículo
	let model = String(previewVehicle.model);
	let firstColorObject = previewVehicle.getCustomPrimaryColour(0, 0, 0);
	let secondColorObject = previewVehicle.getCustomSecondaryColour(0, 0, 0);
	
	// Obtenemos el string de colores
	let firstColor = firstColorObject.r + ',' + firstColorObject.g + ',' + firstColorObject.b;
	let secondColor = secondColorObject.r + ',' + secondColorObject.g + ',' + secondColorObject.b;

    // Eliminamos el menú de previsualización
    previewBrowser.destroy();
    previewBrowser = null;

    // Eliminamos el vehículo creado
    previewVehicle.destroy();
    previewVehicle = null;

    // Habilitamos el HUD
	mp.game.ui.displayHud(true);
	
	// Eliminamos la cámara
	mp.game.cam.renderScriptCams(false, false, 0, true, false);
	previewCamera.destroy();
	previewCamera = null;

    // Devolvemos el control al personaje
	mp.gui.cursor.visible = false;
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);

	// Compramos el vehículo
	mp.events.callRemote('purchaseVehicle', model, firstColor, secondColor);
});

mp.events.add('testVehicle', () => {
	// Obtenemos el modelo del vehículo
	let model = String(previewVehicle.model);
	
    // Eliminamos el menú de previsualización
    previewBrowser.destroy();
    previewBrowser = null;

    // Eliminamos el vehículo creado
    previewVehicle.destroy();
    previewVehicle = null;

    // Habilitamos el HUD
	mp.game.ui.displayHud(true);
	
	// Eliminamos la cámara
	mp.game.cam.renderScriptCams(false, false, 0, true, false);
	previewCamera.destroy();
	previewCamera = null;

    // Devolvemos el control al personaje
	mp.gui.cursor.visible = false;
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);

	// Probamos el vehículo
	mp.events.callRemote('testVehicle', model);
});

mp.events.add('showCarshopCheckpoint', (position) => {
	// Creamos una marca con la posición de entrega
	carShopTestBlip = mp.blips.new(1, position, {color: 1});
});

mp.events.add('deleteCarshopCheckpoint', () => {
	// Borramos la marca del mapa
	carShopTestBlip.destroy();
	carShopTestBlip = null;
});

function hexToRgb(hex) {
    var shorthandRegex = /^#?([a-f\d])([a-f\d])([a-f\d])$/i;
    hex = hex.replace(shorthandRegex, function(m, r, g, b) {
        return r + r + g + g + b + b;
    });

    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}