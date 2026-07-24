using System;
using System.Threading.Tasks;
using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão de passageiros.
    /// Admin gere todos; Passageiro cria/editar o próprio perfil.
    /// </summary>
    [Authorize]
    public class PassengerController : Controller
    {
        private readonly IPassengerRepository _passengerRepository;
        private readonly UserManager<User> _userManager;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        public PassengerController(
            IPassengerRepository passengerRepository,
            UserManager<User> userManager,
            IImageHelper imageHelper,
            IConverterHelper converterHelper)
        {
            _passengerRepository = passengerRepository;
            _userManager = userManager;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
        }

        // ─── INDEX ──────────────────────────────────────────────────────────

        /// <summary>
        /// Lista todos os passageiros (Admin) ou redireciona para o próprio perfil.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var all = await _passengerRepository.GetAllAsync();
                return View(all);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var passenger = await _passengerRepository.GetByUserIdAsync(user.Id);
            if (passenger == null) return RedirectToAction(nameof(Create));

            return RedirectToAction(nameof(Details), new { id = passenger.Id });
        }

        // ─── DETAILS ────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta os detalhes de um passageiro.
        /// Admin/Employee vê qualquer um; Passageiro só vê o próprio.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var passenger = await _passengerRepository.GetWithTicketsAndFlightsAsync(id);
            if (passenger == null) return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || passenger.UserId != user.Id) return Forbid();
            }

            return View(passenger);
        }

        // ─── CREATE ─────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de criação do perfil de passageiro.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Se já tem perfil, redireciona para editar
            var existing = await _passengerRepository.GetByUserIdAsync(user.Id);
            if (existing != null && !User.IsInRole("Admin"))
                return RedirectToAction(nameof(Edit), new { id = existing.Id });

            var vm = new PassengerViewModel
            {
                UserId = user.Id,
                UserEmail = user.Email,
                DocumentTypes = _converterHelper.GetDocumentTypes()
            };

            return View(vm);
        }

        /// <summary>
        /// Processa a criação do perfil de passageiro via ConverterHelper.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PassengerViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.DocumentTypes = _converterHelper.GetDocumentTypes();
                return View(viewModel);
            }

            var existing = await _passengerRepository.GetByUserIdAsync(viewModel.UserId!);
            if (existing != null)
            {
                TempData["Error"] = "Este utilizador já tem um perfil de passageiro.";
                return RedirectToAction(nameof(Index));
            }

            var imageId = Guid.Empty;
            if (viewModel.ImageFile != null)
            {
                imageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "passengers");
            }

            // Mapeia para a entidade usando o ConverterHelper
            var passenger = _converterHelper.ToPassenger(viewModel, imageId, isEdit: false);

            await _passengerRepository.AddAsync(passenger);
            await _passengerRepository.SaveAsync();

            TempData["Success"] = "Perfil de passageiro criado com sucesso!";
            return RedirectToAction(nameof(Details), new { id = passenger.Id });
        }

        // ─── EDIT ───────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de edição do perfil de passageiro carregado na ViewModel.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var passenger = await _passengerRepository.GetByIdAsync(id);
            if (passenger == null) return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Employee"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || passenger.UserId != user.Id) return Forbid();
            }

            var vm = _converterHelper.ToPassengerViewModel(passenger);
            return View(vm);
        }

        /// <summary>
        /// Processa a atualização do perfil de passageiro.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PassengerViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.DocumentTypes = _converterHelper.GetDocumentTypes();
                return View(viewModel);
            }

            var passenger = await _passengerRepository.GetByIdAsync(viewModel.Id);
            if (passenger == null) return NotFound();

            if (viewModel.ImageFile != null)
            {
                await _imageHelper.DeleteImageAsync(passenger.ImageId, "passengers");
                passenger.ImageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "passengers");
            }

            passenger.FirstName = viewModel.FirstName;
            passenger.LastName = viewModel.LastName;
            passenger.DocumentType = viewModel.DocumentType;
            passenger.DocumentNumber = viewModel.DocumentNumber;
            passenger.BirthDate = viewModel.BirthDate;

            await _passengerRepository.UpdateAsync(passenger);
            await _passengerRepository.SaveAsync();

            TempData["Success"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Details), new { id = passenger.Id });
        }

        // ─── DELETE ─────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta a página de confirmação de eliminação.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var passenger = await _passengerRepository.GetByIdAsync(id);
            if (passenger == null) return NotFound();

            return View(passenger);
        }

        /// <summary>
        /// Processa a eliminação do perfil (soft delete).
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var passenger = await _passengerRepository.GetByIdAsync(id);
            if (passenger == null) return NotFound();

            await _passengerRepository.DeleteAsync(passenger);
            await _passengerRepository.SaveAsync();

            TempData["Success"] = "Passageiro removido com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
