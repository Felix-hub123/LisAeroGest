let baseFlightPrice = 0;
let extraLuggageFee = 0;
let mealFee = 0;
let selectedSeatPrice = 0;

function initSeatSelection(flightPrice, luggageFee, mealPrice) {
    baseFlightPrice = flightPrice;
    extraLuggageFee = luggageFee;
    mealFee = mealPrice;
}

function selectSeat(button) {
    document.querySelectorAll('.seat-button').forEach(seat => seat.classList.remove('seat-selected'));
    button.classList.add('seat-selected');

    const seatId = button.getAttribute('data-seat-id');
    const seatCode = button.getAttribute('data-seat-code');
    const seatClass = button.getAttribute('data-seat-class');
    selectedSeatPrice = parseFloat(button.getAttribute('data-seat-price')) || 0;

    document.getElementById('selectedSeatId').value = seatId;
    document.getElementById('seatDisplay').innerText = seatCode;
    document.getElementById('seatClassDisplay').innerText = seatClass || 'Económica';
    document.getElementById('seatPrice').innerText = formatCurrency(selectedSeatPrice);

    document.getElementById('btnSubmit').disabled = false;
    updateTotal();
}

function updateTotal() {
    const extraLuggage = document.getElementById('extraLuggage').checked ? extraLuggageFee : 0;
    const mealIncluded = document.getElementById('mealIncluded').checked ? mealFee : 0;

    const total = baseFlightPrice + selectedSeatPrice + extraLuggage + mealIncluded;
    document.getElementById('totalPriceDisplay').innerText = formatCurrency(total);
}

function formatCurrency(value) {
    return value.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' });
}

document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('seatForm');
    if (form) {
        form.addEventListener('submit', function (event) {
            const seatId = document.getElementById('selectedSeatId').value;
            if (!seatId) {
                event.preventDefault();
                alert('Por favor, selecione um lugar antes de continuar.');
                return;
            }

            const button = document.getElementById('btnSubmit');
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> A processar...';
        });
    }
});