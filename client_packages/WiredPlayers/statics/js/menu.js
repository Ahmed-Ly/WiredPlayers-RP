let purchasedAmount = 1;
let selected = null;

function populateBusinessItems(businessItemsJson, businessName, multiplier) {
	// Inicializamos los valores
	purchasedAmount = 1;
	selected = null;

	// Obtenemos la lista de objetos a mostrar
	let businessItemsArray = JSON.parse(businessItemsJson);
	let header = document.getElementById('header');
	let content = document.getElementById('content');
	let options = document.getElementById('options');
	
	// Añadimos la cabecera del menú
	header.textContent = businessName;
	
	for(let i = 0; i < businessItemsArray.length; i++) {
		// Obtenemos el objeto en curso
		let item = businessItemsArray[i];
		
		// Creamos los elementos para mostrar cada objeto
		let itemContainer = document.createElement('div');
		let imageContainer = document.createElement('div');
		let infoContainer = document.createElement('div');
		let descContainer = document.createElement('div');
		let purchaseContainer = document.createElement('div');
		let priceContainer = document.createElement('div');
		let itemAmountContainer = document.createElement('div');
		let amountTextContainer = document.createElement('div');
		let addSubstractContainer = document.createElement('div');
		let itemImage = document.createElement('img');
		let itemDescription = document.createElement('span');
		let itemPrice = document.createElement('span');
		let itemAmount = document.createElement('span');
		let itemAdd = document.createElement('span');
		let itemSubstract = document.createElement('span');
		
		// Añadimos las clases a cada elemento
		itemContainer.classList.add('item-row');
		imageContainer.classList.add('item-image');
		infoContainer.classList.add('item-content');
		descContainer.classList.add('item-header');
		purchaseContainer.classList.add('item-purchase');
		priceContainer.classList.add('item-price-container');
		itemAmountContainer.classList.add('item-amount-container', 'hidden');
		amountTextContainer.classList.add('item-amount-desc-container');
		addSubstractContainer.classList.add('item-add-substract-container');
		itemDescription.classList.add('item-description');
		itemPrice.classList.add('item-price');
		itemAmount.classList.add('item-amount-description');
		itemAdd.classList.add('item-adder');
		itemSubstract.classList.add('item-substract', 'hidden');		
		
		// Añadimos el contenido de cada elemento
		itemImage.src = '../img/inventory/' + item.hash + '.png';
		itemDescription.textContent = item.description;
		itemPrice.innerHTML = '<b>Precio unitario: </b>' + Math.round(item.products * multiplier) + '$';
		itemAmount.innerHTML = '<b>Cantidad: </b>' + purchasedAmount;
		itemAdd.textContent = '+';
		itemSubstract.textContent = '-';
		
		// Ponemos la función para cada elemento
		itemContainer.onclick = (function() {
			// Comprobamos que se ha pulsado en un elemento no seleccionado
			if(selected !== i) {
				// Miramos si había algún elemento seleccionado
				if(selected != null) {
					let previousSelected = document.getElementsByClassName('item-row')[selected];
					let previousAmountNode = findFirstChildByClass(previousSelected, 'item-amount-container');
					previousSelected.classList.remove('active-item');
					previousAmountNode.classList.add('hidden');
				}
				
				// Seleccionamos el elemento pulsado
				let currentSelected = document.getElementsByClassName('item-row')[i];
				let currentAmountNode = findFirstChildByClass(currentSelected, 'item-amount-container');
				currentSelected.classList.add('active-item');
				currentAmountNode.classList.remove('hidden');
				
				// Guardamos el nuevo índice seleccionado
				purchasedAmount = 1;
				selected = i;
				
				// Actualizamos el texto del elemento
				itemAmount.innerHTML = '<b>Cantidad: </b>' + purchasedAmount;
				document.getElementsByClassName('item-adder')[selected].classList.remove('hidden');
				document.getElementsByClassName('item-substract')[selected].classList.add('hidden');
			}
		});
		
		itemAdd.onclick = (function() {
			// Sumamos una unidad
			purchasedAmount++;
			
			// Obtenemos ambos botones
			let adderButton = document.getElementsByClassName('item-adder')[selected];
			let substractButton = document.getElementsByClassName('item-substract')[selected];
			
			if(purchasedAmount == 10) {
				// Ha llegado al máximo
				adderButton.classList.add('hidden');
			} else if(substractButton.classList.contains('hidden') === true) {
				// Volvemos el elemento visible
				substractButton.classList.remove('hidden');
			}
			
			// Actualizamos la cantidad
			let amountSpan = document.getElementsByClassName('item-amount-description')[selected];
			amountSpan.innerHTML = '<b>Cantidad: </b>' + purchasedAmount;
		});
		
		itemSubstract.onclick = (function() {
			// Restamos una unidad
			purchasedAmount--;
			
			// Obtenemos ambos botones
			let adderButton = document.getElementsByClassName('item-adder')[selected];
			let substractButton = document.getElementsByClassName('item-substract')[selected];
			
			if(purchasedAmount == 1) {
				// Ha llegado al mínimo
				substractButton.classList.add('hidden');
			} else if(adderButton.classList.contains('hidden') === true) {
				// Volvemos el elemento visible
				adderButton.classList.remove('hidden');
			}
			
			// Actualizamos la cantidad
			let amountSpan = document.getElementsByClassName('item-amount-description')[selected];
			amountSpan.innerHTML = '<b>Cantidad: </b>' + purchasedAmount;
		});
		
		// Ordenamos la jerarquía de elementos
		content.appendChild(itemContainer);
		itemContainer.appendChild(imageContainer);
		itemContainer.appendChild(infoContainer);
		imageContainer.appendChild(itemImage);
		infoContainer.appendChild(descContainer);
		descContainer.appendChild(itemDescription);
		infoContainer.appendChild(purchaseContainer);
		purchaseContainer.appendChild(priceContainer);
		priceContainer.appendChild(itemPrice);
		purchaseContainer.appendChild(itemAmountContainer);
		itemAmountContainer.appendChild(amountTextContainer);
		amountTextContainer.appendChild(itemAmount);
		itemAmountContainer.appendChild(addSubstractContainer);
		addSubstractContainer.appendChild(itemAdd);
		addSubstractContainer.appendChild(itemSubstract);
	}
	
	// Añadimos los botones
	let purchaseButton = document.createElement('div');
	let cancelButton = document.createElement('div');
	
	// Añadimos las clases a cada botón
	purchaseButton.classList.add('double-button', 'accept-button');
	cancelButton.classList.add('double-button', 'cancel-button');
	
	// Añadimos el texto de los botones
	purchaseButton.textContent = 'Comprar';
	cancelButton.textContent = 'Salir';
	
	// Ponemos la función para cada elemento
	purchaseButton.onclick = (function() {
		// Mandamos la acción de compra si ha seleccionado algo
		if(selected != null) {
			mp.trigger('purchaseItem', selected, purchasedAmount);
		}
	});
	
	cancelButton.onclick = (function() {
		// Cerramos la ventana de compra
		mp.trigger('cancelBusinessPurchase');
	});
		
	// Ordenamos la jerarquía de elementos
	options.appendChild(purchaseButton);
	options.appendChild(cancelButton);
}

function findFirstChildByClass(element, className) {
	let foundElement = null, found;
	function recurse(element, className, found) {
		for (let i = 0; i < element.childNodes.length && !found; i++) {
			let el = element.childNodes[i];
			let classes = el.className != undefined? el.className.split(" ") : [];
			for (let j = 0, jl = classes.length; j < jl; j++) {
				if (classes[j] == className) {
					found = true;
					foundElement = element.childNodes[i];
					break;
				}
			}
			if(found)
				break;
			recurse(element.childNodes[i], className, found);
		}
	}
	recurse(element, className, false);
	return foundElement;
}		