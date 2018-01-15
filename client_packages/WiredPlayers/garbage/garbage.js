let garbageBlip = null; 

mp.events.add('showGarbageCheckPoint', (position) => {
	// Creamos una marca con la posición del vehículo
	garbageBlip = mp.blips.new(1, position, {color: 1});
});

mp.events.add('deleteGarbageCheckPoint', () => {
	// Borramos la marca del mapa
	garbageBlip.destroy();
	garbageBlip = null;
});