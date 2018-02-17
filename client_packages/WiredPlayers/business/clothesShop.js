let clothesTypeArray = [
	{type: 0, slot: 1, desc: 'Máscaras y pasamontañas'}, {type: 0, slot: 3, desc: 'Torso'}, {type: 0, slot: 4, desc: 'Pantalones'}, {type: 0, slot: 5, desc: 'Mochilas y bolsas'}, 
	{type: 0, slot: 6, desc: 'Calzado'}, {type: 0, slot: 7, desc: 'Complementos'}, {type: 0, slot: 8, desc: 'Camisetas interiores'}, {type: 0, slot: 11, desc: 'Chaquetas'}, 
	{type: 1, slot: 0, desc: 'Gorras y sombreros'}, {type: 1, slot: 1, desc: 'Gafas'}, {type: 1, slot: 2, desc: 'Pendientes'}
];

let selectedIndex = -1;
let clothesTypes = [];

mp.events.add('showClothesBusinessPurchaseMenu', (business, price) => {	
	// Mostramos el menú de ropa
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateClothesShopMenu', JSON.stringify(clothesTypeArray), business, price]);
});

mp.events.add('getClothesByType', (index) => {
	// Guardamos el tipo seleccionado
	selectedIndex = index;
	
	// Obtenemos la lista de prendas
	mp.events.callRemote('getClothesByType', clothesTypeArray[index].type, clothesTypeArray[index].slot);
});

mp.events.add('showTypeClothes', (clothesJson) => {
	let player = mp.players.local;
	let type = clothesTypeArray[selectedIndex].type;
	let slot = clothesTypeArray[selectedIndex].slot;
	
	// Obtenemos la lista de prendas del tipo elegido
	clothesTypes = JSON.parse(clothesJson);
	
	for(let i = 0; i < clothesTypes.length; i++) {
		// Añadimos el número máximo de texturas a la prenda
		clothesTypes[i].textures = type == 0 ? player.getNumberOfTextureVariations(slot, clothesTypes[i].clothesId) : player.getNumberOfPropTextureVariations(slot, clothesTypes[i].clothesId);
	}
	
	// Mostramos la lista de prendas en el menú
	mp.events.call('executeFunction', ['populateTypeClothes', JSON.stringify(clothesTypes).replace(/'/g, "\\'")]);
});

mp.events.add('replacePlayerClothes', (index, texture) => {
	let player = mp.players.local;
	
	if(clothesTypes[index].type === 0) {
		// Cambiamos la prenda del jugador
		player.setComponentVariation(clothesTypes[index].bodyPart, clothesTypes[index].clothesId, texture, 0);
	} else {
		// Cambiamos el accesorio del jugador
		player.setPropIndex(clothesTypes[index].bodyPart, clothesTypes[index].clothesId, texture, 0);
	}
});

mp.events.add('clearClothes', () => {
	// TODO quitar ropa
});

