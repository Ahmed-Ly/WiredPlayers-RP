mp.keys.bind(0x45, false, function() {
	// Se ha pulsado la tecla 'E'
	if(!mp.players.local.vehicle || mp.players.local.seat > 0) {
		mp.events.callRemote('checkPlayerEventKeyStopAnim');
	}
});

mp.keys.bind(0x46, false, function() {
	// Se ha pulsado la tecla 'F'
	if(!mp.players.local.vehicle) {
		// Miramos si puede entrar en algún sitio
		mp.events.callRemote('checkPlayerEventKey');
	}
});

mp.keys.bind(0x4B, false, function() {
	// Se ha pulsado la tecla 'K'	
	if(mp.players.local.vehicle && mp.players.local.seat == 0) {
		// Cambiamos el estado del motor
		mp.events.callRemote('engineOnEventKey');
	}
});