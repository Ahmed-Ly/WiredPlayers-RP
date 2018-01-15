let factionWarningCylinder = null;
let factionWarningBlip = null;

NAPI.OnServerEventTrigger.connect(function (eventName, args) {
	switch(eventName) {
		case 'showFactionWarning':
			// Mostramos la posición del aviso
			factionWarningCylinder = NAPI.CreateMarker(1, args[0], new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), new Vector3(3.0, 3.0, 3.0), 255, 0, 0, 70);
			factionWarningBlip = NAPI.Blip.CreateBlip(args[0]);
			NAPI.SetWaypoint(args[0].X, args[0].Y);
			break;
		case 'deleteFactionWarning':
			// Eliminamos la posición del aviso
			if (factionWarningCylinder != null && factionWarningBlip != null) {
				NAPI.Entity.DeleteEntity(factionWarningCylinder);
				NAPI.Entity.DeleteEntity(factionWarningBlip);
				NAPI.RemoveWaypoint();
			}
			factionWarningCylinder = null;
			factionWarningBlip = null;
			break;
	}
});