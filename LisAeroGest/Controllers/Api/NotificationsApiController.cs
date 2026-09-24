using LisAeroGest.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LisAeroGest.Controllers.Api
{
    /// <summary>
    /// API para notificações do utilizador autenticado.
    /// </summary>
    [Route("api/notifications")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsApiController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // ═══════════════════════════════════════════════════════════
        // 1. LISTAR NOTIFICAÇÕES DO UTILIZADOR
        // GET: /api/notifications
        // ═══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não identificado." });

            var notifications = (await _notificationRepository.GetByUserAsync(userId))
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    link = n.Link,
                    icon = n.Icon,
                    colorClass = n.ColorClass,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt,
                    type = n.Type
                })
                .ToList();

            return Ok(notifications);
        }

        // ═══════════════════════════════════════════════════════════
        // 2. MARCAR NOTIFICAÇÃO COMO LIDA
        // POST: /api/notifications/{id}/read
        // ═══════════════════════════════════════════════════════════

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não identificado." });

            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null || notification.UserId != userId)
                return NotFound(new { message = "Notificação não encontrada." });

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
                await _notificationRepository.SaveAsync();
            }

            return Ok(new { success = true });
        }




        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var count =
                await _notificationRepository
                    .GetUnreadCountAsync(userId);

            return Ok(new
            {
                count
            });
        }
    }
}