let vehicleLocationBlip = null; 

mp.events.add('locateVehicle', (position) => {
	// Creamos una marca con la posición del vehículo
	vehicleLocationBlip = mp.blips.new(1, position, {color: 1});
});

mp.events.add('deleteVehicleLocation', () => {
	// Borramos la marca del mapa
	vehicleLocationBlip.destroy();
	vehicleLocationBlip = null;
});