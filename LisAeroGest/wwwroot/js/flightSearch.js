// ============================================
// ============================================
// LISAEROGEST - FLIGHT SEARCH MODULE
// ============================================

const FlightSearch = {
    // Configuração
    config: {
        debounceDelay: 300,
        minSearchLength: 2,
        cacheKey: 'cachedFlights',
        searchHistoryKey: 'searchHistory'
    },

    // Estado
    state: {
        allFlights: [],
        filteredFlights: [],
        currentSearch: '',
        isSearching: false
    },

    // Inicialização
    init: function () {
        this.cacheDom();
        this.bindEvents();
        this.loadCachedFlights();
        this.setupSearchHistory();
    },

    // Cache de elementos DOM
    cacheDom: function () {
        this.dom = {
            searchInput: document.getElementById('searchInput'),
            suggestions: document.getElementById('searchSuggestions'),
            flightDate: document.getElementById('flightDate'),
            statusFilter: document.getElementById('statusFilter'),
            originInput: document.getElementById('originInput'),
            filterChips: document.querySelectorAll('.filter-chip')
        };
    },

    // Bind de eventos
    bindEvents: function () {
        let debounceTimer;
        this.dom.searchInput.addEventListener('input', (e) => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                this.handleSearch(e.target.value);
            }, this.config.debounceDelay);
        });

        this.dom.searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.performSearch();
                this.dom.suggestions.classList.add('d-none');
            }
        });

        this.dom.flightDate.addEventListener('change', () => this.performSearch());
        this.dom.statusFilter.addEventListener('change', () => this.performSearch());

        this.dom.filterChips.forEach(chip => {
            chip.addEventListener('click', () => {
                const filter = chip.dataset.filter;
                this.dom.statusFilter.value = filter;
                this.performSearch();
                this.highlightActiveChip(chip);
            });
        });

        document.addEventListener('click', (e) => {
            if (!this.dom.searchInput.contains(e.target) && !this.dom.suggestions.contains(e.target)) {
                this.dom.suggestions.classList.add('d-none');
            }
        });
    },

    // Carregar voos do cache ou da API
    loadCachedFlights: function () {
        const cached = localStorage.getItem(this.config.cacheKey);
        if (cached) {
            try {
                this.state.allFlights = JSON.parse(cached);
                this.renderFlights(this.state.allFlights);
            } catch (e) {
                this.fetchFlightsFromApi();
            }
        } else {
            this.fetchFlightsFromApi();
        }
    },

    // Buscar voos da API pública
    fetchFlightsFromApi: function () {
        fetch('/api/flights/departures')
            .then(response => {
                if (!response.ok) throw new Error('API indisponível');
                return response.json();
            })
            .then(data => {
                console.log('🔹 Dados recebidos da API:', data.length);

                var flights = data.map(function (f) {
                    var time = '--:--';
                    if (f.departureTime) {
                        var date = new Date(f.departureTime);
                        if (!isNaN(date)) {
                            time = date.toLocaleTimeString('pt-PT', { hour: '2-digit', minute: '2-digit' });
                        }
                    }

                    var statusMap = {
                        'Scheduled': 'Previsto',
                        'CheckIn': 'Check-in',
                        'Boarding': 'A Embarcar',
                        'Departed': 'Partiu',
                        'Delayed': 'Atrasado',
                        'Cancelled': 'Cancelado'
                    };
                    var statusPt = statusMap[f.status] || f.status || 'Previsto';

                    return {
                        id: f.id,
                        flightNumber: f.flightNumber || 'N/A',
                        origin: f.originCode || f.origin || '???',
                        destination: f.destinationCode || f.destination || '???',
                        destinationCity: f.destination || 'Desconhecida',
                        departureTime: time,
                        status: statusPt,
                        airline: f.airlineName || 'Desconhecida',
                        gate: f.gate || '—',
                        terminal: '1'
                    };
                });

                this.state.allFlights = flights;
                localStorage.setItem(this.config.cacheKey, JSON.stringify(flights));
                this.renderFlights(flights);
                console.log('✅ Voos guardados no localStorage:', flights.length);
                document.dispatchEvent(new Event('flightsUpdated'));
            })
            .catch(function (error) {
                console.warn('⚠️ API indisponível, usando dados de exemplo:', error);
                var mockFlights = this.getMockFlights();
                this.state.allFlights = mockFlights;
                localStorage.setItem(this.config.cacheKey, JSON.stringify(mockFlights));
                this.renderFlights(mockFlights);
            }.bind(this));
    },

    // Dados de exemplo (fallback)
    getMockFlights: function () {
        return [
            { id: 1, flightNumber: 'TP1234', origin: 'LIS', destination: 'CDG', destinationCity: 'Paris', departureTime: '08:30', status: 'Previsto', airline: 'TAP Portugal', gate: 'B14', terminal: '1' },
            { id: 2, flightNumber: 'FR5678', origin: 'LIS', destination: 'LGW', destinationCity: 'Londres', departureTime: '10:15', status: 'Em Hora', airline: 'Ryanair', gate: 'A22', terminal: '2' },
            { id: 3, flightNumber: 'BA8765', origin: 'LIS', destination: 'LHR', destinationCity: 'Londres', departureTime: '12:45', status: 'Check-in', airline: 'British Airways', gate: 'B07', terminal: '1' },
            { id: 4, flightNumber: 'AF2345', origin: 'LIS', destination: 'ORY', destinationCity: 'Paris', departureTime: '14:20', status: 'A Embarcar', airline: 'Air France', gate: 'A11', terminal: '2' },
            { id: 5, flightNumber: 'U28654', origin: 'LIS', destination: 'FCO', destinationCity: 'Roma', departureTime: '16:50', status: 'Partiu', airline: 'EasyJet', gate: 'B03', terminal: '1' },
            { id: 6, flightNumber: 'LH1234', origin: 'LIS', destination: 'FRA', destinationCity: 'Frankfurt', departureTime: '18:10', status: 'Atrasado', airline: 'Lufthansa', gate: 'A19', terminal: '2' },
            { id: 7, flightNumber: 'TK9876', origin: 'LIS', destination: 'IST', destinationCity: 'Istambul', departureTime: '20:30', status: 'Cancelado', airline: 'Turkish Airlines', gate: 'B21', terminal: '1' }
        ];
    },

    // Pesquisa principal
    handleSearch: function (query) {
        this.state.currentSearch = query;

        if (query.length < this.config.minSearchLength) {
            this.dom.suggestions.classList.add('d-none');
            this.performSearch();
            return;
        }

        this.dom.suggestions.classList.remove('d-none');
        const suggestions = this.getSuggestions(query);
        this.renderSuggestions(suggestions, query);
    },

    // Obter sugestões
    getSuggestions: function (query) {
        const lowerQuery = query.toLowerCase().trim();
        var allFlights = JSON.parse(localStorage.getItem(this.config.cacheKey) || '[]');
        if (allFlights.length === 0) {
            allFlights = this.state.allFlights;
        }
        return allFlights
            .filter(function (flight) {
                var flightNumber = (flight.flightNumber || '').toLowerCase();
                var destination = (flight.destinationCity || flight.destination || '').toLowerCase();
                var airline = (flight.airline || '').toLowerCase();
                var origin = (flight.origin || '').toLowerCase();
                return flightNumber.includes(lowerQuery) ||
                    destination.includes(lowerQuery) ||
                    airline.includes(lowerQuery) ||
                    origin.includes(lowerQuery);
            })
            .slice(0, 8);
    },

    // Renderizar sugestões
    renderSuggestions: function (suggestions, query) {
        const container = this.dom.suggestions;
        if (suggestions.length === 0) {
            container.innerHTML = `
                <div class="p-3 text-muted text-center">
                    <i class="bi bi-search me-2"></i> Nenhum voo encontrado para "${query}"
                </div>
            `;
            container.classList.remove('d-none');
            return;
        }

        container.innerHTML = suggestions.map(function (flight) {
            return `
                <div class="suggestion-item d-flex justify-content-between align-items-center p-2 border-bottom" 
                     onclick="FlightSearch.selectSuggestion('${flight.flightNumber}')">
                    <div>
                        <span class="flight-code">${flight.flightNumber}</span>
                        <span class="destination-name">${flight.destinationCity || flight.destination}</span>
                        <span class="time-info ms-2">${flight.departureTime}</span>
                    </div>
                    <div>
                        <span class="badge ${FlightSearch.getStatusBadgeClass(flight.status)}">${flight.status}</span>
                    </div>
                </div>
            `;
        }).join('');
    },

    // Selecionar sugestão
    selectSuggestion: function (flightNumber) {
        this.dom.searchInput.value = flightNumber;
        this.dom.suggestions.classList.add('d-none');
        this.performSearch();
        this.addToSearchHistory(flightNumber);
    },

    // ==========================================================
    // ⬇️ FUNÇÃO PRINCIPAL: Pesquisar em TODOS os voos
    // ==========================================================
    performSearch: function () {
        var searchTerm = this.dom.searchInput.value.toLowerCase().trim();
        var selectedDate = this.dom.flightDate.value;
        var selectedStatus = this.dom.statusFilter.value;

        // 🔧 BUSCAR TODOS OS VOOS do localStorage (não apenas os visíveis)
        var allFlights = JSON.parse(localStorage.getItem(this.config.cacheKey) || '[]');

        // Se não houver voos no localStorage, usar os que estão na memória
        if (allFlights.length === 0) {
            allFlights = this.state.allFlights;
        }

        var filtered = allFlights;

        // Filtro por termo de pesquisa
        if (searchTerm) {
            filtered = filtered.filter(function (flight) {
                var flightNumber = (flight.flightNumber || '').toLowerCase();
                var destination = (flight.destinationCity || flight.destination || '').toLowerCase();
                var airline = (flight.airline || '').toLowerCase();
                var origin = (flight.origin || '').toLowerCase();

                return flightNumber.includes(searchTerm) ||
                    destination.includes(searchTerm) ||
                    airline.includes(searchTerm) ||
                    origin.includes(searchTerm);
            });
        }

        // Filtro por data
        if (selectedDate) {
            filtered = filtered.filter(function (flight) {
                if (flight.date) {
                    return flight.date === selectedDate;
                }
                return true;
            });
        }

        // Filtro por status
        if (selectedStatus) {
            filtered = filtered.filter(function (flight) {
                return flight.status === selectedStatus;
            });
        }

        // Guardar resultados filtrados
        this.state.filteredFlights = filtered;

        // Renderizar a tabela com os resultados
        this.renderFlights(filtered);

        // Atualizar o contador
        this.updateSearchCount(filtered.length);

        console.log('🔍 Pesquisa realizada. Encontrados:', filtered.length, 'voos');
    },

    // ==========================================================
    // Renderizar voos na tabela
    // ==========================================================
    renderFlights: function (flights) {
        var tbody = document.querySelector('#departures table tbody');
        if (!tbody) {
            var tables = document.querySelectorAll('table');
            if (tables.length > 0) {
                tbody = tables[0].querySelector('tbody');
            }
        }

        if (!tbody) {
            console.warn('⚠️ Tabela não encontrada');
            return;
        }

        // 🔧 VERIFICAR SE HÁ TERMO DE PESQUISA
        var searchInput = document.getElementById('searchInput');
        var hasSearchTerm = searchInput && searchInput.value.trim().length > 0;
        var hasFilters = this.dom.statusFilter && this.dom.statusFilter.value !== '';

        // SE NÃO HOUVER PESQUISA E NÃO HOUVER FILTROS → MOSTRAR MENSAGEM
        if (!hasSearchTerm && !hasFilters) {
            tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-5">
                    <i class="bi bi-search fs-1 d-block mb-3 text-muted"></i>
                    <h5 class="fw-bold text-muted">Pesquise por um voo</h5>
                    <p class="text-muted small">Digite o número do voo, destino ou companhia aérea</p>
                </td>
            </tr>
        `;
            return;
        }

        // SE NÃO HOUVER RESULTADOS
        if (!flights || flights.length === 0) {
            tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-4 text-muted">
                    <i class="bi bi-airplane fs-3 d-block mb-2"></i>
                    Nenhum voo encontrado para a sua pesquisa
                </td>
            </tr>
        `;
            return;
        }

        // BUSCAR FAVORITOS
        var favorites = JSON.parse(localStorage.getItem('favoriteFlights') || '[]');

        // PREENCHER A TABELA
        tbody.innerHTML = flights.map(function (f) {
            var badgeClass = 'bg-secondary';
            if (f.status === 'Partiu') badgeClass = 'bg-success';
            else if (f.status === 'Atrasado') badgeClass = 'bg-warning text-dark';
            else if (f.status === 'Cancelado') badgeClass = 'bg-danger';
            else if (f.status === 'Check-in') badgeClass = 'bg-info text-dark';
            else if (f.status === 'A Embarcar') badgeClass = 'bg-primary';

            var isFavorite = favorites.includes(String(f.id));
            var starClass = isFavorite ? 'bi bi-star-fill text-warning' : 'bi bi-star';

            return `<tr>
            <td>
                <button class="btn btn-sm btn-outline-secondary favorite-btn ${isFavorite ? 'active' : ''}" 
                        data-flight-id="${f.id}"
                        onclick="FlightCards.toggleFavorite('${f.id}')">
                    <i class="${starClass}"></i>
                </button>
            </td>
            <td><strong>${f.flightNumber}</strong></td>
            <td>${f.origin}</td>
            <td>${f.destinationCity}</td>
            <td>${f.departureTime}</td>
            <td>${f.gate}</td>
            <td><span class="badge ${badgeClass}">${f.status}</span></td>
        </tr>`;
        }).join('');

        // ATUALIZAR CONTADOR
        var tabLink = document.querySelector('[data-bs-target="#departures"]');
        if (tabLink) {
            var text = tabLink.textContent;
            tabLink.textContent = text.replace(/\(.*\)/, '(' + flights.length + ')');
        }
    },

    // Atualizar contador de resultados
    updateSearchCount: function (count) {
        var tabPanes = document.querySelectorAll('.tab-pane');
        tabPanes.forEach(function (pane) {
            var tabId = pane.id;
            var tabButton = document.querySelector('[data-bs-target="#' + tabId + '"]');
            if (tabButton) {
                var currentText = tabButton.textContent;
                var cleanedText = currentText.replace(/\(.*\)/, '').trim();
                tabButton.textContent = cleanedText + ' (' + count + ')';
            }
        });
    },

    // Obter classe CSS para badge de status
    getStatusBadgeClass: function (status) {
        var map = {
            'Previsto': 'bg-secondary',
            'Check-in': 'bg-info',
            'A Embarcar': 'bg-primary',
            'Partiu': 'bg-success',
            'Atrasado': 'bg-warning text-dark',
            'Cancelado': 'bg-danger',
            'Em Hora': 'bg-success'
        };
        return map[status] || 'bg-secondary';
    },

    // Destacar chip ativo
    highlightActiveChip: function (activeChip) {
        document.querySelectorAll('.filter-chip').forEach(function (chip) {
            chip.classList.remove('bg-primary', 'text-white');
            chip.classList.add('bg-light', 'text-dark');
        });
        if (activeChip) {
            activeChip.classList.remove('bg-light', 'text-dark');
            activeChip.classList.add('bg-primary', 'text-white');
        }
    },

    // Limpar filtros
    clearFilters: function () {
        this.dom.searchInput.value = '';
        this.dom.statusFilter.value = '';
        var today = new Date().toISOString().split('T')[0];
        this.dom.flightDate.value = today;
        document.querySelectorAll('.filter-chip').forEach(function (chip) {
            chip.classList.remove('bg-primary', 'text-white');
            chip.classList.add('bg-light', 'text-dark');
        });
        // 🔧 RECARREGAR A TABELA VAZIA
        this.renderFlights([]);

        // Restaurar contador para 0
        var tabLink = document.querySelector('[data-bs-target="#departures"]');
        if (tabLink) {
            var text = tabLink.textContent;
            var cleanedText = text.replace(/\(.*\)/, '').trim();
            tabLink.textContent = cleanedText + ' (0)';
        }
    },

    // Histórico de pesquisas
    addToSearchHistory: function (term) {
        var history = JSON.parse(localStorage.getItem(this.config.searchHistoryKey) || '[]');
        history = history.filter(function (item) { return item !== term; });
        history.unshift(term);
        if (history.length > 10) history.pop();
        localStorage.setItem(this.config.searchHistoryKey, JSON.stringify(history));
    },

    setupSearchHistory: function () {
        var history = JSON.parse(localStorage.getItem(this.config.searchHistoryKey) || '[]');
        if (history.length > 0) {
            this.dom.searchInput.addEventListener('focus', function () {
                if (!this.dom.searchInput.value && history.length > 0) {
                    this.dom.suggestions.classList.remove('d-none');
                    this.dom.suggestions.innerHTML = history.map(function (term) {
                        return `
                            <div class="suggestion-item p-2 border-bottom" onclick="FlightSearch.selectSuggestion('${term}')">
                                <i class="bi bi-clock-history me-2 text-muted"></i> ${term}
                            </div>
                        `;
                    }).join('');
                }
            }.bind(this));
        }
    }
};

// Inicializar quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    if (typeof FlightSearch !== 'undefined' && FlightSearch.init) {
        FlightSearch.init();
    }
});