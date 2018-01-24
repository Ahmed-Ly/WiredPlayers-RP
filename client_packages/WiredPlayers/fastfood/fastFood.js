let fastFoodBlip = null;

mp.events.add('showFastfoodOrders', (orders, distances) => {
	// Creamos el menú de comida rápida
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateFastfoodOrders', orders, distances]);
});

mp.events.add('deliverFastfoodOrder', (order) => {
	// Cerramos el menú y atendemos el pedido
	mp.events.callRemote('takeFastFoodOrder', order);
	mp.events.call('destroyBrowser');
});

mp.events.add('fastFoodDestinationCheckPoint', (position) => {
	// Creamos una marca con la posición del vehículo
	fastFoodBlip = mp.blips.new(1, position, {color: 1});
});

mp.events.add('fastFoodDeliverBack', (position) => {
	// Creamos una marca con la posición del vehículo
	fastFoodBlip.setCoords(position);
});

mp.events.add('fastFoodDeliverFinished', () => {
	// Borramos la marca del mapa
	fastFoodBlip.destroy();
	fastFoodBlip = null;
});