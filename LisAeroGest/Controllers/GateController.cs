using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Repositories;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão de gates (portões de embarque).
    /// Acesso permitido a Administradores e Funcionários.
    /// </summary>
    [Authorize(Roles = "Admin,Employee")]
    public class GateController : Controller
    {
        private const int PageSize = 10;

        private readonly IGateRepository _gateRepository;
        private readonly IConverterHelper _converterHelper;

        public GateController(
            IGateRepository gateRepository,
            IConverterHelper converterHelper)
        {
            _gateRepository = gateRepository;
            _converterHelper = converterHelper;
        }

        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var allGates =
                (await _gateRepository.GetAllAsync())
                .OrderBy(g => g.Terminal)
                .ThenBy(g => g.GateNumber)
                .ToList();

            var totalGates =
                allGates.Count;

            var totalPages =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalGates /
                        (double)PageSize));

            page = Math.Max(
                1,
                Math.Min(
                    page,
                    totalPages));

            var gates =
                allGates
                    .Skip(
                        (page - 1) *
                        PageSize)
                    .Take(PageSize)
                    .ToList();

            var model =
                new GateIndexViewModel
                {
                    Gates = gates,
                    TotalGates = totalGates,
                    Page = page,
                    PageSize = PageSize,
                    TotalPages = totalPages
                };

            return View(model);
        }

        // =========================================================
        // CREATE
        // =========================================================

        [HttpGet]
        public IActionResult Create()
            => View(new GateViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            GateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing =
                await _gateRepository
                    .GetByGateNumberAsync(
                        model.GateNumber!);

            if (existing != null)
            {
                ModelState.AddModelError(
                    "GateNumber",
                    "Já existe um gate com este número.");

                return View(model);
            }

            var gate =
                _converterHelper
                    .ToGate(
                        model,
                        isEdit: false);

            await _gateRepository
                .AddAsync(gate);

            await _gateRepository
                .SaveAsync();

            TempData["Success"] =
                "Gate criado com sucesso!";

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // EDIT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var gate =
                await _gateRepository
                    .GetByIdAsync(id);

            if (gate == null)
                return NotFound();

            var model =
                _converterHelper
                    .ToGateViewModel(gate);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            GateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var gate =
                await _gateRepository
                    .GetByIdAsync(model.Id);

            if (gate == null)
                return NotFound();

            var existing =
                await _gateRepository
                    .GetByGateNumberAsync(
                        model.GateNumber!);

            if (existing != null &&
                existing.Id != model.Id)
            {
                ModelState.AddModelError(
                    "GateNumber",
                    "Já existe um gate com este número.");

                return View(model);
            }

            gate.GateNumber =
                model.GateNumber!.ToUpper();

            gate.Terminal =
                model.Terminal;

            gate.Status =
                model.Status;

            await _gateRepository
                .UpdateAsync(gate);

            await _gateRepository
                .SaveAsync();

            TempData["Success"] =
                "Gate atualizado com sucesso!";

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var gate =
                await _gateRepository
                    .GetByIdAsync(id);

            if (gate == null)
                return NotFound();

            return View(gate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var gate =
                await _gateRepository
                    .GetByIdAsync(id);

            if (gate == null)
                return NotFound();

            var isUsed =
                await _gateRepository
                    .IsUsedInFlightsAsync(id);

            if (isUsed)
            {
                TempData["Error"] =
                    "Não é possível eliminar este gate pois está associado a voos.";

                return RedirectToAction(
                    nameof(Index));
            }

            await _gateRepository
                .DeleteAsync(gate);

            await _gateRepository
                .SaveAsync();

            TempData["Success"] =
                "Gate eliminado com sucesso!";

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var gate =
                await _gateRepository
                    .GetByIdAsync(id);

            if (gate == null)
                return NotFound();

            var model =
                _converterHelper
                    .ToGateViewModel(gate);

            return View(model);
        }

        // =========================================================
        // BOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Board()
        {
            var gates =
                await _gateRepository
                    .GetAllAsync();

            var model =
                gates
                    .OrderBy(g => g.Terminal)
                    .ThenBy(g => g.GateNumber)
                    .ToList();

            return View(model);
        }
    }
}