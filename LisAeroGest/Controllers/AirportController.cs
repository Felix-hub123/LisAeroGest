using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
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
        private readonly IBlobHelper _blobHelper;

        /// <summary>
        /// Inicializa o AirportController com as dependências necessárias.
        /// </summary>
        /// <param name="airportRepository">Repositório de aeroportos para acesso à base de dados.</param>
        /// <param name="blobHelper">Helper para upload de imagens no Azure Blob Storage.</param>
        /// <returns>
        /// Instância de <see cref="AirportController"/> pronta a processar
        /// pedidos de gestão de aeroportos.
        /// </returns>
        public AirportController(IAirportRepository airportRepository, IBlobHelper blobHelper)
        {
            _airportRepository = airportRepository;
            _blobHelper = blobHelper;
        }

        /// <summary>
        /// Lista todos os aeroportos registados no sistema.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var airports = await _airportRepository.GetAllAsync();
            return View(airports);
        }

        /// <summary>
        /// Apresenta o formulário de criação de novo aeroporto.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
            => View(new AirportViewModel());

        /// <summary>
        /// Processa o formulário de criação de novo aeroporto.
        /// Faz upload da imagem para o Azure Blob Storage se fornecida.
        /// </summary>
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
                imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "airports");

            var airport = new Airport
            {
                Name = model.Name,
                City = model.City,
                Country = model.Country,
                IATACode = model.IATACode!.ToUpper(),
                DefaultFee = model.DefaultFee,
                ImageId = imageId
            };

            await _airportRepository.AddAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta o formulário de edição de um aeroporto existente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var airport = await _airportRepository.GetByIdAsync(id);
            if (airport == null)
                return NotFound();

            var model = new AirportViewModel
            {
                Id = airport.Id,
                Name = airport.Name,
                City = airport.City,
                Country = airport.Country,
                IATACode = airport.IATACode,
                DefaultFee = airport.DefaultFee,
                ImageId = airport.ImageId
            };

            return View(model);
        }

        /// <summary>
        /// Processa o formulário de edição de um aeroporto existente.
        /// </summary>
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

            // Faz upload de nova imagem se fornecida
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                if (airport.ImageId != Guid.Empty)
                    await _blobHelper.DeleteBlobAsync(airport.ImageId, "airports");

                airport.ImageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "airports");
            }

            airport.Name = model.Name;
            airport.City = model.City;
            airport.Country = model.Country;
            airport.IATACode = model.IATACode!.ToUpper();
            airport.DefaultFee = model.DefaultFee;

            await _airportRepository.UpdateAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Apresenta a página de confirmação de eliminação de um aeroporto.
        /// </summary>
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
        /// Impede a eliminação se o aeroporto estiver associado a voos.
        /// </summary>
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
