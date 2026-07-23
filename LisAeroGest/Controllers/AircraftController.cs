using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão da frota de aeronaves.
    /// Operações restritas à role Admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AircraftController : Controller
    {
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        /// <summary>
        /// Inicializa o AircraftController com as dependências necessárias.
        /// </summary>
        /// <param name="aircraftRepository">Repositório para acesso aos dados das aeronaves.</param>
        /// <param name="imageHelper">Helper para gestão de imagens (Local/Cloud).</param>
        /// <param name="converterHelper">Helper para conversão entre ViewModels e Entidades.</param>
        public AircraftController(
            IAircraftRepository aircraftRepository,
            IImageHelper imageHelper,
            IConverterHelper converterHelper)
        {
            _aircraftRepository = aircraftRepository;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Lista todas as aeronaves registadas na frota.
        /// </summary>
        /// <returns>View com a listagem de aeronaves.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _aircraftRepository.GetAllAsync());
        }

        /// <summary>
        /// Apresenta o formulário para adicionar uma nova aeronave.
        /// </summary>
        /// <returns>View com o formulário de criação.</returns>
        [HttpGet]
        public IActionResult Create() => View(new AircraftViewModel());

        /// <summary>
        /// Processa a criação de uma nova aeronave.
        /// Faz o upload da imagem se fornecida e guarda os dados na base de dados.
        /// </summary>
        /// <param name="viewModel">Modelo com os dados da nova aeronave.</param>
        /// <returns>Redirecionamento para a Index em caso de sucesso, ou a própria View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var imageId = Guid.Empty;
            if (viewModel.ImageFile != null)
                imageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "aircraft");

            // Mapeamento delegado ao ConverterHelper
            var aircraft = _converterHelper.ToAircraft(viewModel, imageId);

            await _aircraftRepository.AddAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave adicionada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta o formulário de edição para uma aeronave existente.
        /// </summary>
        /// <param name="id">ID da aeronave a editar.</param>
        /// <returns>View com os dados da aeronave ou NotFound se não existir.</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(id);
            if (aircraft == null) return NotFound();

            // Mapeamento delegado ao ConverterHelper
            var model = _converterHelper.ToAircraftViewModel(aircraft);

            return View(model);
        }

        /// <summary>
        /// Processa a atualização dos dados de uma aeronave.
        /// Gere a substituição da imagem se um novo ficheiro for enviado.
        /// </summary>
        /// <param name="viewModel">Modelo com os dados atualizados.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AircraftViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            var aircraft = await _aircraftRepository.GetByIdAsync(viewModel.Id);
            if (aircraft == null) return NotFound();

            var imageId = aircraft.ImageId;
            if (viewModel.ImageFile != null)
            {
                await _imageHelper.DeleteImageAsync(aircraft.ImageId, "aircraft");
                imageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "aircraft");
            }

            // Delegamos a atribuição de propriedades ao ConverterHelper
            _converterHelper.UpdateAircraftFromViewModel(aircraft, viewModel, imageId);

            await _aircraftRepository.UpdateAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta os detalhes de uma aeronave específica, incluindo os seus lugares.
        /// </summary>
        /// <param name="id">ID da aeronave.</param>
        /// <returns>View com os detalhes da aeronave.</returns>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var aircraft = await _aircraftRepository.GetWithSeatsAsync(id);
            if (aircraft == null) return NotFound();

            return View(aircraft);
        }

        /// <summary>
        /// Apresenta a página de confirmação de eliminação de uma aeronave.
        /// </summary>
        /// <param name="id">ID da aeronave a eliminar.</param>
        /// <returns>View de confirmação.</returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(id);
            if (aircraft == null) return NotFound();

            return View(aircraft);
        }

        /// <summary>
        /// Processa a eliminação da aeronave após confirmação se não estiver associada a voos.
        /// </summary>
        /// <param name="id">ID da aeronave a eliminar.</param>
        /// <returns>Redirecionamento para a Index.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isUsed = await _aircraftRepository.IsUsedInFlightsAsync(id);
            if (isUsed)
            {
                TempData["Error"] = "Não é possível eliminar esta aeronave pois está associada a voos.";
                return RedirectToAction(nameof(Index));
            }

            var aircraft = await _aircraftRepository.GetByIdAsync(id);
            if (aircraft == null) return NotFound();

            await _aircraftRepository.DeleteAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave removida da frota com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
