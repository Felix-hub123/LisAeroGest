using LisAeroGest.Data.Entities;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Identity;

namespace LisAeroGest.Helpers
{
    /// <summary>
    /// Implementação do helper de utilizadores.
    /// Encapsula as operações do UserManager e SignInManager do ASP.NET Identity.
    /// </summary>
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Inicializa o UserHelper com as dependências do ASP.NET Identity.
        /// </summary>
        /// <param name="userManager">Gestor de utilizadores do Identity.</param>
        /// <param name="signInManager">Gestor de sessões do Identity.</param>
        /// <param name="roleManager">Gestor de roles do Identity.</param>
        /// <returns>
        /// Instância de <see cref="UserHelper"/> pronta a ser usada
        /// via injeção de dependências no DI container.
        /// </returns>
        public UserHelper(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByEmailAsync(string email)
            => await _userManager.FindByEmailAsync(email);

        /// <inheritdoc/>
        public async Task<User?> GetUserByIdAsync(string id)
            => await _userManager.FindByIdAsync(id);

        /// <inheritdoc/>
        public async Task<IdentityResult> AddUserAsync(User user, string password)
            => await _userManager.CreateAsync(user, password);

        /// <inheritdoc/>
        public async Task<IdentityResult> UpdateUserAsync(User user)
            => await _userManager.UpdateAsync(user);

        /// <inheritdoc/>
        public async Task<IdentityResult> DeleteUserAsync(User user)
            => await _userManager.DeleteAsync(user);

        /// <inheritdoc/>
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
            => await _signInManager.PasswordSignInAsync(
                model.Username!,
                model.Password!,
                model.RememberMe,
                lockoutOnFailure: true);

        /// <inheritdoc/>
        public async Task LogoutAsync()
            => await _signInManager.SignOutAsync();

        /// <inheritdoc/>
        public async Task<IdentityResult> AddUserToRoleAsync(User user, string roleName)
            => await _userManager.AddToRoleAsync(user, roleName);

        /// <inheritdoc/>
        public async Task<IdentityResult> RemoveUserFromRoleAsync(User user, string roleName)
            => await _userManager.RemoveFromRoleAsync(user, roleName);

        /// <inheritdoc/>
        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
            => await _userManager.IsInRoleAsync(user, roleName);

        /// <inheritdoc/>
        public async Task CheckRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        /// <inheritdoc/>
        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
            => await _userManager.GenerateEmailConfirmationTokenAsync(user);

        /// <inheritdoc/>
        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
            => await _userManager.ConfirmEmailAsync(user, token);

        /// <inheritdoc/>
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
            => await _userManager.GeneratePasswordResetTokenAsync(user);

        /// <inheritdoc/>
        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
            => await _userManager.ResetPasswordAsync(user, token, newPassword);

        /// <inheritdoc/>
        public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
            => await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        /// <inheritdoc/>
        public async Task<IdentityResult> AddPasswordAsync(User user, string password)
            => await _userManager.AddPasswordAsync(user, password);
    }
}
