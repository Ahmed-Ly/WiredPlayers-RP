let targetType = null;

mp.events.add('showPlayerInventory', (inventoryJson, target) => {
	// Guardamos los datos del inventario
	targetType = target;
	
	// Mostramos la ventana con los objetos del inventario
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/inventory.html', 'populateInventory', inventoryJson, 'a']);
});

mp.events.add('getInventoryOptions', (itemType, itemHash) => {
	// Inicializamos el array de opciones
	let optionsArray = [];
	let dropable = false;
	
	// Miramos el tipo de objeto y la entidad destino
	switch(targetType) {
		case 0:
			// Inventario del jugador
			if(itemType === 0) {
				// Es un consumible
				optionsArray.push("Consumir");
			} else if(itemType === 2) {
				// Es un contenedor
				optionsArray.push("Abrir");
			}
			
			if(isNaN(itemHash) === false) {
				// Es equipable
				optionsArray.push("Equipar");
			}
			
			// Desde el inventario se pueden tirar los objetos
			dropable = true;
			break;
		case 1:
			// Cacheo a un jugador
			optionsArray.push("Requisar");
			break;
		case 2:
			// Maletero de un vehículo
			optionsArray.push("Sacar");
			break;
		case 3:
			// Inventario al maletero
			optionsArray.push("Guardar");
			break;
	}
	
	// Mostramos las opciones en el navegador
	mp.events.call('executeFunction', [optionsArray, dropable]);
});