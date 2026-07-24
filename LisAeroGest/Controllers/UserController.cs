using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LisAeroGest.Data.Entities;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        /// <summary>
        /// Inicializa o UserController com as dependências necessárias.
        /// </summary>
        public UserController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserHelper userHelper,
            IConverterHelper converterHelper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
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

            var result = await _userManager.CreateAsync(user, "Mudar123!");
            if (!result.Succeeded)
            {
                TempData["Error"] = "Erro ao criar utilizador.";
                return RedirectToAction(nameof(Create));
            }

            await _userManager.AddToRoleAsync(user, role);

            TempData["Success"] = $"Utilizador {role} criado com sucesso! Password temporária: Mudar123!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Altera a role de um utilizador.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

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

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Utilizador eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}

