let clothesTypesArray = [
    { type: 0, slot: 1, desc: 'Máscaras y pasamontañas' }, { type: 0, slot: 3, desc: 'Torso' }, { type: 0, slot: 4, desc: 'Pantalones' }, { type: 0, slot: 5, desc: 'Mochilas y bolsas' },
    { type: 0, slot: 6, desc: 'Calzado' }, { type: 0, slot: 7, desc: 'Complementos' }, { type: 0, slot: 8, desc: 'Camisetas interiores' }, { type: 0, slot: 11, desc: 'Chaquetas' },
    { type: 1, slot: 0, desc: 'Gorras y sombreros' }, { type: 1, slot: 1, desc: 'Gafas' }, { type: 1, slot: 2, desc: 'Pendientes' }
];
let clothesArray = [];
let clothesNames = [];
let wardrobeMenuCount = 0;
let selectedClothes = -1;

// Menú de prendas del armario
var wardrobeMenu = NAPI.CreateMenu("Armario", "", 0, 0, 6);
NAPI.SetMenuBannerRectangle(wardrobeMenu, 255, 0, 0, 255);
wardrobeMenu.ResetKey(menuControl.Back);
wardrobeMenu.OnItemSelect.connect(function (sender, item, index) {
    if (index == wardrobeMenu.Size - 1) {
        if (selectedClothes == -1) {
            // Cerramos completamente el menú
            wardrobeMenu.Visible = false;
        } else {
            // Vamos al menú anterior
            navigatePrevMenu();
        }
    } else {
        navigateNextMenu(index);
    }
});
wardrobeMenu.Visible = false;

NAPI.OnServerEventTrigger.connect(function (name, args) {
    if (name == "showPlayerWardrobe") {
        wardrobeMenuCount = 0;
        wardrobeMenu.Clear();
        populateWardrobeMainMenu();
        clothesArray = JSON.parse(args[0]);
        clothesNames = JSON.parse(args[1]);
        wardrobeMenu.Visible = true;
    }
});

NAPI.OnKeyUp.connect(function (sender, e) {
    if (wardrobeMenuCount > 0 && e.KeyCode === Keys.Back) {
        // Vamos al menú anterior
        navigatePrevMenu();
    }
});

function populateWardrobeMainMenu() {
    for (let i = 0; i < clothesTypesArray.length; i++) {
        let menuItem = NAPI.CreateMenuItem(clothesTypesArray[i].desc, "");
        wardrobeMenu.AddItem(menuItem);
    }
    var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    wardrobeMenu.AddItem(cancelItem);
    wardrobeMenu.RefreshIndex();
}

function populateWardrobeType() {
    for (let i = 0; i < clothesNames.length; i++) {
        let menuItem = NAPI.CreateMenuItem(clothesNames[i], "");
        wardrobeMenu.AddItem(menuItem);
    }
    var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    wardrobeMenu.AddItem(cancelItem);
    wardrobeMenu.RefreshIndex();
}

function navigatePrevMenu() {
    selectedClothes = -1;
    wardrobeMenu.Clear();
    populateWardrobeMainMenu();
    wardrobeMenuCount--;
}

function navigateNextMenu(index) {
    switch (wardrobeMenuCount) {
        case 0:
            selectedClothes = index;
            wardrobeMenu.Clear();
            populateWardrobeType();
            break;
        case 1:
            NAPI.TriggerServerEvent("wardrobeClothesItemSelected", clothesArray[selectedClothes].id, clothesArray[selectedClothes].type, clothesArray[selectedClothes].slot);
            break;
    }
    wardrobeMenuCount++;
}