using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão dos aeroportos.
    /// Operações permitidas para as roles Admin e Employee.
    /// </summary>
    [Authorize(Roles = "Admin,Employee")]
    public class AirportsController : Controller
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;

        public AirportsController(
            IAirportRepository airportRepository,
            IConverterHelper converterHelper,
            IImageHelper imageHelper)
        {
            _airportRepository = airportRepository;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
        }

        #region Leitura (Index & Details)

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var airports = await _airportRepository.GetAllAsync();
            return View(airports);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var airport = await _airportRepository.GetByIdAsync(id.Value);
            if (airport == null) return NotFound();

            return View(airport);
        }

        #endregion

        #region Criação (Create)

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new AirportViewModel
            {
                Countries = _converterHelper.GetCountries(),
                Cities = _converterHelper.GetCities()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirportViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Countries = _converterHelper.GetCountries(viewModel.Country);
                viewModel.Cities = _converterHelper.GetCities(viewModel.Country, viewModel.City);
                return View(viewModel);
            }

            // Tratamento da Imagem
            Guid imageId = Guid.Empty;
            if (viewModel.ImageFile != null)
            {
                imageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "airports");
            }

            var airport = _converterHelper.ToAirport(viewModel, imageId);
            await _airportRepository.AddAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Edição (Edit)

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var airport = await _airportRepository.GetByIdAsync(id.Value);
            if (airport == null) return NotFound();

           
            var viewModel = _converterHelper.ToAirportViewModel(airport);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AirportViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();
                       
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var airport = await _airportRepository.GetByIdAsync(viewModel.Id);
            if (airport == null) return NotFound();

            // Mantém a imagem atual ou substitui por uma nova no armazenamento
            Guid imageId = airport.ImageId;
            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                if (airport.ImageId != Guid.Empty)
                {
                    await _imageHelper.DeleteImageAsync(airport.ImageId, "airports");
                }
                imageId = await _imageHelper.UploadImageAsync(viewModel.ImageFile, "airports");
            }

            // Atualiza apenas os campos operacionais (DefaultFee) e Imagem
            _converterHelper.UpdateAirportFromViewModel(airport, viewModel, imageId);

            await _airportRepository.UpdateAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Remoção (Delete)

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var airport = await _airportRepository.GetByIdAsync(id.Value);
            if (airport == null) return NotFound();

            return View(airport);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airport = await _airportRepository.GetByIdAsync(id);
            if (airport == null) return NotFound();

            await _airportRepository.DeleteAsync(airport);
            await _airportRepository.SaveAsync();

            TempData["Success"] = "Aeroporto eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Endpoints AJAX para Formulários

        [HttpGet]
        public IActionResult GetCitiesByCountry(string country)
        {
            var cities = _converterHelper.GetCitiesWithIata(country);
            return Json(cities);
        }

        #endregion
    }
}

