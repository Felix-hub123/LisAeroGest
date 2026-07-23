using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão de aeroportos.
    /// Operações restritas à role Admin — listagem, criação, edição e eliminação.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AirportController : Controller
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        /// <summary>
        /// Inicializa o AirportController com as dependências necessárias.
        /// </summary>
        /// <param name="airportRepository">Repositório para acesso aos dados dos aeroportos.</param>
        /// <param name="imageHelper">Helper para gestão e upload de imagens.</param>
        /// <param name="converterHelper">Helper para conversões entre entidades e ViewModels.</param>
        public AirportController(
            IAirportRepository airportRepository,
            IImageHelper imageHelper,
            IConverterHelper converterHelper)
        {
            _airportRepository = airportRepository;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Lista todos os aeroportos registados no sistema.
        /// </summary>
        /// <returns>View com a listagem de aeroportos.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _airportRepository.GetAllAsync());
        }

        /// <summary>
        /// Apresenta o formulário de criação de novo aeroporto.
        /// </summary>
        /// <returns>View com o formulário de criação.</returns>
        [HttpGet]
        public IActionResult Create() => View(new AirportViewModel());

        /// <summary>
        /// Processa o formulário de criação de novo aeroporto.
        /// </summary>
        /// <param name="model">ViewModel com os dados do novo aeroporto.</param>
        /// <returns>Redirecionamento para a Index em caso de sucesso, ou a View com erros de validação.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verifica código IATA duplicado
            var existing = await _airportRepository.GetByIATACodeAsync(model.IATACode!);
            if (existing != null)
            {
                ModelState.AddModelError("IATACode", "Já existe um aeroporto com este código IATA.");
                return View(model);
            }

            var imageId = Guid.Empty;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
                imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "airports");

            // Mapeamento delegado ao ConverterHelper
            var airport = _converterHelper.ToAirport(model, imageId);

            await _airportRepository.AddAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta o formulário de edição de um aeroporto existente.
        /// </summary>
        /// <param name="id">ID do aeroporto a editar.</param>
        /// <returns>View de edição ou NotFound se não for encontrado.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var airport = await _airportRepository.GetByIdAsync(id);
            if (airport == null)
                return NotFound();

            // Mapeamento delegado ao ConverterHelper
            var model = _converterHelper.ToAirportViewModel(airport);

            return View(model);
        }

        /// <summary>
        /// Processa o formulário de edição de um aeroporto existente.
        /// </summary>
        /// <param name="model">ViewModel com os dados atualizados do aeroporto.</param>
        /// <returns>Redirecionamento para a Index ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AirportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var airport = await _airportRepository.GetByIdAsync(model.Id);
            if (airport == null)
                return NotFound();

            // Verifica código IATA duplicado noutro aeroporto
            var existing = await _airportRepository.GetByIATACodeAsync(model.IATACode!);
            if (existing != null && existing.Id != model.Id)
            {
                ModelState.AddModelError("IATACode", "Já existe um aeroporto com este código IATA.");
                return View(model);
            }

            var imageId = airport.ImageId;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                if (airport.ImageId != Guid.Empty)
                    await _imageHelper.DeleteImageAsync(airport.ImageId, "airports");

                imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "airports");
            }

            // Atualização delegada ao ConverterHelper
            _converterHelper.UpdateAirportFromViewModel(airport, model, imageId);

            await _airportRepository.UpdateAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta a página de confirmação de eliminação de um aeroporto.
        /// </summary>
        /// <param name="id">ID do aeroporto a eliminar.</param>
        /// <returns>View de confirmação ou NotFound se não existir.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var airport = await _airportRepository.GetByIdAsync(id);
            if (airport == null)
                return NotFound();

            return View(airport);
        }

        /// <summary>
        /// Processa a eliminação lógica de um aeroporto.
        /// </summary>
        /// <param name="id">ID do aeroporto a eliminar.</param>
        /// <returns>Redirecionamento para a Index.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airport = await _airportRepository.GetByIdAsync(id);
            if (airport == null)
                return NotFound();

            // Impede eliminação se usado em voos
            var isUsed = await _airportRepository.IsUsedInFlightsAsync(id);
            if (isUsed)
            {
                TempData["Error"] = "Não é possível eliminar este aeroporto pois está associado a voos.";
                return RedirectToAction(nameof(Index));
            }

            await _airportRepository.DeleteAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta os detalhes de um aeroporto.
        /// </summary>
        /// <param name="id">ID do aeroporto a consultar.</param>
        /// <returns>View com detalhes do aeroporto ou NotFound se não existir.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var airport = await _airportRepository.GetByIdAsync(id);
            if (airport == null)
                return NotFound();

            return View(airport);
        }
    }
}