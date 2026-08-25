using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão das companhias aéreas.
    /// Operações restritas à role Admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AirlineController : Controller
    {
        private readonly IAirlineRepository _airlineRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IFlightRepository _flightRepository;  // ← NOVO

        /// <summary>
        /// Inicializa uma nova instância do <see cref="AirlineController"/>.
        /// </summary>
        /// <param name="airlineRepository">Repositório para operações com companhias aéreas.</param>
        /// <param name="imageHelper">Helper para gestão do upload e remoção de imagens.</param>
        /// <param name="converterHelper">Helper para conversões entre entidades e ViewModels.</param>
        /// <param name="flightRepository">Repositório para operações com voos.</param>
        public AirlineController(
            IAirlineRepository airlineRepository,
            IImageHelper imageHelper,
            IConverterHelper converterHelper,
            IFlightRepository flightRepository)  // ← NOVO
        {
            _airlineRepository = airlineRepository;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
            _flightRepository = flightRepository;  // ← NOVO
        }

        // ════════════════════════════════════════════════════════════════════
        //  AÇÕES PÚBLICAS (ACESSÍVEIS A TODOS)  ← NOVAS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Lista todas as companhias aéreas que operam no aeroporto.
        /// Acesso livre a visitantes.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PublicIndex()
        {
            var airlines = await _airlineRepository.GetAllQueryable()
                .Where(a => !a.WasDeleted)
                .Include(a => a.Flights)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return View(airlines);
        }

        /// <summary>
        /// Mostra os detalhes de uma companhia aérea e os seus voos.
        /// Acesso livre a visitantes.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PublicDetails(int id)
        {
            var airline = await _airlineRepository.GetAllQueryable()
                .Include(a => a.Flights)
                    .ThenInclude(f => f.OriginAirport)
                .Include(a => a.Flights)
                    .ThenInclude(f => f.DestinationAirport)
                .Include(a => a.Flights)
                    .ThenInclude(f => f.Gate)
                .FirstOrDefaultAsync(a => a.Id == id && !a.WasDeleted);

            if (airline == null)
                return NotFound();

            // Filtra apenas voos não eliminados e futuros
            var futureFlights = airline.Flights?
                .Where(f => !f.WasDeleted && f.DepartureTime >= DateTime.Now)
                .OrderBy(f => f.DepartureTime)
                .Take(10)
                .ToList() ?? new List<Flight>();

            ViewBag.FutureFlights = futureFlights;
            ViewBag.TotalFlights = airline.Flights?.Count ?? 0;
            ViewBag.ActiveFlights = airline.Flights?
                .Count(f => !f.WasDeleted && f.DepartureTime >= DateTime.Now) ?? 0;

            return View(airline);
        }

        // ════════════════════════════════════════════════════════════════════
        //  AÇÕES DE GESTÃO (APENAS ADMIN)  ← JÁ EXISTENTES
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Apresenta a listagem geral das companhias aéreas registadas.
        /// </summary>
        /// <returns>View com a lista de companhias aéreas.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _airlineRepository.GetAllAsync());
        }

        /// <summary>
        /// Exibe o formulário de registo de uma nova companhia aérea preenchendo a lista de países.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Countries = _converterHelper.GetCountries();
            return View(new AirlineViewModel());
        }

        /// <summary>
        /// Endpoint JSON para obter as companhias pré-definidas associadas ao país selecionado.
        /// </summary>
        [HttpGet]
        public IActionResult GetAirlinesByCountry(string country)
        {
            var airlines = _converterHelper.GetAirlinesByCountry(country);
            return Json(airlines);
        }

        /// <summary>
        /// Processa a criação de uma nova companhia aérea, validando a unicidade do código IATA.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirlineViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Countries = _converterHelper.GetCountries();
                return View(model);
            }

            // Validação de duplicidade do código IATA
            var existing = await _airlineRepository.GetByIATACodeAsync(model.IATACode!.ToUpper());
            if (existing != null)
            {
                ModelState.AddModelError("IATACode", "Já existe uma companhia registada com este código IATA.");
                ViewBag.Countries = _converterHelper.GetCountries();
                return View(model);
            }

            var imageId = Guid.Empty;
            if (model.ImageFile != null)
                imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "airlines");

            // Mapeamento via ConverterHelper
            var airline = _converterHelper.ToAirline(model, imageId);

            await _airlineRepository.AddAsync(airline);
            await _airlineRepository.SaveAsync();

            TempData["Success"] = "Companhia aérea criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Exibe o formulário de edição de uma companhia aérea existente (HTTP GET).
        /// </summary>
        /// <param name="id">ID da companhia aérea a editar.</param>
        /// <returns>View de edição preenchida com os dados atuais ou NotFound se não existir.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var airline = await _airlineRepository.GetByIdAsync(id);
            if (airline == null)
            {
                return NotFound();
            }

            // Converte a entidade da BD para o ViewModel que a View espera
            var model = _converterHelper.ToAirlineViewModel(airline);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AirlineViewModel model)
        {
            var airline = await _airlineRepository.GetByIdAsync(model.Id);
            if (airline == null) return NotFound();

            var imageId = airline.ImageId;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                if (airline.ImageId != Guid.Empty)
                {
                    await _imageHelper.DeleteImageAsync(airline.ImageId, "airlines");
                }

                imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "airlines");
            }

            airline.ImageId = imageId;

            // ATUALIZA OS OUTROS CAMPOS QUE FALTAM
            airline.Name = model.Name;
            airline.IATACode = model.IATACode?.ToUpper();
            airline.Country = model.Country;

            await _airlineRepository.UpdateAsync(airline);
            await _airlineRepository.SaveAsync();

            TempData["Success"] = "Companhia aérea atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Exibe os detalhes de uma companhia aérea, incluindo os seus voos associados.
        /// </summary>
        /// <param name="id">ID da companhia aérea.</param>
        /// <returns>View com detalhes ou NotFound se não existir.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var airline = await _airlineRepository.GetWithFlightsAsync(id);
            if (airline == null) return NotFound();

            return View(airline);
        }

        /// <summary>
        /// Exibe a ecrã de confirmação de eliminação de uma companhia aérea.
        /// </summary>
        /// <param name="id">ID da companhia aérea a eliminar.</param>
        /// <returns>View de confirmação ou NotFound se não existir.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var airline = await _airlineRepository.GetByIdAsync(id);
            if (airline == null) return NotFound();

            return View(airline);
        }

        /// <summary>
        /// Processa a eliminação (Soft Delete) da companhia aérea.
        /// </summary>
        /// <param name="id">ID da companhia aérea.</param>
        /// <returns>Redirecionamento para a Index.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airline = await _airlineRepository.GetByIdAsync(id);
            if (airline == null) return NotFound();

            // 1. Valida no repositório se a companhia tem voos associados
            if (await _airlineRepository.IsUsedInFlightsAsync(id))
            {
                ModelState.AddModelError(string.Empty, "Não é possível eliminar esta companhia aérea pois existem voos associados a ela.");
                return View(airline);
            }

            // 2. Apaga fisicamente se não houver dependências
            await _airlineRepository.DeleteAsync(airline);
            await _airlineRepository.SaveAsync();

            TempData["Success"] = "Companhia aérea eliminada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}