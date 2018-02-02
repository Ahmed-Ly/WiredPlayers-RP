const townHallOptions = [
	{'desc': 'Documentación', 'price': 500}, {'desc': 'Seguro médico', 'price': 2000}, 
	{'desc': 'Licencia de taxista', 'price': 5000}, {'desc': 'Multas', 'price': 0}
];

mp.events.add('showTownHallMenu', () => {
	// Mostramos el menú del ayuntamiento
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateTownHallMenu', JSON.stringify(townHallOptions)]);
});

mp.events.add('executeTownHallOperation', (selectedOption) => {
	// Ejecutamos la opción seleccionada
	mp.events.callRemote('documentOptionSelected', selectedOption);
});

mp.events.add('showPlayerFineList', (playerFines) => {
	// Mostramos el menú de multas
	mp.events.call('executeFunction', ['populateFinesMenu', playerFines]);
});

mp.events.add('payPlayerFines', (finesArray) => {
	// Pagamos las multas seleccionadas
	mp.events.call('payPlayerFines', JSON.stringify(finesArray));
});

mp.events.add('backTownHallIndex', () => {
	// Mostramos el menú del ayuntamiento
	mp.events.call('executeFunction', ['populateTownHallMenu', JSON.stringify(townHallOptions)]);
});