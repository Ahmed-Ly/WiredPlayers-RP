let customBrowser = null;
let parameters = [];

mp.events.add('createBrowser', (arguments) => {
	// Comprobamos que no haya ninguna ventana abierta
	if(customBrowser == null) {
		// Guardamos los parámetros
		parameters = arguments.slice(1, arguments.length);
		
		// Creamos el navegador
		customBrowser = mp.browsers.new(arguments[0]);
	}
});

mp.events.add('browserDomReady', (browser) => {
	if(customBrowser === browser) {
		// Activamos el cursor
		mp.gui.cursor.visible = true;
		
		if(parameters.length > 0) {
			// Llamamos a la función de inicialización
			mp.events.call('executeFunction', parameters);
		}
	}
});

mp.events.add('executeFunction', (arguments) => {
	// Declaramos los parametros en modo lista
	let input = '';
	
	for(let i = 1; i < arguments.length; i++) {
		// Miramos si hay algún parámetro añadido
		if(input.length > 0) {
			input += ', \'' + arguments[i] + '\'';
		} else {
			input = '\'' + arguments[i] + '\'';
		}
	}
	
	// Llamamos a la funcion
	customBrowser.execute(`${arguments[0]}(${input});`);
});

mp.events.add('destroyBrowser', () => {
	// Deshabilitamos el cursor
	mp.gui.cursor.visible = false;
	
	// Eliminamos el navegador
	customBrowser.destroy();
	customBrowser = null;
});