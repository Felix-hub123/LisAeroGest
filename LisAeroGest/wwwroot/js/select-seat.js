let baseFlightPrice = 0;
let extraLuggageFee = 0;
let mealFee = 0;
let passengerCount = 1;

// Array de lugares selecionados
let selectedSeats = [];

function initSeatSelection(flightPrice, luggageFee, mealPrice) {
    baseFlightPrice = flightPrice;
    extraLuggageFee = luggageFee;
    mealFee = mealPrice;

    // Ler o número de passageiros do hidden input
    const passengerInput = document.getElementById('passengerCount');
    if (passengerInput) {
        passengerCount = parseInt(passengerInput.value, 10) || 1;
    }

    updateSelectedSeatsUI();
}

function selectSeat(button) {
    if (!button) return;

    const seatId = parseInt(button.getAttribute('data-seat-id'), 10);
    const seatCode = button.getAttribute('data-seat-code');
    const seatClass = button.getAttribute('data-seat-class') || 'Económica';
    const seatPrice = parseFloat(button.getAttribute('data-seat-price')) || 0;

    const existingIndex = selectedSeats.findIndex(s => s.id === seatId);

    if (existingIndex >= 0) {
        // Desselecionar
        selectedSeats.splice(existingIndex, 1);
        button.classList.remove('seat-selected');
    } else {
        // Verificar limite
        if (selectedSeats.length >= passengerCount) {
            alert(`Só pode selecionar ${passengerCount} lugar(es) — um por passageiro.`);
            return;
        }

        selectedSeats.push({
            id: seatId,
            code: seatCode,
            seatClass: seatClass,
            price: seatPrice
        });
        button.classList.add('seat-selected');
    }

    updateSelectedSeatsUI();
}

function updateSelectedSeatsUI() {
    const seatDisplay = document.getElementById('seatDisplay');
    const seatClassDisplay = document.getElementById('seatClassDisplay');
    const seatPriceDisplay = document.getElementById('seatPrice');
    const btnSubmit = document.getElementById('btnSubmit');
    const selectedCountBadge = document.getElementById('selectedCount');
    const selectedSeatsContainer = document.getElementById('selectedSeatsContainer');

    // Contador
    if (selectedCountBadge) {
        selectedCountBadge.textContent = selectedSeats.length;
        selectedCountBadge.className = selectedSeats.length === passengerCount
            ? 'badge bg-success fs-6'
            : 'badge bg-primary fs-6';
    }

    // Display
    if (selectedSeats.length === 0) {
        seatDisplay.textContent = 'Nenhum';
        seatDisplay.className = 'fs-2 fw-bold text-primary';
        seatClassDisplay.textContent = 'Selecione os lugares no mapa';
        seatPriceDisplay.textContent = formatCurrency(0);
        btnSubmit.disabled = true;
        btnSubmit.innerHTML = '<i class="bi bi-cart-plus me-2"></i>Continuar para reserva';
    } else {
        seatDisplay.textContent = selectedSeats.map(s => s.code).join(', ');
        seatDisplay.className = 'fs-2 fw-bold text-success';

        seatClassDisplay.textContent = selectedSeats
            .map(s => `${s.code} (${s.seatClass})`)
            .join(' • ');

        const seatsTotal = selectedSeats.reduce((sum, s) => sum + s.price, 0);
        seatPriceDisplay.textContent = formatCurrency(seatsTotal);

        // Botão só ativa quando TODOS os lugares estiverem selecionados
        btnSubmit.disabled = selectedSeats.length !== passengerCount;

        if (selectedSeats.length === passengerCount) {
            btnSubmit.innerHTML = '<i class="bi bi-cart-plus me-2"></i>Continuar para reserva';
        } else {
            btnSubmit.innerHTML = `<i class="bi bi-cart-plus me-2"></i>Faltam ${passengerCount - selectedSeats.length} lugar(es)`;
        }
    }

    // Hidden inputs
    if (selectedSeatsContainer) {
        selectedSeatsContainer.innerHTML = '';
        selectedSeats.forEach(seat => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'seatIds';
            input.value = seat.id;
            selectedSeatsContainer.appendChild(input);
        });
    }

    updateTotal();
}

function updateTotal() {
    const extraLuggageCheck = document.getElementById('extraLuggage');
    const mealCheck = document.getElementById('mealIncluded');
    const totalPriceDisplay = document.getElementById('totalPriceDisplay');
    const flightPriceDisplay = document.getElementById('flightPrice');

    const extraLuggage = extraLuggageCheck?.checked ? extraLuggageFee : 0;
    const mealIncluded = mealCheck?.checked ? mealFee : 0;

    const seatsTotal = selectedSeats.reduce((sum, s) => sum + s.price, 0);
    const flightTotal = baseFlightPrice * passengerCount;

    // Atualizar preço do voo no resumo
    if (flightPriceDisplay) {
        flightPriceDisplay.innerText = formatCurrency(flightTotal);
    }

    const total = flightTotal + seatsTotal + extraLuggage + mealIncluded;

    if (totalPriceDisplay) {
        totalPriceDisplay.innerText = formatCurrency(total);
    }
}

function formatCurrency(value) {
    return value.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' });
}

document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('seatForm');
    if (form) {
        form.addEventListener('submit', function (event) {
            if (selectedSeats.length !== passengerCount) {
                event.preventDefault();
                alert(`Por favor, selecione ${passengerCount} lugar(es) antes de continuar.`);
                return;
            }

            const button = document.getElementById('btnSubmit');
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> A processar...';
        });
    }

    // Eventos dos serviços adicionais
    const extraLuggageCheck = document.getElementById('extraLuggage');
    const mealCheck = document.getElementById('mealIncluded');

    if (extraLuggageCheck) extraLuggageCheck.addEventListener('change', updateTotal);
    if (mealCheck) mealCheck.addEventListener('change', updateTotal);
});