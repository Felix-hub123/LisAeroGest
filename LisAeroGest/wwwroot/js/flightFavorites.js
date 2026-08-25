// ============================================
// LISAEROGEST - FLIGHT FAVORITES MODULE
// ============================================

const FlightFavorites = {
    // Configuração
    config: {
        favoritesKey: 'favoriteFlights',
        cacheKey: 'cachedFlights'
    },

    // Estado
    state: {
        favorites: JSON.parse(localStorage.getItem('favoriteFlights') || '[]'),
        allFlights: JSON.parse(localStorage.getItem('cachedFlights') || '[]')
    },

    // Inicialização
    init: function () {
        this.render();
        this.bindEvents();
    },

    // Bind de eventos
    bindEvents: function () {
        // Ouvir alterações nos favoritos
        window.addEventListener('storage', (e) => {
            if (e.key === this.config.favoritesKey) {
                this.state.favorites = JSON.parse(e.newValue || '[]');
                this.render();
            }
        });

        // Atualizar quando a lista de voos mudar
        document.addEventListener('flightsUpdated', () => {
            this.state.allFlights = JSON.parse(localStorage.getItem(this.config.cacheKey) || '[]');
            this.render();
        });
    },

    // Renderizar secção de favoritos
    render: function () {
        const section = document.getElementById('favoritesSection');
        const container = document.getElementById('favoritesContainer');

        if (!section || !container) return;

        // Se não houver favoritos, esconder secção
        if (this.state.favorites.length === 0) {
            section.classList.add('d-none');
            return;
        }

        // Mostrar secção
        section.classList.remove('d-none');

        // Buscar dados dos voos favoritos
        const favoriteFlights = this.state.allFlights.filter(f =>
            this.state.favorites.includes(String(f.id)) ||
            this.state.favorites.includes(f.id)
        );

        if (favoriteFlights.length === 0) {
            container.innerHTML = `
                <div class="col-12 text-center py-4 text-muted">
                    <i class="bi bi-star fs-2 d-block mb-2"></i>
                    Os teus voos favoritos ainda não estão disponíveis.
                    <br><small>Tenta recarregar a página ou pesquisa novamente.</small>
                </div>
            `;
            return;
        }

        // Renderizar cartões
        container.innerHTML = favoriteFlights.map(flight => `
            <div class="flight-card" data-flight-id="${flight.id}" data-flight-number="${flight.flightNumber}">
                <div class="flight-card-inner">
                    <div class="flight-card-header d-flex justify-content-between align-items-start">
                        <div>
                            <span class="flight-number">${flight.flightNumber}</span>
                            <span class="flight-airline text-muted ms-2">${flight.airline || ''}</span>
                        </div>
                        <span class="flight-status-badge ${this.getStatusClass(flight.status)}">
                            <span class="status-dot"></span>
                            ${flight.status || 'Desconhecido'}
                        </span>
                    </div>
                    <div class="flight-card-body">
                        <div class="flight-route d-flex align-items-center">
                            <div class="route-origin">
                                <span class="route-code">${flight.origin || '???'}</span>
                                <span class="route-city d-block small text-muted">${flight.originCity || ''}</span>
                            </div>
                            <div class="route-arrow mx-3">
                                <i class="bi bi-arrow-right"></i>
                            </div>
                            <div class="route-destination">
                                <span class="route-code">${flight.destination || '???'}</span>
                                <span class="route-city d-block small text-muted">${flight.destinationCity || flight.destination || ''}</span>
                            </div>
                        </div>
                        <div class="flight-times d-flex justify-content-between mt-2">
                            <div>
                                <small class="text-muted d-block">Previsto</small>
                                <span class="scheduled-time">${flight.departureTime || '—'}</span>
                            </div>
                            <div class="text-end">
                                <small class="text-muted d-block">Estado</small>
                                <span class="fw-bold">${flight.status || '—'}</span>
                            </div>
                        </div>
                    </div>
                    <div class="flight-card-footer d-flex justify-content-between align-items-center">
                        <div>
                            <span class="gate-info">
                                <i class="bi bi-door-open me-1"></i> Gate ${flight.gate || '—'}
                            </span>
                        </div>
                        <div class="flight-actions">
                            <button class="btn btn-sm btn-outline-secondary favorite-btn active" 
                                    data-flight-id="${flight.id}"
                                    onclick="FlightCards.toggleFavorite('${flight.id}')">
                                <i class="bi bi-star-fill text-warning"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-primary detail-btn" 
                                    data-flight-id="${flight.id}"
                                    onclick="FlightCards.openDetail('${flight.id}')">
                                <i class="bi bi-info-circle"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-success share-btn" 
                                    data-flight-id="${flight.id}"
                                    onclick="FlightCards.shareFlight('${flight.id}')">
                                <i class="bi bi-share"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');
    },

    // Obter classe CSS para status
    getStatusClass: function (status) {
        const map = {
            'Previsto': 'status-previsto',
            'Check-in': 'status-check-in',
            'A Embarcar': 'status-a-embarcar',
            'Partiu': 'status-partiu',
            'Atrasado': 'status-atrasado',
            'Cancelado': 'status-cancelado',
            'Em Hora': 'status-em-hora'
        };
        return map[status] || 'status-previsto';
    },

    // Verificar se um voo é favorito
    isFavorite: function (flightId) {
        return this.state.favorites.includes(String(flightId));
    },

    // Obter lista de voos favoritos
    getFavorites: function () {
        return this.state.allFlights.filter(f =>
            this.state.favorites.includes(String(f.id))
        );
    },

    // Atualizar estado (chamar quando a lista de voos mudar)
    refresh: function () {
        this.state.allFlights = JSON.parse(localStorage.getItem(this.config.cacheKey) || '[]');
        this.render();
    }
};

// Inicializar quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    if (typeof FlightFavorites !== 'undefined' && FlightFavorites.init) {
        FlightFavorites.init();
    }
});