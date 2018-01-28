mp.events.add('accountLoginForm', () => {
	// Creamos la ventana de login
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/accountLogin.html']);
});

mp.events.add('requestPlayerLogin', (password) => {
	// Hacemos que el servidor valide las credenciales
	mp.events.callRemote('loginAccount', password);
});

mp.events.add('showLoginError', () => {
});

mp.events.add('clearLoginWindow', () => {
	// Descongelamos al jugador
	mp.players.local.freezePosition(false);
	
	// Borramos la pantalla de login
	mp.events.call('destroyBrowser');
});