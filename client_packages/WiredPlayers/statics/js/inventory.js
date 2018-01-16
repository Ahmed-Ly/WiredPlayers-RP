function populateInventory(inventoryJson, title) {
	// Obtenemos el array de objetos
	let inventory = JSON.parse(inventoryJson);
	
	// Obtenemos los contenedores de elementos
	let titleContainer = document.getElementById('identifier');
	let inventoryContainer = document.getElementById('inventory');
	
	for(let i = 0; i < inventory.length; i++) {
		// Obtenemos el objeto del inventario
		let item = inventory[i];
		
		// Creamos los elementos para mostrar cada objeto
		let itemContainer = document.createElement('div');
		let amountContainer = document.createElement('div');
		let itemImage = document.createElement('img');
		
		// Añadimos las clases a cada elemento
		itemContainer.classList.add('inventory-item');
		
		// Añadimos el contenido de cada elemento
		itemImage.src = '../img/inventory/' + item.hash + '.png';
		amountContainer.textContent = item.amount;
	}
}