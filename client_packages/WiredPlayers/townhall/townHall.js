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

let documentsMenu = NAPI.CreateMenu("Trámites", "", 0, 0, 6);
NAPI.SetMenuBannerRectangle(documentsMenu, 255, 51, 51, 255);
documentsMenu.ResetKey(menuControl.Back);
documentsMenu.OnItemSelect.connect(function (sender, item, index) {
    if (index == documentsMenu.Size - 1) {
        documentsMenu.Visible = false;
		NAPI.Entity.SetEntityPositionFrozen(NAPI.GetLocalPlayer(), false);
    } else {
		NAPI.TriggerServerEvent("documentOptionSelected", index);
    }
});
documentsMenu.Visible = false;

let finesMenu = NAPI.CreateMenu("Multas", "", 0, 0, 6);
NAPI.SetMenuBannerRectangle(finesMenu, 255, 51, 51, 255);
finesMenu.ResetKey(menuControl.Back);
finesMenu.OnItemSelect.connect(function (sender, item, index) {
    if (index == finesMenu.Size - 1) {
        documentsMenu.Visible = true;
        finesMenu.Visible = false;
    } else if (index == finesMenu.Size - 2) {
		NAPI.TriggerServerEvent("payPlayerFines", index);
        documentsMenu.Visible = true;
        finesMenu.Visible = false;
    }
});
finesMenu.Visible = false;

NAPI.OnServerEventTrigger.connect(function (name, args) {
    if (name == "showTownHallDocumentsMenu") {
		// Limpiamos las opciones de menú
        documentsMenu.Clear();

        let item1 = NAPI.CreateMenuItem("Documentación", "");
        let item2 = NAPI.CreateMenuItem("Seguro médico", "");
        let item3 = NAPI.CreateMenuItem("Licencia de taxista", "");
        let item4 = NAPI.CreateMenuItem("Multas", "");
        let cancelItem = NAPI.CreateColoredItem("Salir", "", "#DB543B", "#FF5151");

        documentsMenu.AddItem(item1);
        documentsMenu.AddItem(item2);
        documentsMenu.AddItem(item3);
        documentsMenu.AddItem(item4);
        documentsMenu.AddItem(cancelItem);

        documentsMenu.Visible = true;
		NAPI.Entity.SetEntityPositionFrozen(NAPI.GetLocalPlayer(), true);
    } else if (name == "showPlayerFineList") {
		// Ocultamos el menú principal
        documentsMenu.Visible = false;

		// Creamos el menú con las multas
		finesMenu.Clear();
		populateFinesMenu(args[0]);

		// Activamos el menú de Multas
		finesMenu.Visible = true;
	}
});

function populateFinesMenu(finesJsonArray) {
    let finesArray = JSON.parse(finesJsonArray);
    for (let i = 0; i < finesArray.length; i++) {
        let menuItem = NAPI.CreateMenuItem(finesArray[i].reason, finesArray[i].date + " - " + finesArray[i].amount + "$");
        finesMenu.AddItem(menuItem);
    }
	let acceptItem = NAPI.CreateColoredItem("Pagar", "", "#558B2F", "#33691E")
    let cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    finesMenu.AddItem(acceptItem);
    finesMenu.AddItem(cancelItem);
}