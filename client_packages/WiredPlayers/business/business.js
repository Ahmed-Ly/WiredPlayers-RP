let clothesArray = [
	{type: 0, slot: 1, desc: 'Máscaras y pasamontañas'}, {type: 0, slot: 3, desc: 'Torso'}, {type: 0, slot: 4, desc: 'Pantalones'}, {type: 0, slot: 5, desc: 'Mochilas y bolsas'}, 
	{type: 0, slot: 6, desc: 'Calzado'}, {type: 0, slot: 7, desc: 'Complementos'}, {type: 0, slot: 8, desc: 'Camisetas interiores'}, {type: 0, slot: 11, desc: 'Chaquetas'}, 
	{type: 1, slot: 0, desc: 'Gorras y sombreros'}, {type: 1, slot: 1, desc: 'Gafas'}, {type: 1, slot: 2, desc: 'Pendientes'}
];
let faceHairArray = [];
let clothesIndexArray = [];
let clothesMenuCount = 0;
let selectedClothes = -1;
let clothesShop = false;

let businessItems = null;
let businessName = null;
let businessPriceMultiplier = 0.0;
let businessMenuBrowser = null;

mp.events.add('showBusinessPurchaseMenu', (itemsJsonArray, business, multiplier) => {
	// Almacenamos los objetos y precios
	businessItems = itemsJsonArray;
	businessName = business;
	businessPriceMultiplier = multiplier;
	
	// Mostramos la ventana con los objetos a comprar
	businessMenuBrowser = mp.browsers.new('package://WiredPlayers/statics/html/businessMenu.html');
});

mp.events.add('getBusinessItems', () => {
	// Rellenamos la lista de objetos
	mp.gui.cursor.visible = true;
    businessMenuBrowser.execute(`populateBusinessItems('${businessItems}', '${businessName}', ${businessPriceMultiplier});`);
});

mp.events.add('purchaseItem', (index, amount) => {
	// Obtenemos el objeto comprado y su coste
	let playerMoney = mp.players.local.getVariable('PLAYER_MONEY');
	let purchasedItem = JSON.parse(businessItems)[index];
	let itemPrice = parseInt(Math.round(purchasedItem.products * businessPriceMultiplier) * amount);
	
	if(itemPrice > playerMoney) {
		// El jugador no tiene suficiente dinero en mano
		mp.gui.chat.push("!{#A80707} Necesitas tener " + itemPrice + "$ en mano para realizar la compra.");
	} else {
		// Efectuamos la compra
		mp.events.callRemote('businessPurchaseMade', purchasedItem.description, amount);
	}	
});

mp.events.add('cancelBusinessPurchase', () => {
    businessMenuBrowser.destroy();
	mp.gui.cursor.visible = false;
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);
    businessMenuBrowser = null;
});
/*

			purchaseMenu.Clear();
			populateBusinessPurchaseMenu(args[0], args[1]);
			purchaseMenu.Visible = true;
			clothesShop = false;
			
			var itemsArray = JSON.parse(itemsJsonArray);itemsJsonArray, multiplier)
			for (var i = 0; i < itemsArray.length; i++) {
				var value = parseInt(itemsArray[i].products * multiplier);
				var menuItem = NAPI.CreateMenuItem(itemsArray[i].description, value + "$");
				purchaseMenu.AddItem(menuItem);

				// Añadimos el identificador si es tienda de ropa
				if(clothesShop) {
					clothesIndexArray.push(itemsArray[i].clothesId);
				}
			}
			var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
			purchaseMenu.AddItem(cancelItem);
			purchaseMenu.RefreshIndex();
			
			

// Menú de artículos del negocio
var purchaseMenu = NAPI.CreateMenu("Artículos", "", 0, 0, 6);
NAPI.SetMenuBannerRectangle(purchaseMenu, 255, 0, 0, 255);
purchaseMenu.ResetKey(menuControl.Back);
purchaseMenu.OnItemSelect.connect(function (sender, item, index) {
    if (index == purchaseMenu.Size - 1) {
		if(clothesMenuCount == 0) {
			// Cerramos completamente el menú
			purchaseMenu.Visible = false;
		} else {
			// Vamos al menú anterior
			navigatePrevMenu();
		}
    } else {
		// Miramos en qué tienda está
		if(clothesShop) {
			// Tienda de ropa
			navigateNextMenu(index);
		} else {
			// Otro tipo de tienda
			if (NAPI.Data.HasEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_RIGHT_HAND") == true) {
				NAPI.SendChatMessage("Tienes la mano ocupada.");
			} else {
				var price = parseInt(item.Description.replace("$", ""));
				if (NAPI.Data.GetEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_MONEY") >= price) {
					purchaseMenu.Visible = false;
					NAPI.TriggerServerEvent("businessPurchaseMade", item.Text);
				} else {
					NAPI.SendChatMessage("Necesitas " + item.Description + " para comprar este artículo.");
				}
			}
		}
    }
});
purchaseMenu.OnIndexChange.connect(function (sender, index) {
	if(clothesMenuCount > 0 && index != purchaseMenu.Size - 1) {
		// Miramos si es ropa o accesorio
		if(clothesArray[selectedClothes].type == 0) {
			NAPI.Player.SetPlayerClothes(NAPI.GetLocalPlayer(), clothesArray[selectedClothes].slot, clothesIndexArray[index], 0);
		} else {
			NAPI.Player.SetPlayerAccessory(NAPI.GetLocalPlayer(), clothesArray[selectedClothes].slot, clothesIndexArray[index], 0);
		}
	}
});
purchaseMenu.Visible = false;

// Menú de peluquería
var hairdresserMenu = NAPI.CreateMenu("Peluquería", "", 0, 0, 6);
NAPI.SetMenuBannerRectangle(hairdresserMenu, 255, 0, 0, 255);
hairdresserMenu.ResetKey(menuControl.Back);
hairdresserMenu.OnItemSelect.connect(function (sender, item, index) {
    if (index == hairdresserMenu.Size - 2 || index == hairdresserMenu.Size - 1) {
		// Cogemos el jugador
		let player = NAPI.GetLocalPlayer();

		if(index == hairdresserMenu.Size - 2) {
			// Ponemos el nuevo estilo
			NAPI.Data.SetEntitySharedData(player, "GTAO_HAIR_MODEL", faceHairArray['hairModel']);
			NAPI.Data.SetEntitySharedData(player, "GTAO_HAIR_FIRST_COLOR", faceHairArray['hairFirstColor']);
			NAPI.Data.SetEntitySharedData(player, "GTAO_HAIR_SECOND_COLOR", faceHairArray['hairSecondColor']);
			NAPI.Data.SetEntitySharedData(player, "GTAO_BEARD_MODEL", faceHairArray['beardModel']);
			NAPI.Data.SetEntitySharedData(player, "GTAO_BEARD_COLOR", faceHairArray['beardColor']);
			NAPI.Data.SetEntitySharedData(player, "GTAO_EYEBROWS_MODEL", faceHairArray['eyebrowsModel']);
			NAPI.Data.SetEntitySharedData(player, "GTAO_EYEBROWS_COLOR", faceHairArray['eyebrowsColor']);

			// Cobramos por el cambio
			NAPI.TriggerServerEvent("hairStyleChanged");
		} else {
			// Volvemos a poner el pelo como estaba
			cancelHairdressersChanges();
		}

		// Cerramos el menú
		hairdresserMenu.Visible = false;
    }
});
hairdresserMenu.OnIndexChange.connect(function (sender, index) {
	
	if(clothesMenuCount > 0 && index != purchaseMenu.Size - 1) {
		// Miramos si es ropa o accesorio
		if(clothesArray[selectedClothes].type == 0) {
			NAPI.Player.SetPlayerClothes(NAPI.GetLocalPlayer(), clothesArray[selectedClothes].slot, clothesIndexArray[index], 0);
		} else {
			NAPI.Player.SetPlayerAccessory(NAPI.GetLocalPlayer(), clothesArray[selectedClothes].slot, clothesIndexArray[index], 0);
		}
	}
});
hairdresserMenu.Visible = false;

NAPI.OnServerEventTrigger.connect(function (eventName, args) {
	// Miramos el evento
	switch(eventName) {
		case "showBusinessPurchaseMenu":
			purchaseMenu.Clear();
			populateBusinessPurchaseMenu(args[0], args[1]);
			purchaseMenu.Visible = true;
			clothesShop = false;
			break;
		case "showClothesBusinessPurchaseMenu":
			clothesMenuCount = 0;
			purchaseMenu.Clear();
			populateClothesMenu();
			purchaseMenu.Visible = true;
			clothesShop = true;
			break;
		case "showClothesFromSelectedType":
			clothesMenuCount++;
			purchaseMenu.Clear();
			clothesIndexArray = [];
			populateBusinessPurchaseMenu(args[0], args[1]);
			if(clothesArray[selectedClothes].type == 0) {
				NAPI.Player.SetPlayerClothes(NAPI.GetLocalPlayer(), clothesArray[selectedClothes].slot, 0, 0);
			} else {
				NAPI.Player.SetPlayerAccessory(NAPI.GetLocalPlayer(), clothesArray[selectedClothes].slot, 0, 0);
			}
			break;
		case "showHairdresserMenu":
			// Obtenemos el jugador
			let player = NAPI.GetLocalPlayer();

			// Inicializamos todo
			hairdresserMenu.Clear();
			faceHairArray['hairModel'] = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_MODEL");
			faceHairArray['hairFirstColor'] = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_FIRST_COLOR");
			faceHairArray['hairSecondColor'] = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_SECOND_COLOR");
			faceHairArray['beardModel'] = NAPI.Data.GetEntitySharedData(player, "GTAO_BEARD_MODEL");
			faceHairArray['beardColor'] = NAPI.Data.GetEntitySharedData(player, "GTAO_BEARD_COLOR");
			faceHairArray['eyebrowsModel'] = NAPI.Data.GetEntitySharedData(player, "GTAO_EYEBROWS_MODEL");
			faceHairArray['eyebrowsColor'] = NAPI.Data.GetEntitySharedData(player, "GTAO_EYEBROWS_COLOR");

			// Creación del menú
			populateHairdresserMenu();
			break;
		case "cancelHairdressersChanges":
			// Cancelamos el cambio de look
			cancelHairdressersChanges();
			break;
	}
});

NAPI.OnKeyUp.connect(function (sender, e) {
    if (clothesMenuCount > 0 && e.KeyCode === Keys.Back) {
		// Vamos al menú anterior
		navigatePrevMenu();
    }
});

function populateBusinessPurchaseMenu(itemsJsonArray, multiplier) {
    var itemsArray = JSON.parse(itemsJsonArray);
    for (var i = 0; i < itemsArray.length; i++) {
        var value = parseInt(itemsArray[i].products * multiplier);
        var menuItem = NAPI.CreateMenuItem(itemsArray[i].description, value + "$");
        purchaseMenu.AddItem(menuItem);

		// Añadimos el identificador si es tienda de ropa
		if(clothesShop) {
			clothesIndexArray.push(itemsArray[i].clothesId);
		}
    }
    var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    purchaseMenu.AddItem(cancelItem);
	purchaseMenu.RefreshIndex();
}

function populateClothesMenu() {
	for (let i = 0; i < clothesArray.length; i++) {
        let menuItem = NAPI.CreateMenuItem(clothesArray[i].desc, "");
		purchaseMenu.AddItem(menuItem);
	}
    var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    purchaseMenu.AddItem(cancelItem);
	purchaseMenu.RefreshIndex();
}

function populateHairdresserMenu() {
	// Obtenemos el jugador
	let player = NAPI.GetLocalPlayer();

	// Creamos las diferentes opciones
	let hairModelList = new List(String);
	let hairFirstColorList = new List(String);
	let hairSecondColorList = new List(String);
	let beardModelList = new List(String);
	let beardColorList = new List(String);
	let eyebrowsModelList = new List(String);
	let eyebrowsColorList = new List(String);

	if(NAPI.Data.GetEntitySharedData(player, "PLAYER_SEX") == 0) {
		// Cargamos los estilos de pelo de hombre
		for(let i = 0; i < 37; i++) {
			if(i == 0) {
				hairModelList.Add("Sin pelo");
			} else {
				hairModelList.Add("Tipo " + i);
			}
		}

		// Cargamos los estilos de barba
		for(let i = -1; i < 37; i++) {
			if(i == -1) {
				beardModelList.Add("Sin barba");
			} else {
				beardModelList.Add("Tipo " + (i + 1));
			}
		}

		// Cargamos los colores de barba
		for(let i = 0; i < 64; i++) {
			beardColorList.Add("Color " + (i + 1));
		}
	} else {
		// Cargamos los estilos de pelo de mujer
		for(let i = 0; i < 39; i++) {
			if(i == 0) {
				hairModelList.Add("Sin pelo");
			} else {
				hairModelList.Add("Tipo " + i);
			}
		}
	}

	// Cargamos los colores de pelo principales
	for(let i = 0; i < 64; i++) {
		hairFirstColorList.Add("Color " + (i + 1));
	}

	// Cargamos los colores de pelo secundarios
	for(let i = 0; i < 64; i++) {
		hairSecondColorList.Add("Color " + (i + 1));
	}

	// Cargamos los estilos de cejas
	for(let i = 0; i < 34; i++) {
		eyebrowsModelList.Add("Tipo " + (i + 1));
	}

	// Cargamos los colores de cejas
	for(let i = 0; i < 64; i++) {
		eyebrowsColorList.Add("Color " + (i + 1));
	}
	
	let hairModelListItem = NAPI.CreateListItem("Estilo de pelo", "", hairModelList, faceHairArray['hairModel']);
	let hairFirstColorListItem = NAPI.CreateListItem("Color primario de pelo", "", hairFirstColorList, faceHairArray['hairFirstColor']);
	let hairSecondColorListItem = NAPI.CreateListItem("Color primario de pelo", "", hairSecondColorList, faceHairArray['hairFirstColor']);
	let eyebrowsModelListItem = NAPI.CreateListItem("Estilo de cejas", "", eyebrowsModelList, faceHairArray['eyebrowsModel']);
	let eyebrowsColorListItem = NAPI.CreateListItem("Color de cejas", "", eyebrowsColorList, faceHairArray['eyebrowsColor']);

	// Añadimos las opciones al menú
	hairdresserMenu.AddItem(hairModelListItem);
	hairdresserMenu.AddItem(hairFirstColorListItem);
	hairdresserMenu.AddItem(hairSecondColorListItem);
	hairdresserMenu.AddItem(eyebrowsModelListItem);
	hairdresserMenu.AddItem(eyebrowsColorListItem);	

	if(NAPI.Data.GetEntitySharedData(player, "PLAYER_SEX") == 0) {
		// Añadimos la barba
		let beardModelListItem = NAPI.CreateListItem("Estilo de barba", "", beardModelList, faceHairArray['beardModel']);
		let beardColorListItem = NAPI.CreateListItem("Color de barba", "", beardColorList, faceHairArray['beardColor']);

		// Añadimos las opciones al menú
		hairdresserMenu.AddItem(beardModelListItem);
		hairdresserMenu.AddItem(beardColorListItem);

		// Metemos los eventos de cambio de lista
		beardModelListItem.OnListChanged.connect(function(sender, newIndex) {
			faceHairArray['beardModel'] = newIndex - 1;
			NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 1, faceHairArray['beardModel'], 0.99);
		});
		
		beardColorListItem.OnListChanged.connect(function(sender, newIndex) {
			faceHairArray['beardColor'] = newIndex;
			NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 1, 1, faceHairArray['beardColor'], 0);
		});
	}
	
	hairModelListItem.OnListChanged.connect(function(sender, newIndex) {
		faceHairArray['hairModel'] = newIndex;
		NAPI.CallNative("SET_PED_COMPONENT_VARIATION", player, 2, faceHairArray['hairModel'], 0, 0);
	});

	hairFirstColorListItem.OnListChanged.connect(function(sender, newIndex) {
		faceHairArray['hairFirstColor'] = newIndex;
		NAPI.CallNative("_SET_PED_HAIR_COLOR", player, faceHairArray['hairFirstColor'], faceHairArray['hairSecondColor']);
	});

	hairSecondColorListItem.OnListChanged.connect(function(sender, newIndex) {
		faceHairArray['hairSecondColor'] = newIndex;
		NAPI.CallNative("_SET_PED_HAIR_COLOR", player, faceHairArray['hairFirstColor'], faceHairArray['hairSecondColor']);
	});

	eyebrowsModelListItem.OnListChanged.connect(function(sender, newIndex) {
		faceHairArray['eyebrowsModel'] = newIndex;
		NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 2, faceHairArray['eyebrowsModel'], 0.99);
	});

	eyebrowsColorListItem.OnListChanged.connect(function(sender, newIndex) {
		faceHairArray['eyebrowsColor'] = newIndex;
		NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 2, 1, faceHairArray['eyebrowsColor'], 0);
	});

	// Añadimos los botones de aceptar y cancelar
	let acceptItem = NAPI.CreateColoredItem("Aceptar", "", "#558B2F", "#33691E");
    let cancelItem = NAPI.CreateColoredItem("Cancelar", "", "#C62828", "#B71C1C");
    hairdresserMenu.AddItem(acceptItem);
    hairdresserMenu.AddItem(cancelItem);

	// Mostramos el menú
	hairdresserMenu.RefreshIndex();
	hairdresserMenu.Visible = true;
}

function navigatePrevMenu() {
	// Cerramos el menú de objetos
    purchaseMenu.Clear();
	populateClothesMenu();
	clothesMenuCount--;

	// Ponemos al jugador su vestimenta equipada
	NAPI.TriggerServerEvent("dressEquipedClothes", clothesArray[selectedClothes].type, clothesArray[selectedClothes].slot);
}

function navigateNextMenu(index) {
	switch(clothesMenuCount) {
		case 0:
			selectedClothes = index;
			NAPI.TriggerServerEvent("clothesSlotSelected", clothesArray[index].type, clothesArray[index].slot);
			break;
		case 1:
			NAPI.TriggerServerEvent("clothesItemSelected", clothesIndexArray[index], clothesArray[selectedClothes].type, clothesArray[selectedClothes].slot);
			break;
	}
}

function cancelHairdressersChanges() {
	// Obtenemos el jugador
	let player = NAPI.GetLocalPlayer();

	// Recogemos las antiguas variables
	let hairModel = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_MODEL");
	let hairFirstColor = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_FIRST_COLOR");
	let hairSecondColor = NAPI.Data.GetEntitySharedData(player, "GTAO_HAIR_SECOND_COLOR");
	let beardModel = NAPI.Data.GetEntitySharedData(player, "GTAO_BEARD_MODEL");
	let beardColor = NAPI.Data.GetEntitySharedData(player, "GTAO_BEARD_COLOR");
	let eyebrowsModel = NAPI.Data.GetEntitySharedData(player, "GTAO_EYEBROWS_MODEL");
	let eyebrowsColor = NAPI.Data.GetEntitySharedData(player, "GTAO_EYEBROWS_COLOR");

	// Volvemos a poner todo como estaba
	NAPI.CallNative("SET_PED_COMPONENT_VARIATION", player, 2, hairModel, 0, 0);
	NAPI.CallNative("_SET_PED_HAIR_COLOR", player, hairFirstColor, hairSecondColor);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 1, beardModel, 0.99);
	NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 1, 1, beardColor, 0);
	NAPI.CallNative("SET_PED_HEAD_OVERLAY", player, 2, eyebrowsModel, 0.99);
	NAPI.CallNative("_SET_PED_HEAD_OVERLAY_COLOR", player, 2, 1, eyebrowsColor, 0);
}*/