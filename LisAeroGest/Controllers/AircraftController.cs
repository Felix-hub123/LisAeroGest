using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
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

        /// <summary>
        /// Inicializa o AircraftController com as dependências necessárias.
        /// </summary>
        /// <param name="aircraftRepository">Repositório para acesso aos dados das aeronaves.</param>
        /// <param name="imageHelper">Helper para gestão de imagens (Local/Cloud).</param>
        public AircraftController(IAircraftRepository aircraftRepository, IImageHelper imageHelper)
        {
            _aircraftRepository = aircraftRepository;
            _imageHelper = imageHelper;
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
        /// <param name="model">Modelo com os dados da nova aeronave.</param>
        /// <returns>Redirecionamento para a Index em caso de sucesso, ou a própria View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AircraftViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var imageId = Guid.Empty;
            if (model.ImageFile != null)
                imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "aircraft");

            var aircraft = new Aircraft
            {
                Brand = model.Brand,
                Model = model.Model,
                EconomySeats = model.EconomySeats,
                BusinessSeats = model.BusinessSeats,
                IsAvailable = model.IsAvailable,
                ImageId = imageId
            };

            await _aircraftRepository.AddAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave adicionada à frota com sucesso!";
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

            var model = new AircraftViewModel
            {
                Id = aircraft.Id,
                Brand = aircraft.Brand,
                Model = aircraft.Model,
                EconomySeats = aircraft.EconomySeats,
                BusinessSeats = aircraft.BusinessSeats,
                IsAvailable = aircraft.IsAvailable,
                ImageId = aircraft.ImageId
            };

            return View(model);
        }

        /// <summary>
        /// Processa a atualização dos dados de uma aeronave.
        /// Gere a substituição da imagem se um novo ficheiro for enviado.
        /// </summary>
        /// <param name="model">Modelo com os dados atualizados.</param>
        /// <returns>Redirecionamento para a Index ou View com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AircraftViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var aircraft = await _aircraftRepository.GetByIdAsync(model.Id);
            if (aircraft == null) return NotFound();

            if (model.ImageFile != null)
            {
                // Elimina a imagem antiga antes de guardar a nova
                await _imageHelper.DeleteImageAsync(aircraft.ImageId, "aircraft");
                aircraft.ImageId = await _imageHelper.UploadImageAsync(model.ImageFile, "aircraft");
            }

            aircraft.Brand = model.Brand;
            aircraft.Model = model.Model;
            aircraft.EconomySeats = model.EconomySeats;
            aircraft.BusinessSeats = model.BusinessSeats;
            aircraft.IsAvailable = model.IsAvailable;

            await _aircraftRepository.UpdateAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Dados da aeronave atualizados com sucesso!";
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
        /// Processa a eliminação (Soft Delete) da aeronave após confirmação.
        /// </summary>
        /// <param name="id">ID da aeronave a eliminar.</param>
        /// <returns>Redirecionamento para a Index.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aircraft = await _aircraftRepository.GetByIdAsync(id);
            if (aircraft == null) return NotFound();

            await _aircraftRepository.DeleteAsync(aircraft);
            await _aircraftRepository.SaveAsync();

            TempData["Success"] = "Aeronave removida da frota com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
