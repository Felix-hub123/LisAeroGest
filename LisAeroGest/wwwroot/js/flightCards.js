// ============================================
// LISAEROGEST - FLIGHT SEARCH MODULE
// ============================================

const FlightSearch = {
    config: {
        debounceDelay: 300,
        minSearchLength: 2,
        cacheKey: 'cachedFlights',
        searchHistoryKey: 'searchHistory'
    },

    state: {
        allFlights: [],
        filteredFlights: [],
        currentSearch: '',
        isSearching: false
    },

    init: function () {
        this.cacheDom();
        this.bindEvents();
        this.loadCachedFlights();
        this.setupSearchHistory();
    },

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

    // ==========================================
    // ⬇️ FUNÇÃO PRINCIPAL: Buscar voos da API
    // ==========================================
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

                // ⬇️ CHAMADA PARA PREENCHER A TABELA ⬇️
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

    // ==========================================
    // ⬇️ FUNÇÃO PARA PREENCHER A TABELA
    // ==========================================
    renderFlights: function (flights) {
        // PROCURAR A TABELA DE PARTIDAS
        var tbody = document.querySelector('#departures table tbody');

        // SE NÃO ENCONTRAR, TENTAR A PRIMEIRA TABELA DA PÁGINA
        if (!tbody) {
            var tables = document.querySelectorAll('table');
            if (tables.length > 0) {
                tbody = tables[0].querySelector('tbody');
            }
        }

        // SE NÃO ENCONTRAR NENHUMA TABELA
        if (!tbody) {
            console.warn('⚠️ Tabela não encontrada na página');
            return;
        }

        // SE NÃO HOUVER VOOS
        if (!flights || flights.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" class="text-center py-4 text-muted">
                        <i class="bi bi-airplane fs-3 d-block mb-2"></i>
                        Nenhum voo disponível
                    </td>
                </tr>
            `;
            return;
        }

        // PREENCHER A TABELA COM OS VOOS
        tbody.innerHTML = flights.map(function (f) {
            var badgeClass = 'bg-secondary';
            if (f.status === 'Partiu') badgeClass = 'bg-success';
            else if (f.status === 'Atrasado') badgeClass = 'bg-warning text-dark';
            else if (f.status === 'Cancelado') badgeClass = 'bg-danger';
            else if (f.status === 'Check-in') badgeClass = 'bg-info text-dark';
            else if (f.status === 'A Embarcar') badgeClass = 'bg-primary';

            return `<tr>
                <td><strong>${f.flightNumber}</strong></td>
                <td>${f.origin}</td>
                <td>${f.destinationCity}</td>
                <td>${f.departureTime}</td>
                <td><span class="badge ${badgeClass}">${f.status}</span></td>
                <td>${f.gate}</td>
            </tr>`;
        }).join('');

        // ATUALIZAR O CONTADOR "Partidas (0)"
        var tabLink = document.querySelector('[data-bs-target="#departures"]');
        if (tabLink) {
            var text = tabLink.textContent;
            tabLink.textContent = text.replace(/\(.*\)/, '(' + flights.length + ')');
        }

        console.log('✅ Tabela preenchida com', flights.length, 'voos!');
    },

    // ... resto das funções (getSuggestions, renderSuggestions, etc.) ...
    // Mantém todas as outras funções que já existem
};