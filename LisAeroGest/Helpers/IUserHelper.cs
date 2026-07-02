using LisAeroGest.Data.Entities;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Identity;

namespace LisAeroGest.Helpers
{
    /// <summary>
    /// Interface que define as operações de gestão de utilizadores,
    /// incluindo autenticação, registo, roles e password.
    /// </summary>
    public interface IUserHelper
    {
        /// <summary>
        /// Obtém um utilizador pelo seu endereço de email.
        /// </summary>
        /// <param name="email">Endereço de email do utilizador a pesquisar.</param>
        /// <returns>
        /// O <see cref="User"/> correspondente ao email,
        /// ou <c>null</c> se não existir nenhum utilizador com esse email.
        /// </returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Obtém um utilizador pelo seu ID.
        /// </summary>
        Task<User?> GetUserByIdAsync(string id);

        /// <summary>
        /// Cria um novo utilizador com a password fornecida.
        /// </summary>
        Task<IdentityResult> AddUserAsync(User user, string password);

        /// <summary>
        /// Atualiza os dados de um utilizador existente.
        /// </summary>
        Task<IdentityResult> UpdateUserAsync(User user);

        /// <summary>
        /// Elimina um utilizador do sistema.
        /// </summary>
        Task<IdentityResult> DeleteUserAsync(User user);

        /// <summary>
        /// Faz login de um utilizador com email e password.
        /// Aplica lockout automático após 5 tentativas falhadas.
        /// </summary>
        /// <param name="model">ViewModel com email, password e opção "lembrar-me".</param>
        /// <returns>
        /// <see cref="SignInResult"/> indicando se o login foi bem-sucedido,
        /// se a conta está bloqueada ou se as credenciais são inválidas.
        /// </returns>
        Task<SignInResult> LoginAsync(LoginViewModel model);

        /// <summary>
        /// Termina a sessão do utilizador atual.
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Adiciona um utilizador a uma role específica.
        /// </summary>
        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);

        /// <summary>
        /// Remove um utilizador de uma role específica.
        /// </summary>
        Task<IdentityResult> RemoveUserFromRoleAsync(User user, string roleName);

        /// <summary>
        /// Verifica se um utilizador pertence a uma role específica.
        /// </summary>
        Task<bool> IsUserInRoleAsync(User user, string roleName);

        /// <summary>
        /// Verifica se uma role existe e cria-a caso não exista.
        /// </summary>
        Task CheckRoleAsync(string roleName);

        /// <summary>
        /// Gera um token para confirmação de email.
        /// </summary>
        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        /// <summary>
        /// Confirma o email de um utilizador com o token fornecido.
        /// </summary>
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        /// <summary>
        /// Gera um token para recuperação de password.
        /// </summary>
        Task<string> GeneratePasswordResetTokenAsync(User user);

        /// <summary>
        /// Redefine a password de um utilizador com o token de recuperação.
        /// </summary>
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);

        /// <summary>
        /// Altera a password de um utilizador autenticado.
        /// </summary>
        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);

        /// <summary>
        /// Define uma password para um utilizador que ainda não tem password definida.
        /// </summary>
        Task<IdentityResult> AddPasswordAsync(User user, string password);
    }
}
