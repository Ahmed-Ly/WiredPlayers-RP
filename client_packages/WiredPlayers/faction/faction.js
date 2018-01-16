let factionWarningBlip = null;

mp.events.add('showFactionWarning', (position) => {
	// Creamos una marca con la posición del punto
	factionWarningBlip = mp.blips.new(1, position, {color: 1});
});

mp.events.add('deleteFactionWarning', () => {
	// Borramos la marca del mapa
	factionWarningBlip.destroy();
	factionWarningBlip = null;
});