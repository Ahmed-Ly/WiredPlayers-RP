let clothesTypesArray = [
    { type: 0, slot: 1, desc: 'Máscaras y pasamontañas' }, { type: 0, slot: 3, desc: 'Torso' }, { type: 0, slot: 4, desc: 'Pantalones' }, { type: 0, slot: 5, desc: 'Mochilas y bolsas' },
    { type: 0, slot: 6, desc: 'Calzado' }, { type: 0, slot: 7, desc: 'Complementos' }, { type: 0, slot: 8, desc: 'Camisetas interiores' }, { type: 0, slot: 11, desc: 'Chaquetas' },
    { type: 1, slot: 0, desc: 'Gorras y sombreros' }, { type: 1, slot: 1, desc: 'Gafas' }, { type: 1, slot: 2, desc: 'Pendientes' }
];

let clothes = [];

mp.events.add('showPlayerWardrobe', () => {	
	// Mostramos el menú del armario
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateWardrobeMenu', JSON.stringify(clothesTypeArray)]);
});

mp.events.add('getPlayerPurchasedClothes', (index) => {	
	// Obtenemos la lista de prendas
	mp.events.callRemote('getPlayerPurchasedClothes', clothesTypeArray[index].type, clothesTypeArray[index].slot);
});

mp.events.add('showPlayerClothes', (clothesJson, namesJson) => {
	let clothesNames = JSON.parse(namesJson);
	clothes = JSON.parse(clothesJson);
	
	for(let i = 0; i < clothes.length; i++) {
		// Añadimos el nombre de la prenda
		clothes[i].name = clothesNames[i];
	}
	
	// Mostramos la lista de prendas en el menú de armario
	mp.events.call('executeFunction', ['populateWardrobeClothes', JSON.stringify(clothes).replace(/'/g, "\\'")]);
});

mp.events.add('previewPlayerClothes', (index) => {
	let player = mp.players.local;
	
	if(clothes[index].type === 0) {
		// Cambiamos la prenda del jugador
		player.setComponentVariation(clothes[index].slot, clothes[index].drawable, clothes[index].texture, 0);
	} else {
		// Cambiamos el accesorio del jugador
		player.setPropIndex(clothes[index].slot, clothes[index].drawable, clothes[index].texture, 0);
	}
});

mp.events.add('changePlayerClothes', (index) => {	
	// Equipamos la prenda en el personaje
	mp.events.callRemote('wardrobeClothesItemSelected', clothes[index].id);
});