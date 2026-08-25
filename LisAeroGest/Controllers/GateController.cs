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
        private readonly IGateRepository _gateRepository;
        private readonly IConverterHelper _converterHelper;

        /// <summary>
        /// Inicializa o GateController com as dependências necessárias.
        /// </summary>
        public GateController(IGateRepository gateRepository, IConverterHelper converterHelper)
        {
            _gateRepository = gateRepository;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Lista todos os gates registados no sistema.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var gates = await _gateRepository.GetAllAsync();
            return View(gates);
        }

        /// <summary>
        /// Apresenta o formulário de criação de novo gate.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
            => View(new GateViewModel());

        /// <summary>
        /// Processa o formulário de criação de novo gate utilizando o ConverterHelper.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verifica número de gate duplicado
            var existing = await _gateRepository.GetByGateNumberAsync(model.GateNumber!);
            if (existing != null)
            {
                ModelState.AddModelError("GateNumber", "Já existe um gate com este número.");
                return View(model);
            }

            // Delega a conversão para o ConverterHelper
            var gate = _converterHelper.ToGate(model, isEdit: false);

            await _gateRepository.AddAsync(gate);
            await _gateRepository.SaveAsync();

            TempData["Success"] = "Gate criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta o formulário de edição de um gate existente via ConverterHelper.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var gate = await _gateRepository.GetByIdAsync(id);
            if (gate == null)
                return NotFound();

            var model = _converterHelper.ToGateViewModel(gate);
            return View(model);
        }

        /// <summary>
        /// Processa o formulário de edição de um gate existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var gate = await _gateRepository.GetByIdAsync(model.Id);
            if (gate == null)
                return NotFound();

            // Verifica número de gate duplicado noutro gate
            var existing = await _gateRepository.GetByGateNumberAsync(model.GateNumber!);
            if (existing != null && existing.Id != model.Id)
            {
                ModelState.AddModelError("GateNumber", "Já existe um gate com este número.");
                return View(model);
            }

            // Atualiza as propriedades com base no model (ou podes converter)
            gate.GateNumber = model.GateNumber!.ToUpper();
            gate.Terminal = model.Terminal;
            gate.Status = model.Status;

            await _gateRepository.UpdateAsync(gate);
            await _gateRepository.SaveAsync();

            TempData["Success"] = "Gate atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta a página de confirmação de eliminação de um gate.
        /// A view Views/Gate/Delete.cshtml usa a entidade diretamente (@model Gate).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var gate = await _gateRepository.GetByIdAsync(id);
            if (gate == null)
                return NotFound();

            return View(gate);
        }

        /// <summary>
        /// Processa a eliminação lógica de um gate.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gate = await _gateRepository.GetByIdAsync(id);
            if (gate == null)
                return NotFound();

            var isUsed = await _gateRepository.IsUsedInFlightsAsync(id);
            if (isUsed)
            {
                TempData["Error"] = "Não é possível eliminar este gate pois está associado a voos.";
                return RedirectToAction(nameof(Index));
            }

            await _gateRepository.DeleteAsync(gate);
            await _gateRepository.SaveAsync();

            TempData["Success"] = "Gate eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta os detalhes de um gate mapeados para ViewModel.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var gate = await _gateRepository.GetByIdAsync(id);
            if (gate == null)
                return NotFound();

            var model = _converterHelper.ToGateViewModel(gate);
            return View(model);
        }
    }
}