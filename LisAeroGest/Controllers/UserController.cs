using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LisAeroGest.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão de utilizadores.
    /// Acesso restrito a Administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IPassengerRepository _passengerRepository;
        private readonly ITicketRepository _ticketRepository;

        /// <summary>
        /// Inicializa o UserController com as dependências necessárias.
        /// </summary>
        public UserController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserHelper userHelper,
            IConverterHelper converterHelper,
            IPassengerRepository passengerRepository,
            ITicketRepository ticketRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _passengerRepository = passengerRepository;
            _ticketRepository = ticketRepository;
        }

        /// <summary>
        /// Lista todos os utilizadores registados no sistema.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserWithRole>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault() ?? string.Empty;

                // Delegado ao ConverterHelper
                userRoles.Add(_converterHelper.ToUserWithRole(user, roleName));
            }

            return View(userRoles);
        }

        /// <summary>
        /// Apresenta o formulário de criação de novo utilizador (Funcionário/Admin).
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = _converterHelper.ToRoleSelectList();
            return View();
        }

        /// <summary>
        /// Processa a criação de um novo utilizador (Funcionário/Admin).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string firstName, string lastName, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Todos os campos são obrigatórios.";
                return RedirectToAction(nameof(Create));
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = "Este email já está registado.";
                return RedirectToAction(nameof(Create));
            }

            // Delegado ao ConverterHelper
            var user = _converterHelper.ToUser(email, firstName, lastName);

            // Password temporária gerada aleatoriamente para cada utilizador (evita uma password fixa e previsível)
            var temporaryPassword = GenerateTemporaryPassword();

            var result = await _userManager.CreateAsync(user, temporaryPassword);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Erro ao criar utilizador.";
                return RedirectToAction(nameof(Create));
            }

            await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = $"Utilizador {role} criado com sucesso! Password temporária: {temporaryPassword}";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Gera uma password temporária aleatória que cumpre a política de password da aplicação
        /// (mínimo 6 caracteres, pelo menos uma maiúscula e um dígito).
        /// </summary>
        private static string GenerateTemporaryPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";

            var random = Random.Shared;

            var chars = new[]
            {
                upper[random.Next(upper.Length)],
                lower[random.Next(lower.Length)],
                digits[random.Next(digits.Length)],
                lower[random.Next(lower.Length)],
                digits[random.Next(digits.Length)],
                upper[random.Next(upper.Length)]
            };

            return new string(chars);
        }

        /// <summary>
        /// Altera a role de um utilizador.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

    
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["Error"] = "Não podes alterar a tua própria role.";
                return RedirectToAction(nameof(Index));
            }

            // Impede remover o último Admin do sistema
            if (await _userManager.IsInRoleAsync(user, "Admin") && newRole != "Admin")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Não é possível remover o último Administrador do sistema.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = "Role do utilizador atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Elimina um utilizador do sistema.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Impede que o Admin se elimine a si próprio
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["Error"] = "Não podes eliminar a tua própria conta.";
                return RedirectToAction(nameof(Index));
            }

            // Impede eliminar o último Admin do sistema
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["Error"] = "Não é possível eliminar o último Administrador do sistema.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Impede eliminar um Passageiro que já tenha bilhetes associados
            // (User → Passenger é Cascade, mas Ticket → Passenger é Restrict,
            // por isso é preciso validar aqui antes de a base de dados recusar o cascade)
            var passenger = await _passengerRepository.GetByUserIdAsync(user.Id);
            if (passenger != null)
            {
                var tickets = await _ticketRepository.GetByPassengerAsync(passenger.Id);
                if (tickets.Any())
                {
                    TempData["Error"] = "Não é possível eliminar este utilizador: existem bilhetes associados ao seu perfil de passageiro.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Utilizador eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}

