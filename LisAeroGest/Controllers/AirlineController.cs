using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AirlineController : Controller
    {
        private readonly IAirlineRepository _airlineRepository;
        private readonly IImageHelper _imageHelper;

        public AirlineController(IAirlineRepository airlineRepository, IImageHelper imageHelper)
        {
            _airlineRepository = airlineRepository;
            _imageHelper = imageHelper;
        }

        public async Task<IActionResult> Index()
        {
            // O repositório já devolve a lista filtrada pelo Soft Delete
            return View(await _airlineRepository.GetAllAsync());
        }

        [HttpGet]
        public IActionResult Create() => View(new AirlineViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirlineViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Verifica se o código IATA já existe
            var existing = await _airlineRepository.GetByIATACodeAsync(model.IATACode!.ToUpper());
            if (existing != null)
            {
                ModelState.AddModelError("IATACode", "Já existe uma companhia com este código IATA.");
                return View(model);
            }

            var imageId = Guid.Empty;
            if (model.ImageFile != null)
                imageId = await _imageHelper.UploadImageAsync(model.ImageFile, "airlines");

            var airline = new Airline
            {
                Name = model.Name,
                IATACode = model.IATACode!.ToUpper(),
                Country = model.Country,
                ImageId = imageId
            };

            await _airlineRepository.AddAsync(airline);
            await _airlineRepository.SaveAsync();

            TempData["Success"] = "Companhia aérea criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var airline = await _airlineRepository.GetByIdAsync(id);
            if (airline == null) return NotFound();

            var model = new AirlineViewModel
            {
                Id = airline.Id,
                Name = airline.Name,
                IATACode = airline.IATACode,
                Country = airline.Country,
                ImageId = airline.ImageId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AirlineViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var airline = await _airlineRepository.GetByIdAsync(model.Id);
            if (airline == null) return NotFound();

            // Verifica IATA duplicado noutra companhia
            var existing = await _airlineRepository.GetByIATACodeAsync(model.IATACode!.ToUpper());
            if (existing != null && existing.Id != model.Id)
            {
                ModelState.AddModelError("IATACode", "Este código IATA já está em uso por outra companhia.");
                return View(model);
            }

            if (model.ImageFile != null)
            {

                await _imageHelper.DeleteImageAsync(airline.ImageId, "airlines");
                airline.ImageId = await _imageHelper.UploadImageAsync(model.ImageFile, "airlines");
            }

            airline.Name = model.Name;
            airline.IATACode = model.IATACode!.ToUpper();
            airline.Country = model.Country;

            await _airlineRepository.UpdateAsync(airline);
            await _airlineRepository.SaveAsync();

            TempData["Success"] = "Companhia aérea atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var airline = await _airlineRepository.GetWithFlightsAsync(id);
            if (airline == null) return NotFound();
            return View(airline);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var airline = await _airlineRepository.GetByIdAsync(id);
            if (airline == null) return NotFound();
            return View(airline);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airline = await _airlineRepository.GetByIdAsync(id);
            if (airline == null) return NotFound();


            await _airlineRepository.DeleteAsync(airline);
            await _airlineRepository.SaveAsync();

            TempData["Success"] = "Companhia aérea eliminada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
