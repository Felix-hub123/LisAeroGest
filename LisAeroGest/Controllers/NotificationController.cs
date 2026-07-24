using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<User> _userManager;
        private readonly IConverterHelper _converterHelper;

        public NotificationController(
            INotificationRepository notificationRepository,
            UserManager<User> userManager,
            IConverterHelper converterHelper)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
            _converterHelper = converterHelper;
        }

        /// <summary>
        /// Apresenta a lista de notificações do utilizador convertida para ViewModel.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
            if (user == null) return RedirectToAction("Index", "Home");

            var notifications = await _notificationRepository.GetByUserAsync(user.Id);

            // Mapeia a lista de entidades para ViewModels via ConverterHelper
            var viewModelList = _converterHelper.ToNotificationViewModelList(notifications);

            return View(viewModelList);
        }

        /// <summary>
        /// Obtém a quantidade de notificações não lidas para o utilizador atual (AJAX).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
            if (user == null) return Json(new { count = 0 });

            var count = await _notificationRepository.GetUnreadCountAsync(user.Id);
            return Json(new { count });
        }

        /// <summary>
        /// Marca uma notificação específica como lida e redireciona para o link associado.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
            if (user == null) return RedirectToAction("Index", "Home");

            var notifications = await _notificationRepository.GetByUserAsync(user.Id);
            var notification = notifications.FirstOrDefault(n => n.Id == id);

            if (notification != null)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
                await _notificationRepository.SaveAsync();
            }

            if (!string.IsNullOrEmpty(notification?.Link))
                return Redirect(notification.Link!);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Marca todas as notificações do utilizador como lidas.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
            if (user == null) return RedirectToAction("Index", "Home");

            var notifications = await _notificationRepository.GetUnreadByUserAsync(user.Id);

            foreach (var n in notifications)
            {
                n.IsRead = true;
                await _notificationRepository.UpdateAsync(n);
            }

            await _notificationRepository.SaveAsync();
            TempData["Success"] = "Todas as notificações marcadas como lidas.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Elimina uma notificação do utilizador.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name!);
            if (user == null) return RedirectToAction("Index", "Home");

            var notifications = await _notificationRepository.GetByUserAsync(user.Id);
            var notification = notifications.FirstOrDefault(n => n.Id == id);

            if (notification != null)
            {
                await _notificationRepository.DeleteAsync(notification);
                await _notificationRepository.SaveAsync();
                TempData["Success"] = "Notificação eliminada.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
