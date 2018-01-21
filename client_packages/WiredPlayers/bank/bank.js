mp.events.add('showATM', () => {
	// Creamos la ventana del banco
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/bankMenu.html']);
	
	// Desactivamos el chat
	mp.gui.chat.activate(false);
	mp.gui.chat.show(false);
});

mp.events.add('getBankAccountMoney', () => {
	// Obtenemos el dinero en el banco del jugador
    return mp.players.local.getVariable('PLAYER_BANK');
});

mp.events.add('executeBankOperation', (operation, amount, target) => {
	// Ejecutamos una operación bancaria
	mp.events.callRemote('loadPlayerBankBalance', operation, amount, target);
});

mp.events.add('bankOperationResponse', (action) => {
	// Miramos la acción del cajero
	if (action == '') {
		mp.events.call('executeFunction', ['bankBack']);
	} else {
		mp.events.call('executeFunction', ['bankBack', action]);
	}
});

mp.events.add('loadPlayerBankBalance', () => {
	// Cargamos el balance bancario del jugador
	mp.events.callRemote('loadPlayerBankBalance');
});

mp.events.add('showPlayerBankBalance', (operationJson, playerName) => {
	// Mostramos las operaciones del jugador
	mp.events.call('executeFunction', ['showBankOperations', operationJson, playerName]);
});

mp.events.add('closeATM', () => {
	// Borramos el menú del cajero
	mp.events.call('destroyBrowser');
	
	// Reactivamos el chat
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);
});