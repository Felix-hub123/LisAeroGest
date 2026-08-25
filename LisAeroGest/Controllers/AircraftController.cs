using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controlador responsável pelo ciclo de vida e operações CRUD das aeronaves da frota.
    /// Gere a integração entre o repositório de dados, helpers de conversão/dropdowns e gestão de ficheiros de imagem.
    /// </summary>
    [Authorize(Roles = "Admin,Employee")]
    public class AircraftController : Controller
    {
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;

        /// <summary>
        /// Inicializa uma nova instância do controlador de aeronaves.
        /// </summary>
        /// <param name="aircraftRepository">Repositório de dados para persistência das aeronaves.</param>
        /// <param name="converterHelper">Helper para transformação de modelos, DTOs e listas de seleção.</param>
        /// <param name="imageHelper">Helper para upload e remoção de imagens físicas no servidor.</param>
        public AircraftController(
            IAircraftRepository aircraftRepository,
            IConverterHelper converterHelper,
            IImageHelper imageHelper)
        {
            _aircraftRepository = aircraftRepository;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
        }

        #region Leitura (Index & Details)

        /// <summary>
        /// GET: Aircraft
        /// Apresenta a listagem completa das aeronaves registadas no sistema.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var aircrafts = await _aircraftRepository.GetAllAsync();
            return View(aircrafts);
        }

        /// <summary>
        /// GET: Aircraft/Details/5
        /// Exibe a página de detalhes técnicos de uma aeronave específica.
        /// </summary>
        /// <param name="id">Identificador único da aeronave.</param>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var aircraft = await _aircraftRepository.GetByIdAsync(id.Value);
            if (aircraft == null) return NotFound();

            return View(aircraft);
        }

        #endregion

        #region Criação (Create)

        /// <summary>
        /// GET: Aircraft/Create
        /// Exibe o formulário de registo de uma nova aeronave com as listas de seleção inicializadas.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new AircraftViewModel
            {
                Brands = _converterHelper.GetAircraftBrands(),
                Models = _converterHelper.GetAircraftModels()
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: Aircraft/Create
        /// Valida os dados, processa o upload de imagem e persiste uma nova aeronave no repositório.
        /// </summary>
        /// <param name="viewModel">Dados do formulário de criação de aeronave.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repovoa os dropdowns encadeados mantendo os valores selecionados pelo utilizador
                viewModel.Brands = _converterHelper.GetAircraftBrands(viewModel.Brand);
                viewModel.Models = _converterHelper.GetAircraftModels(viewModel.Brand, viewModel.Model);
                return View(viewModel);
            }

            // Processa o upload da fotografia, caso tenha sido fornecida
            Guid imageId = Guid.Empty;
            if (viewModel.ImageFile != null)
            {
                imageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "aircraft");
            }

            var aircraft = _converterHelper.ToAircraft(viewModel, imageId);

            await _aircraftRepository.AddAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave registada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Edição (Edit)
        /// <summary>
        /// GET: Aircraft/Edit/5
        /// Carrega os dados operacionais da aeronave para edição.
        /// </summary>
        /// <param name="id">Identificador da aeronave a editar.</param>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var aircraft = await _aircraftRepository.GetByIdAsync(id.Value);
            if (aircraft == null) return NotFound();

            var viewModel = _converterHelper.ToAircraftViewModel(aircraft);

            return View(viewModel);
        }

        /// <summary>
        /// POST: Aircraft/Edit/5
        /// Atualiza as informações operacionais da aeronave e a sua imagem física.
        /// </summary>
        /// <param name="id">Identificador único da aeronave.</param>
        /// <param name="viewModel">Dados atualizados do formulário.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AircraftViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            // Como Brand e Model vêm desativados no HTML, removemos da validação do ModelState
            ModelState.Remove(nameof(viewModel.Brand));
            ModelState.Remove(nameof(viewModel.Model));

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var aircraft = await _aircraftRepository.GetByIdAsync(viewModel.Id);
            if (aircraft == null) return NotFound();

            Guid imageId = aircraft.ImageId;

            // Se uma nova imagem for carregada, apaga a anterior do disco e guarda a nova
            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                if (aircraft.ImageId != Guid.Empty)
                {
                    await _imageHelper.DeleteImageAsync(aircraft.ImageId, "aircraft");
                }

                imageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "aircraft");
            }

            // Atualiza apenas os campos operacionais
            _converterHelper.UpdateAircraftFromViewModel(aircraft, viewModel, imageId);

            await _aircraftRepository.UpdateAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Remoção (Delete)

        /// <summary>
        /// GET: Aircraft/Delete/5
        /// Apresenta a ecrã de confirmação de remoção do registo da aeronave.
        /// </summary>
        /// <param name="id">Identificador da aeronave a remover.</param>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var aircraft = await _aircraftRepository.GetByIdAsync(id.Value);
            if (aircraft == null) return NotFound();

            return View(aircraft);
        }

        /// <summary>
        /// POST: Aircraft/Delete/5
        /// Executa a remoção definitiva da aeronave na base de dados e limpa os ficheiros multimédia associados.
        /// </summary>
        /// <param name="id">Identificador da aeronave a ser eliminada.</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(id);
            if (aircraft == null) return NotFound();

          
            var isUsed = await _aircraftRepository.IsUsedInFlightsAsync(id);
            if (isUsed)
            {
                TempData["Error"] = "Não é possível eliminar esta aeronave: existem voos associados a ela.";
                return RedirectToAction(nameof(Index));
            }


            if (aircraft.ImageId != Guid.Empty)
            {
                await _imageHelper.DeleteImageAsync(aircraft.ImageId, "aircraft");
            }

            await _aircraftRepository.DeleteAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave eliminada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Endpoints de Apoio (AJAX / API)

        /// <summary>
        /// GET: Aircraft/GetModelsByBrand?brand=Airbus
        /// Endpoint assíncrono para suporte a dropdowns encadeados em cliente (AJAX/Fetch API).
        /// </summary>
        /// <param name="brand">Nome do fabricante para filtragem de modelos.</param>
        /// <returns>Lista em formato JSON com os modelos correspondentes à marca selecionada.</returns>
        [HttpGet]
        public IActionResult GetModelsByBrand(string brand)
        {
            var models = _converterHelper.GetAircraftModels(brand);
            return Json(models);
        }

        #endregion
    }

}

