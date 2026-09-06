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
    /// Controller responsável pelo fórum de discussão interno e moderação de comentários.
    /// Acesso restrito a Administradores e Funcionários.
    /// </summary>
    [Authorize(Roles = "Admin, Employee")]
    public class ForumController : Controller
    {
        private readonly IForumTopicRepository _topicRepository;
        private readonly IForumCommentRepository _commentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserHelper _userHelper;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Inicializa o ForumController com os repositórios específicos e helpers do sistema.
        /// </summary>
        public ForumController(
            IForumTopicRepository topicRepository,
            IForumCommentRepository commentRepository,
            INotificationRepository notificationRepository,
            IUserHelper userHelper,
            UserManager<User> userManager)
        {
            _topicRepository = topicRepository;
            _commentRepository = commentRepository;
            _notificationRepository = notificationRepository;
            _userHelper = userHelper;
            _userManager = userManager;
        }

        #region Leitura e Listagem (Index & Details)

        /// <summary>
        /// Lista todos os tópicos do fórum, ordenados do mais recente ao mais antigo.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var topics = await _topicRepository.GetAllAsync(); // Deve carregar CreatedBy e Comments

            var model = topics.Select(t => new ForumTopicViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Content = t.Content,
                CreatedAt = t.CreatedAt,
                IsClosed = t.IsClosed,
                AuthorName = t.CreatedBy?.FullName ?? "N/A",
                CreatedByUserId = t.CreatedByUserId,
                // Mantém na coleção apenas os comentários aprovados para a contagem correta na View
                Comments = t.Comments?.Where(c => c.IsApproved).ToList() ?? new List<ForumComment>()
            }).OrderByDescending(t => t.CreatedAt);

            return View(model);
        }

        /// <summary>
        /// Apresenta os detalhes de um tópico com os seus respetivos comentários.
        /// </summary>
        /// <param name="id">ID do tópico.</param>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var topic = await _topicRepository.GetByIdAsync(id);
            if (topic == null) return NotFound();

            var userEmail = User.Identity?.Name ?? string.Empty;
            var isAdmin = User.IsInRole("Admin");

            // Procura na BD apenas os comentários que este utilizador pode ver
            var visibleComments = await _commentRepository.GetVisibleCommentsForUserAsync(id, userEmail, isAdmin);

            // Injeta a lista já filtrada nos Comments da Model
            topic.Comments = visibleComments.ToList();

            return View(topic);
        }

        #endregion

        #region Gestão de Tópicos (Create, Close, Delete)

        /// <summary>
        /// Apresenta o formulário de criação de um novo tópico.
        /// </summary>
        [HttpGet]
        public IActionResult Create() => View();

        /// <summary>
        /// Processa a criação de um novo tópico no fórum e notifica a equipa.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumTopic topic)
        {
            if (!ModelState.IsValid) return View(topic);

            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            var userFullName = user?.FullName ?? "Um utilizador";

            topic.CreatedByUserId = user!.Id;
            topic.CreatedAt = DateTime.UtcNow;

            await _topicRepository.AddAsync(topic);
            await _topicRepository.SaveAsync();

            // Notifica Employees e Admins do novo tópico criado
            await NotifyUsersAsync(
                title: "Novo tópico no fórum",
                message: $"{userFullName} criou o tópico: {topic.Title}",
                link: $"/Forum/Details/{topic.Id}",
                icon: "bi-chat-dots",
                color: "text-primary",
                type: "ForumTopic"
            );

            TempData["Success"] = "Tópico criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Fecha um tópico a novos comentários. Apenas para Administradores.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var topic = await _topicRepository.GetByIdAsync(id);
            if (topic == null) return NotFound();

            topic.IsClosed = true;
            await _topicRepository.UpdateAsync(topic);
            await _topicRepository.SaveAsync();

            TempData["Success"] = "Tópico fechado com sucesso!";
            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// Elimina um tópico da base de dados. Apenas para Administradores.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var topic = await _topicRepository.GetByIdAsync(id);
            if (topic == null) return NotFound();

            await _topicRepository.DeleteAsync(topic);
            await _topicRepository.SaveAsync();

            TempData["Success"] = "Tópico eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Comentários e Moderação (AddComment, ApproveComment, PendingComments)

        /// <summary>
        /// Submete um novo comentário a um tópico existente.
        /// Admins têm aprovação automática; Employees entram no fluxo de moderação.
        /// </summary>




        /// <summary>
        /// Fila de comentários pendentes de aprovação. Apenas Administradores.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PendingComments()
        {
            var pendingComments = await _commentRepository.GetPendingCommentsAsync();

            var model = pendingComments.Select(c =>
            {
                var firstName = c.CreatedBy?.FirstName?.Trim() ?? string.Empty;
                var lastName = c.CreatedBy?.LastName?.Trim() ?? string.Empty;
                var fullName = c.CreatedBy?.FullName?.Trim();

                var authorName = !string.IsNullOrWhiteSpace(fullName)
                    ? fullName
                    : $"{firstName} {lastName}".Trim();

                if (string.IsNullOrWhiteSpace(authorName))
                    authorName = "Utilizador desconhecido";

                var initial = !string.IsNullOrEmpty(firstName)
                    ? firstName.Substring(0, 1).ToUpper()
                    : "?";

                return new ForumPendingCommentViewModel
                {
                    Id = c.Id,
                    ForumTopicId = c.ForumTopicId,
                    TopicTitle = c.ForumTopic?.Title ?? $"Tópico #{c.ForumTopicId}",
                    AuthorName = authorName,
                    AuthorInitial = initial,
                    Content = c.Content ?? string.Empty,
                    CreatedAt = c.CreatedAt
                };
            }).ToList();

            return View(model);
        }

        #endregion

        #region Métodos Auxiliares Privados

        /// <summary>
        /// Envia uma notificação no sistema para todos os Employees e Admins,
        /// ignorando o utilizador responsável pela ação.
        /// </summary>
        private async Task NotifyUsersAsync(
            string title,
            string message,
            string link,
            string icon,
            string color,
            string type)
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var allUsers = employees.Union(admins).ToList();

            var currentUser = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (currentUser != null)
                allUsers = allUsers.Where(u => u.Id != currentUser.Id).ToList();

            if (!allUsers.Any()) return;

            foreach (var u in allUsers)
            {
                var notification = new Notification
                {
                    UserId = u.Id,
                    Title = title,
                    Message = message,
                    Link = link,
                    Icon = icon,
                    ColorClass = color,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(notification);
            }

            await _notificationRepository.SaveAsync();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int topicId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Details", new { id = topicId });

            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null || topic.IsClosed)
                return NotFound();

            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (user == null)
                return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var comment = new ForumComment
            {
                ForumTopicId = topicId,
                Content = content,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsApproved = isAdmin // Admin: imediato | Employee: pendente
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveAsync();

            if (isAdmin)
            {
                // Comentário já visível → avisa a equipa (Admin + Employee)
                await NotifyUsersAsync(
                    title: "Novo comentário no fórum",
                    message: $"{user.FullName} comentou no tópico: {topic.Title}",
                    link: $"/Forum/Details/{topicId}",
                    icon: "bi-chat-left-text",
                    color: "text-info",
                    type: "ForumComment"
                );
                TempData["Success"] = "Comentário adicionado!";
            }
            else
            {
                // Comentário pendente → avisa só os Administradores
                await NotifyAdminsAsync(
                    title: "Comentário aguarda aprovação",
                    message: $"{user.FullName} comentou no tópico \"{topic.Title}\" e aguarda moderação.",
                    link: "/Forum/PendingComments",
                    icon: "bi-hourglass-split",
                    color: "text-warning",
                    type: "ForumModeration"
                );
                TempData["Info"] = "O seu comentário foi submetido e aguarda aprovação de um Administrador.";
            }

            return RedirectToAction("Details", new { id = topicId });
        }


        /// <summary>
        /// Notifica apenas os Administradores (exceto o utilizador atual).
        /// Usado quando um Employee submete comentário pendente.
        /// </summary>
        private async Task NotifyAdminsAsync(
            string title,
            string message,
            string link,
            string icon,
            string color,
            string type)
        {
            var admins = (await _userManager.GetUsersInRoleAsync("Admin")).ToList();

            var currentUser = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (currentUser != null)
                admins = admins.Where(u => u.Id != currentUser.Id).ToList();

            if (!admins.Any()) return;

            foreach (var u in admins)
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = u.Id,
                    Title = title,
                    Message = message,
                    Link = link,
                    Icon = icon,
                    ColorClass = color,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _notificationRepository.SaveAsync();
        }

        /// <summary>
        /// Notifica um único utilizador (ex.: autor do comentário aprovado).
        /// </summary>
        private async Task NotifySingleUserAsync(
            string userId,
            string title,
            string message,
            string link,
            string icon,
            string color,
            string type)
        {
            if (string.IsNullOrEmpty(userId)) return;

            await _notificationRepository.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                Icon = icon,
                ColorClass = color,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _notificationRepository.SaveAsync();
        }




        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveComment(int commentId, int topicId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return NotFound();

            var topic = await _topicRepository.GetByIdAsync(topicId);

            comment.IsApproved = true;
            await _commentRepository.UpdateAsync(comment);
            await _commentRepository.SaveAsync();

            // Notifica o autor do comentário
            if (!string.IsNullOrEmpty(comment.CreatedByUserId))
            {
                await NotifySingleUserAsync(
                    userId: comment.CreatedByUserId,
                    title: "Comentário aprovado",
                    message: topic != null
                        ? $"O seu comentário no tópico \"{topic.Title}\" foi aprovado."
                        : "O seu comentário no fórum foi aprovado.",
                    link: $"/Forum/Details/{topicId}",
                    icon: "bi-check-circle",
                    color: "text-success",
                    type: "ForumApproved"
                );
            }

            TempData["Success"] = "Comentário aprovado com sucesso!";
            return RedirectToAction("Details", new { id = topicId });
        }

        #endregion
    }
}
