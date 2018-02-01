let faceHairArray = [];
let clothesIndexArray = [];
let clothesMenuCount = 0;
let selectedClothes = -1;
let clothesShop = false;

let businessItems = null;
let businessPriceMultiplier = 0.0;

mp.events.add('showBusinessPurchaseMenu', (itemsJsonArray, business, multiplier) => {
	// Almacenamos los objetos y precios
	businessItems = itemsJsonArray;
	businessPriceMultiplier = multiplier;
	
	// Mostramos la ventana con los objetos a comprar
	mp.events.call('createBrowser', ['package://WiredPlayers/statics/html/sideMenu.html', 'populateBusinessItems', itemsJsonArray, business, multiplier]);
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
	// Borramos el menú del negocio
	mp.events.call('destroyBrowser');
	
	// Reactivamos el chat
	mp.gui.chat.activate(true);
	mp.gui.chat.show(true);
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
}*/