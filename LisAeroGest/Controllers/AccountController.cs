using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Mvc;

namespace LisAeroGest.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa o AccountController com as dependências necessárias.
        /// </summary>
        /// <param name="userHelper">Helper de gestão de utilizadores do Identity.</param>
        /// <param name="mailHelper">Helper de envio de emails transacionais.</param>
        /// <param name="configuration">Configuração da aplicação para leitura de settings.</param>
        /// <returns>
        /// Instância de <see cref="AccountController"/> pronta a processar
        /// pedidos de autenticação e gestão de conta.
        /// </returns>
        public AccountController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConfiguration configuration)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _configuration = configuration;
        }

        // ─── Login ───────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de login.
        /// Redireciona para a página inicial se o utilizador já estiver autenticado.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        /// <summary>
        /// Processa o formulário de login.
        /// Autentica o utilizador e redireciona para a página inicial em caso de sucesso.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userHelper.LoginAsync(model);

            if (result.Succeeded)
            {
                if (Request.Query.Keys.Contains("ReturnUrl"))
                    return Redirect(Request.Query["ReturnUrl"].First()!);

                // Redireciona consoante a role do utilizador
                var loggedUser = await _userHelper.GetUserByEmailAsync(model.Username!);

                if (await _userHelper.IsUserInRoleAsync(loggedUser!, "Admin"))
                    return RedirectToAction("Index", "Dashboard");

                if (await _userHelper.IsUserInRoleAsync(loggedUser!, "Employee"))
                    return RedirectToAction("Index", "Dashboard");

                // Passenger vai para a página inicial
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Conta bloqueada temporariamente. Tente novamente em 15 minutos.");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty,
                    "Confirme o seu email antes de fazer login.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email ou password incorretos.");
            return View(model);
        }

        // ─── Logout ──────────────────────────────────────────────────────────

        /// <summary>
        /// Termina a sessão do utilizador autenticado e redireciona para o login.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Login");
        }

        // ─── Registo ─────────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de registo de novo passageiro.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        /// <summary>
        /// Processa o formulário de registo.
        /// Cria o utilizador, atribui a role Passenger e envia email de confirmação.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Verifica se o email já está registado
            var existingUser = await _userHelper.GetUserByEmailAsync(model.Username!);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Este email já está registado.");
                return View(model);
            }

            // Cria o utilizador
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Username,
                UserName = model.Username,
                Address = model.Address,
                IsPasswordSet = true
            };

            var result = await _userHelper.AddUserAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            // Atribui a role Passenger
            await _userHelper.AddUserToRoleAsync(user, "Passenger");

            // Envia email de confirmação
            var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                "ConfirmEmail", "Account",
                new { userId = user.Id, token },
                protocol: HttpContext.Request.Scheme);

            var emailBody = $@"
                <h2>Bem-vindo ao LisAeroGest!</h2>
                <p>Olá {user.FirstName},</p>
                <p>Obrigado por se registar. Clique no link abaixo para confirmar o seu email:</p>
                <p><a href='{confirmationLink}'>Confirmar Email</a></p>
                <p>Se não se registou, ignore este email.</p>
                <br/>
                <p>LisAeroGest — Aeroporto de Lisboa</p>";

            var response = _mailHelper.SendEmail(
                model.Username!,
                "Confirmação de Email — LisAeroGest",
                emailBody);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty,
                    "Utilizador criado mas não foi possível enviar o email de confirmação.");
                return View(model);
            }

            ViewBag.Message = "Registo efetuado com sucesso! Verifique o seu email para confirmar a conta.";
            return View("RegisterConfirmation");
        }

        // ─── Confirmação de Email ─────────────────────────────────────────────

        /// <summary>
        /// Confirma o email do utilizador através do token recebido por email.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                ViewBag.Message = "Email confirmado com sucesso! Já pode fazer login.";
            else
                ViewBag.Message = "Erro ao confirmar email. O link pode ter expirado.";

            return View();
        }

        // ─── Recuperação de Password ──────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de recuperação de password.
        /// </summary>
        [HttpGet]
        public IActionResult RecoverPassword()
            => View();

        /// <summary>
        /// Processa o pedido de recuperação de password.
        /// Envia um email com o link para redefinir a password.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email!);

            // Por segurança, não revelamos se o email existe ou não
            if (user == null)
            {
                ViewBag.Message = "Se este email estiver registado, receberá um link de recuperação.";
                return View("RecoverPasswordConfirmation");
            }

            var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(
                "ResetPassword", "Account",
                new { token, email = model.Email },
                protocol: HttpContext.Request.Scheme);

            var emailBody = $@"
                <h2>Recuperação de Password — LisAeroGest</h2>
                <p>Olá {user.FirstName},</p>
                <p>Recebemos um pedido para redefinir a sua password.</p>
                <p><a href='{resetLink}'>Clique aqui para redefinir a sua password</a></p>
                <p>Se não fez este pedido, ignore este email.</p>
                <br/>
                <p>LisAeroGest — Aeroporto de Lisboa</p>";

            _mailHelper.SendEmail(
                model.Email!,
                "Recuperação de Password — LisAeroGest",
                emailBody);

            ViewBag.Message = "Se este email estiver registado, receberá um link de recuperação.";
            return View("RecoverPasswordConfirmation");
        }

        // ─── Redefinição de Password ──────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de redefinição de password com o token do email.
        /// </summary>
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };
            return View(model);
        }

        /// <summary>
        /// Processa a redefinição de password com o token recebido por email.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email!);
            if (user == null)
            {
                ViewBag.Message = "Utilizador não encontrado.";
                return View(model);
            }

            var result = await _userHelper.ResetPasswordAsync(user, model.Token!, model.Password!);

            if (result.Succeeded)
            {
                ViewBag.Message = "Password redefinida com sucesso! Já pode fazer login.";
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ─── Alteração de Password ────────────────────────────────────────────

        /// <summary>
        /// Apresenta o formulário de alteração de password para utilizadores autenticados.
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
            => View();

        /// <summary>
        /// Processa a alteração de password do utilizador autenticado.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userHelper.ChangePasswordAsync(
                user, model.OldPassword!, model.NewPassword!);

            if (result.Succeeded)
            {
                ViewBag.Message = "Password alterada com sucesso!";
                return View("ChangePasswordConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ─── Acesso Negado ────────────────────────────────────────────────────

        /// <summary>
        /// Apresenta a página de acesso negado quando o utilizador
        /// tenta aceder a um recurso sem permissões suficientes.
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
            => View();
    }
}
