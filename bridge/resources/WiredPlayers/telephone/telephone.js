let action = 0;
let contact = 0;
let resolution = null;
let contactBrowser = null;
let messageBrowser = null;
let contactsArray = [];

// Menú de contactos
let contactsMenu = NAPI.CreateMenu("Contactos", "", 0, 0, 6);
NAPI.SetMenuBannerRectangle(contactsMenu, 255, 51, 51, 255);
contactsMenu.ResetKey(menuControl.Back);
contactsMenu.OnItemSelect.connect(function (sender, item, index) {
    if (index == contactsMenu.Size - 1) {
        contactsMenu.Visible = false;
    } else {
		// Cogemos el id del contacto
		contact = contactsArray[index].id;

		switch(action) {
			case 2:
				// Opción de modificar
				contactBrowser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
				NAPI.SetCefBrowserPosition(contactBrowser, 0, 0);
				NAPI.LoadPageCefBrowser(contactBrowser, "statics/html/addPhoneContact.html");
				NAPI.WaitUntilCefBrowserLoaded(contactBrowser);
				NAPI.SetCanOpenChat(false);
				NAPI.ShowCursor(true);

				// Ocultamos el menú
				contactsMenu.Visible = false;
				break;
			case 3:
				// Opción de borrar
				NAPI.TriggerServerEvent("deleteContact", contact);
				
				// Ocultamos el menú
				contactsMenu.Visible = false;
				break;
			case 5:
				// Opción de SMS
				messageBrowser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
				NAPI.SetCefBrowserPosition(messageBrowser, 0, 0);
				NAPI.LoadPageCefBrowser(messageBrowser, "statics/html/sendContactMessage.html");
				NAPI.WaitUntilCefBrowserLoaded(messageBrowser);
				NAPI.SetCanOpenChat(false);
				NAPI.ShowCursor(true);

				// Ocultamos el menú
				contactsMenu.Visible = false;
		}
    }
});
contactsMenu.Visible = false;


NAPI.OnResourceStart.connect(function () {
	// Calculamos la resolucion
    resolution = NAPI.GetScreenResolution();
});

NAPI.OnServerEventTrigger.connect(function (name, args) {
    if (name == "showPhoneContacts") {
		// Creamos el menú con los contactos
		contactsMenu.Clear();
		populateContactsMenu(args[0]);
		action = args[1];

		// Activamos el menú de contactos
		contactsMenu.Visible = true;
    } else if (name == "addContactWindow") {
		// Recogemos la acción
		action = args[0];

		// Mostramos el la interfaz de alta
        contactBrowser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
        NAPI.SetCefBrowserPosition(contactBrowser, 0, 0);
        NAPI.LoadPageCefBrowser(contactBrowser, "statics/html/addPhoneContact.html");
        NAPI.WaitUntilCefBrowserLoaded(contactBrowser);
        NAPI.SetCanOpenChat(false);
        NAPI.ShowCursor(true);
	}
});

NAPI.OnResourceStop.connect(function () {
	// Eliminamos la interfaz web
    if (contactBrowser != null) {
        NAPI.DestroyCefBrowser(contactBrowser);
        contactBrowser = null;
    }    
	
	if (messageBrowser != null) {
        NAPI.DestroyCefBrowser(messageBrowser);
        messageBrowser = null;
    }
});

function populateContactsMenu(contactsJsonArray) {
    contactsArray = JSON.parse(contactsJsonArray);
    for (let i = 0; i < contactsArray.length; i++) {
		let number = contactsArray[i].contactNumber.toString();
        let menuItem = NAPI.CreateMenuItem(contactsArray[i].contactName, number);
        contactsMenu.AddItem(menuItem);
    }
    let cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    contactsMenu.AddItem(cancelItem);
}

function preloadContactData() {
	if(contact > 0) {
		// Cargamos los datos del contacto
		let number = contactsArray[contact].contactNumber;
		let name = contactsArray[contact].contactName;

		// Rellenamos los datos en el navegador
        contactBrowser.call("populateContactData", number, name);
	}
}

function setContactData(number, name) {
	// Eliminamos la interfaz web
	NAPI.DestroyCefBrowser(contactBrowser);
	NAPI.SetCanOpenChat(true);
    NAPI.ShowCursor(false);
	contactBrowser = null;

	if(action == 4) {
		// Nuevo Contactos
		NAPI.TriggerServerEvent("addNewContact", number, name);
	} else {
		// Edición de Contactos
		NAPI.TriggerServerEvent("modifyContact", contact, number, name);
	}
}

function sendPhoneMessage(message) {
	// Eliminamos la interfaz web
	NAPI.DestroyCefBrowser(messageBrowser);
	NAPI.SetCanOpenChat(true);
    NAPI.ShowCursor(false);
	messageBrowser = null;

	// Mandamos el mensaje al objetivo
	NAPI.TriggerServerEvent("sendPhoneMessage", contact, message);
}

function cancelMessage() {
	// Eliminamos la interfaz web
	NAPI.DestroyCefBrowser(messageBrowser);
	NAPI.SetCanOpenChat(false);
    NAPI.ShowCursor(true);
	messageBrowser = null;

	// Volvemos a mostrar el Menú
	contactsMenu.Visible = true;
}