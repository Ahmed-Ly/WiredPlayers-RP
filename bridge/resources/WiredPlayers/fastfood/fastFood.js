let fastFoodCylinder = null;
let fastFoodBlip = null;
let fastFoodOrders = 0;

// Menú de selección de pedidos
var fastFoodMenu = NAPI.CreateMenu("Pedidos de comida rápida", "", 0, 0, 6);
fastFoodMenu.ResetKey(menuControl.Back);
fastFoodMenu.OnItemSelect.connect(function (sender, item, index) {
	if (index == fastFoodMenu.Size - 1) {
        fastFoodMenu.Visible = false;
    } else {
		var orderId = item.Text.split('#')[1];
		fastFoodMenu.Visible = false;
		NAPI.TriggerServerEvent("takeFastFoodOrder", orderId);
	}
});
fastFoodMenu.Visible = false;

NAPI.OnServerEventTrigger.connect(function (name, args) {
    if (name == "mostrarRepartosComidaRapida") {
        fastFoodOrders = 0;
        fastFoodMenu.Clear();
        populateFastFoodMenu();
        if (fastFoodOrders > 0) {
            fastFoodMenu.Visible = true;
        } else {
            NAPI.SendChatMessage("No hay ningún pedido.");
        }
    } else if (name == "fastFoodDestinationCheckPoint") {
        fastFoodCylinder = NAPI.CreateMarker(1, args[0], new Vector3(0.0, 0.0, 0.0), new Vector3(0.0, 0.0, 0.0), new Vector3(3.0, 3.0, 3.0), 255, 0, 0, 70);
        fastFoodBlip = NAPI.Blip.CreateBlip(args[0]);
		NAPI.SetWaypoint(args[0].X, args[0].Y);
		NAPI.Blip.SetBlipShortRange(fastFoodBlip, false);
    } else if (name == "fastFoodDeliverBack") {
        NAPI.Entity.SetEntityPosition(fastFoodCylinder, args[0]);
        NAPI.SetBlipPosition(fastFoodBlip, args[0]);
		NAPI.SetWaypoint(args[0].X, args[0].Y);
    } else if (name == "fastFoodDeliverFinished") {
        if (fastFoodCylinder != null && fastFoodBlip != null) {
            NAPI.Entity.DeleteEntity(fastFoodCylinder);
            NAPI.Entity.DeleteEntity(fastFoodBlip);
			NAPI.RemoveWaypoint();
        }
        fastFoodCylinder = null;
        fastFoodBlip = null;
    }
});

function populateFastFoodMenu() {
    var fastFoodItems = NAPI.GetWorldSharedData("FASTFOOD_LIST");
    var fastFoodArray = JSON.parse(fastFoodItems);
    for (var i = 0; i < fastFoodArray.length; i++) {
        if (!fastFoodArray[i].taken) {
            var orderNumber = "Pedido #" + fastFoodArray[i].id;
            var orderDescription = fastFoodArray[i].pizzas + "x Pizzas, ";
            orderDescription += fastFoodArray[i].hamburgers + "x Hamburguesas, ";
            orderDescription += fastFoodArray[i].sandwitches + "x Sandwitches, ";
            var menuItem = NAPI.CreateMenuItem(orderNumber, orderDescription);
            fastFoodMenu.AddItem(menuItem);
            fastFoodOrders++;
        }
    }
    var cancelItem = NAPI.CreateColoredItem("Salir", "", "#C62828", "#B71C1C");
    fastFoodMenu.AddItem(cancelItem);
}