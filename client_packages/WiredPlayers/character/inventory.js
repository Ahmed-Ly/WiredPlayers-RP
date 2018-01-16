let inventoryBrowser = null;
let inventoryData = null;
let targetPlayer = null;

mp.events.add('showPlayerInventory', (inventoryJson, target) => {
	// Guardamos los datos del inventario
	inventoryData = inventoryJson;
	targetPlayer = target;
	
	// Mostramos la ventana con los objetos del inventario
	inventoryBrowser = mp.browsers.new('package://WiredPlayers/statics/html/inventory.html');
});

mp.events.add('getInventoryItems', () => {
	// Rellenamos la lista de objetos
	mp.gui.cursor.visible = true;
    inventoryBrowser.execute(`populateInventory('${inventoryData}', 'a');`);
});