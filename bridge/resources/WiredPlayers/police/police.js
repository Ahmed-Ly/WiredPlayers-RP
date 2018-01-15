let policeControlBrowser = null;
let control = "";
let resolution;
let crimes = [];
let reinforces = [];
let handcuffed = false;

// Menú de selección de delitos
var crimeMenu = NAPI.CreateMenu("Delitos", "", 0, 0, 6);
crimeMenu.ResetKey(menuControl.Back);
crimeMenu.OnItemSelect.connect(function (sender, item, index) {
    if (index == crimeMenu.Size - 2) {
        NAPI.TriggerServerEvent("applyCrimesToPlayer", crimes.toString());
    }
	crimeMenu.Visible = false;
});
crimeMenu.OnCheckboxChange.connect(function (sender, item, checked) {
    if (checked) {
        crimes.push(item.Text);
    } else {
        var index = crimes.indexOf(item.Text);
        crimes.splice(index, 1);
    }
});
crimeMenu.Visible = false;

// Menú de controles de policía
var policeControlMenu = NAPI.CreateMenu("Controles", "", 0, 0, 6);
policeControlMenu.ResetKey(menuControl.Back);
policeControlMenu.OnItemSelect.connect(function (sender, item, index) {
	if(index != policeControlMenu.Size - 1) {
		if (NAPI.Data.GetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_POLICE_CONTROL") == 1 && index == policeControlMenu.Size - 2) {
			showPoliceControlName();
			control = "";
		} else {
			if (NAPI.Data.GetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_POLICE_CONTROL") == 2) {
				control = item.Text;
				showPoliceControlName();
			} else {
				NAPI.TriggerServerEvent("policeControlSelected", item.Text);
			}
		}
	}
    policeControlMenu.Visible = false;
});
policeControlMenu.Visible = false;

NAPI.OnResourceStart.connect(function () {
    resolution = NAPI.GetScreenResolution();
});

NAPI.OnServerEventTrigger.connect(function (name, args) {
	switch(name) {
		case "mostrarMenuDelitos":
			crimes = [];
			crimeMenu.Clear();
			populateCrimeMenu(args[0]);
			crimeMenu.Visible = true;
			break;
		case "loadPoliceControlList":
			policeControlMenu.Clear();
			populatePoliceControlMenu(args[0]);
			if (NAPI.Data.GetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_POLICE_CONTROL") == 1) {
				var menuItem = NAPI.CreateMenuItem("Nuevo control policial", "");
				policeControlMenu.AddItem(menuItem);
			}
			var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
			policeControlMenu.AddItem(cancelItem);
			policeControlMenu.Visible = true;
			break;
		case "showPoliceControlName":
			policeControlMenu.Visible = false;
			showPoliceControlName();
			control = "";
			break;
		case "updatePoliceReinforces":
			let updatedReinforces = JSON.parse(args[0]);

			// Miramos quienes tienen refuerzos activos
			for(let i = 0; i < updatedReinforces.length; i++) {
				// Obtenemos el identificador
				let police = updatedReinforces[i].playerId;
				let position = new Vector3(updatedReinforces[i].position.X, updatedReinforces[i].position.Y, updatedReinforces[i].position.Z);

				if(reinforces[police] === undefined) {
					// Creamos la nueva marca en el mapa
					let reinforcesBlip = NAPI.Blip.CreateBlip(position);
					NAPI.Blip.SetBlipShortRange(reinforcesBlip, false);
					NAPI.Blip.SetBlipSprite(reinforcesBlip, 487);
					NAPI.SetBlipColor(reinforcesBlip, 38);

					// El miembro no estaba en la lista, lo añadimos
					reinforces[police] = reinforcesBlip;
				} else {
					// Actualizamos la posición
					NAPI.SetBlipPosition(reinforces[police], position);
				}
			}
			break;
		case "reinforcesRemove":
			// Eliminamos el refuerzo de la persona
			NAPI.Entity.DeleteEntity(reinforces[args[0]]);
			reinforces[args[0]] = undefined;
			break;
		case "toggleHandcuffed":
			handcuffed = args[0];
			break;
	}
});

NAPI.OnUpdate.connect(function () {
	// Obtenemos al jugador local
	let localPlayer = NAPI.GetLocalPlayer();

    if (handcuffed) {
        NAPI.DisableControlThisFrame(12);
        NAPI.DisableControlThisFrame(13);
        NAPI.DisableControlThisFrame(14);
        NAPI.DisableControlThisFrame(15);
        NAPI.DisableControlThisFrame(16);
        NAPI.DisableControlThisFrame(17);
        NAPI.DisableControlThisFrame(22);
        NAPI.DisableControlThisFrame(24);
        NAPI.DisableControlThisFrame(25);
    }
});

NAPI.OnResourceStop.connect(function () {
    if (policeControlBrowser != null) {
        NAPI.DestroyCefBrowser(policeControlBrowser);
        policeControlBrowser = null;
    }
});

function populateCrimeMenu(crimeListJson) {
    var crimeArray = JSON.parse(crimeListJson);
    for (var i = 0; i < crimeArray.length; i++) {
        var menuCheckItem = NAPI.CreateCheckboxItem(crimeArray[i].crime, crimeArray[i].reminder, false);
        crimeMenu.AddItem(menuCheckItem);
    }
    var menuItem = NAPI.CreateMenuItem("Inculpar", "");
    crimeMenu.AddItem(menuItem);
    var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    crimeMenu.AddItem(cancelItem);
}

function populatePoliceControlMenu(policeControlListJson) {
    var policeControlArray = JSON.parse(policeControlListJson);
    for (var i = 0; i < policeControlArray.length; i++) {
        var menuItem = NAPI.CreateMenuItem(policeControlArray[i], "");
        policeControlMenu.AddItem(menuItem);
    }
}

function showPoliceControlName() {
    policeControlBrowser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
    NAPI.SetCefBrowserPosition(policeControlBrowser, 0, 0);
    NAPI.LoadPageCefBrowser(policeControlBrowser, "statics/html/policeControlName.html");
    NAPI.WaitUntilCefBrowserLoaded(policeControlBrowser);
    NAPI.SetCanOpenChat(false);
    NAPI.ShowCursor(true);
}

function policeControlSelectedName(name) {
    NAPI.TriggerServerEvent("policeControlNamed", control, name);
    NAPI.DestroyCefBrowser(policeControlBrowser);
    NAPI.SetCanOpenChat(true);
    NAPI.ShowCursor(false);
    policeControlBrowser = null;
    control = "";
}