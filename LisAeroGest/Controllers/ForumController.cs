using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    /// <summary>

    /// Controller responsável pelo fórum de discussão interno.

    /// Acesso restrito a Administradores e Funcionários.

    /// </summary>

    [Authorize(Roles = "Admin, Employee")]

    public class ForumController : Controller

    {

        private readonly IForumTopicRepository _topicRepository;

        private readonly IGenericRepository<ForumComment> _commentRepository;

        private readonly INotificationRepository _notificationRepository;

        private readonly IUserHelper _userHelper;

        private readonly UserManager<User> _userManager;


        /// <summary>

        /// Inicializa o ForumController com as dependências necessárias.

        /// </summary>

        /// <param name="topicRepository">Repositório de tópicos do fórum.</param>

        /// <param name="commentRepository">Repositório de comentários do fórum.</param>

        /// <param name="notificationRepository">Repositório de notificações.</param>

        /// <param name="userHelper">Helper de utilizadores para obter o utilizador autenticado.</param>

        /// <param name="userManager">Gestor de utilizadores do Identity.</param>

        public ForumController(

            IForumTopicRepository topicRepository,

            IGenericRepository<ForumComment> commentRepository,

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


        /// <summary>

        /// Lista todos os tópicos do fórum, ordenados do mais recente ao mais antigo.

        /// </summary>

        /// <returns>View com a listagem de tópicos.</returns>



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var topics = await _topicRepository.GetAllWithDetailsAsync();
            return View(topics);
        }


        /// <summary>

        /// Apresenta o formulário de criação de um novo tópico.

        /// </summary>

        /// <returns>View com o formulário de criação.</returns>

        [HttpGet]

        public IActionResult Create() => View();


        /// <summary>

        /// Processa a criação de um novo tópico no fórum.

        /// Notifica todos os Employees e Admins via sistema de notificações.

        /// </summary>

        /// <param name="topic">Dados do tópico a criar.</param>

        /// <returns>Redirecionamento para a lista de tópicos.</returns>

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


            // Notifica Employees e Admins

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

        /// Apresenta os detalhes de um tópico com os seus comentários.

        /// </summary>

        /// <param name="id">ID do tópico.</param>

        /// <returns>View com o tópico e comentários.</returns>

        

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var topic = await _topicRepository.GetWithCommentsAsync(id);
            if (topic == null) return NotFound();
            return View(topic);
        }


        /// <summary>

        /// Adiciona um comentário a um tópico existente.

        /// Notifica todos os Employees e Admins via sistema de notificações.

        /// </summary>

        /// <param name="topicId">ID do tópico.</param>

        /// <param name="content">Conteúdo do comentário.</param>

        /// <returns>Redirecionamento para os detalhes do tópico.</returns>

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> AddComment(int topicId, string content)

        {

            if (string.IsNullOrWhiteSpace(content))

                return RedirectToAction("Details", new { id = topicId });


            var topic = await _topicRepository.GetByIdAsync(topicId);

            if (topic == null || topic.IsClosed) return NotFound();


            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);

            var userFullName = user?.FullName ?? "Um utilizador";


            var comment = new ForumComment

            {

                ForumTopicId = topicId,

                Content = content,

                CreatedByUserId = user!.Id,

                CreatedAt = DateTime.UtcNow

            };


            await _commentRepository.AddAsync(comment);

            await _commentRepository.SaveAsync();


            // Notifica Employees e Admins

            await NotifyUsersAsync(

                title: "Novo comentário no fórum",

                message: $"{userFullName} comentou no tópico: {topic.Title}",

                link: $"/Forum/Details/{topicId}",

                icon: "bi-chat-left-text",

                color: "text-info",

                type: "ForumComment"

            );


            return RedirectToAction("Details", new { id = topicId });

        }


        /// <summary>

        /// Fecha um tópico a novos comentários. Apenas Administradores.

        /// </summary>

        /// <param name="id">ID do tópico a fechar.</param>

        /// <returns>Redirecionamento para os detalhes do tópico.</returns>

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

        /// Elimina um tópico. Apenas Administradores.

        /// </summary>

        /// <param name="id">ID do tópico a eliminar.</param>

        /// <returns>Redirecionamento para a lista de tópicos.</returns>

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


        // ══════════════════════════════════════════════════════════════

        // HELPER — Notificar Employees e Admins

        // ══════════════════════════════════════════════════════════════

        /// <summary>

        /// Envia uma notificação a todos os Employees e Admins,

        /// exceto ao utilizador que realizou a ação.

        /// </summary>

        /// <param name="title">Título da notificação.</param>

        /// <param name="message">Mensagem detalhada.</param>

        /// <param name="link">Link de redirecionamento.</param>

        /// <param name="icon">Ícone Bootstrap Icons.</param>

        /// <param name="color">Classe de cor CSS.</param>

        /// <param name="type">Tipo de notificação.</param>

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


            var notifications = allUsers.Select(u => new Notification

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


            await _notificationRepository.AddRangeAsync(notifications);

            await _notificationRepository.SaveAsync();

        }

    }
}
