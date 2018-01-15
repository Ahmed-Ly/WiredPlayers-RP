let loginBrowser = null;
let resolution = null;

mp.events.add('accountLoginForm', () => {
	// Creamos la ventana de login
	loginBrowser = mp.browsers.new('package://WiredPlayers/statics/html/accountLogin.html');
	mp.gui.cursor.visible = true;
});

mp.events.add('requestPlayerLogin', (password) => {
	// Hacemos que el servidor valide las credenciales
	mp.events.callRemote('loginAccount', password);
});

mp.events.add('showLoginError', () => {
});

mp.events.add('clearLoginWindow', () => {
	// Borramos la pantalla de login
	loginBrowser.destroy();
	mp.gui.cursor.visible = false;
	loginBrowser = null;
});

// Menú de selección de personajes
/*var playersMenu = NAPI.CreateMenu("Personajes", "", 0, 0, 6);
playersMenu.ResetKey(menuControl.Back);
playersMenu.OnItemSelect.connect(function (sender, item, index) {
    if (item.Text == "Crear personaje") {
		NAPI.TriggerServerEvent("setCharacterIntoCreator");
    } else {
        NAPI.TriggerServerEvent("loadCharacter", item.Text);
    }
	playersMenu.Visible = false;
});
playersMenu.Visible = false;

NAPI.OnResourceStart.connect(function () {
    resolution = NAPI.GetScreenResolution();
});

mp.ev

NAPI.OnServerEventTrigger.connect(function (name, args) {    
    if (name == "accountRegisterForm") {
        browser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
        NAPI.SetCefBrowserPosition(browser, 0, 0);
        NAPI.LoadPageCefBrowser(browser, "statics/html/accountRegister.html");
        NAPI.WaitUntilCefBrowserLoaded(browser);
        NAPI.SetCanOpenChat(false);
        NAPI.ShowCursor(true);
    } else if (name == "accountLoginForm") {            
        browser = NAPI.CreateCefBrowser(resolution.Width, resolution.Height, true);
        NAPI.WaitUntilCefBrowserInit(browser);
        NAPI.SetCefBrowserPosition(browser, 0, 0);
        NAPI.LoadPageCefBrowser(browser, "statics/html/accountLogin.html");
        NAPI.WaitUntilCefBrowserLoaded(browser);
        NAPI.ShowCursor(true);
    } else if (name == "showLoginError") {

    } else if (name == "clearLoginWindow") {
        NAPI.DestroyCefBrowser(browser);
        NAPI.ShowCursor(false);
        browser = null;
    } else if (name == "showPlayersMenu") {
        var players = JSON.parse(args[0]);
        playersMenu.Clear();
        for (var i = 0; i < players.length; i++) {
            var menuItem = NAPI.CreateMenuItem(players[i], "");
            playersMenu.AddItem(menuItem);
        }
        var createPlayerItem = NAPI.CreateMenuItem("Crear personaje", "Crea un nuevo personaje");
        playersMenu.AddItem(createPlayerItem);
        playersMenu.Visible = true;
    }
});

NAPI.OnResourceStop.connect(function () {
    if (browser != null) {
        NAPI.DestroyCefBrowser(browser);
        browser = null;
    }
});

NAPI.OnKeyUp.connect(function (sender, e) {
    if (e.KeyCode === Keys.F && playersMenu.Visible === false) {
        if (NAPI.Data.HasEntitySharedData(NAPI.GetLocalPlayer(), "create-area") === true && NAPI.Data.HasEntitySharedData(NAPI.GetLocalPlayer(), "PLAYER_CREATING_CHARACTER") === false) {
            var inCreateArea = NAPI.Data.GetEntitySharedData(NAPI.GetLocalPlayer(), "create-area");
            if (inCreateArea) {
                playersMenu.Clear();
                NAPI.TriggerServerEvent("getPlayerCharacters");
            }
        }
    } else if (e.KeyCode === Keys.Back) {
        if (playersMenu.Visible === true) {
            NAPI.TriggerServerEvent("unfreezePlayer");
            playersMenu.Visible = false;
            NAPI.ShowCursor(false);
        }
    }
});

function checkCredentials(forumName, password, action) {
    if (action == "register") {
        NAPI.DestroyCefBrowser(browser);
        NAPI.SetCanOpenChat(true);
        NAPI.ShowCursor(false);
        browser = null;
        NAPI.TriggerServerEvent("registerAccount", forumName, password);
    } else if (action == "login") {
        NAPI.TriggerServerEvent("loginAccount", password);
    }
}*/