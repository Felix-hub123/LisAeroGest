using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Mvc;

namespace BilheticaAeronauticaWeb.Controllers
{
    /// <summary>
    /// Controller responsável pela autenticação, registo e recuperação de conta.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IPassengerRepository _passengerRepository;

        public AccountController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConverterHelper converterHelper,
            IPassengerRepository passengerRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _converterHelper = converterHelper;
            _passengerRepository = passengerRepository;
        }

        // ─── Login ───────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login()
        {
            // Se já estiver autenticado, vai direto para a área principal
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

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

                if (await _userHelper.IsUserInRoleAsync(loggedUser!, "Admin") ||
                    await _userHelper.IsUserInRoleAsync(loggedUser!, "Employee"))
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Conta bloqueada temporariamente. Tente novamente em 15 minutos.");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Confirme o seu email antes de fazer login.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email ou password incorretos.");
            return View(model);
        }

        // ─── Logout ──────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Login");
        }

        // ─── Registo ─────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Validar se o email já existe
            var existingUser = await _userHelper.GetUserByEmailAsync(model.Username!);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Este email já está registado.");
                return View(model);
            }

            // 2. Usar o ConverterHelper para criar a entidade User
            var user = _converterHelper.ToUser(model);

            var result = await _userHelper.AddUserAsync(user, model.Password!);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            // 3. Atribuir a role "Passenger"
            await _userHelper.AddUserToRoleAsync(user, "Passenger");

            // 4. Usar o ConverterHelper para criar a entidade Passageiro associada
            var passenger = _converterHelper.ToPassenger(model, user.Id);
            await _passengerRepository.AddAsync(passenger);
            await _passengerRepository.SaveAsync();

            // 5. Gerar token e enviar email de confirmação
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
                <br/>
                <p>LisAeroGest — Aeroporto de Lisboa</p>";

            var response = _mailHelper.SendEmail(model.Username!, "Confirmação de Email — LisAeroGest", emailBody);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Utilizador criado, mas não foi possível enviar o email de confirmação.");
                return View(model);
            }

            ViewBag.Message = "Registo efetuado com sucesso! Verifique o seu email para confirmar a conta.";
            return View("RegisterConfirmation");
        }

        // ─── Confirmação de Email ─────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userHelper.ConfirmEmailAsync(user, token);

            ViewBag.Message = result.Succeeded
                ? "Email confirmado com sucesso! Já pode fazer login."
                : "Erro ao confirmar email. O link pode ter expirado.";

            return View();
        }

        // ─── Recuperação de Password ──────────────────────────────────────────

        [HttpGet]
        public IActionResult RecoverPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.Email!);

            // Medida de segurança: não revelar se o email existe na base de dados
            if (user != null)
            {
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
                    <br/>
                    <p>LisAeroGest — Aeroporto de Lisboa</p>";

                _mailHelper.SendEmail(model.Email!, "Recuperação de Password — LisAeroGest", emailBody);
            }

            ViewBag.Message = "Se este email estiver registado, receberá um link de recuperação.";
            return View("RecoverPasswordConfirmation");
        }

        // ─── Redefinição de Password ──────────────────────────────────────────

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

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
    }
}