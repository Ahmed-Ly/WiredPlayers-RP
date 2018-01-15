let bankBrowser = null;

mp.events.add('showATM', () => {
	// Creamos la ventana del banco
	bankBrowser = mp.browsers.new('package://WiredPlayers/statics/html/bankMenu.html');
	mp.gui.cursor.visible = true;
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
		bankBrowser.execute(`bankBack();`);
	} else {
		bankBrowser.execute(`showOperationError('${action}');`);
	}
});

mp.events.add('loadPlayerBankBalance', () => {
	// Cargamos el balance bancario del jugador
	mp.events.callRemote('loadPlayerBankBalance');
});

mp.events.add('showPlayerBankBalance', (operationJson, playerName) => {
	// Mostramos las operaciones del jugador
	bankBrowser.execute(`showBankOperations('${operationJson}', '${playerName}');`);
});

mp.events.add('closeATM', () => {
	// Borramos el menú del cajero
	bankBrowser.destroy();
	mp.gui.cursor.visible = false;
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);
	bankBrowser = null;
});