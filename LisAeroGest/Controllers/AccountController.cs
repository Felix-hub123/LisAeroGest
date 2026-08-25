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
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null!)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                   
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError(string.Empty, "Login inválido.");
            }
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

            // ================================================================
            // 4. 🔥 VERIFICAR SE EXISTE PASSAGEIRO CONVIDADO COM ESTE EMAIL
            // ================================================================
            var existingPassenger = await _passengerRepository.GetByEmailAsync(model.Username!);

            if (existingPassenger != null && string.IsNullOrEmpty(existingPassenger.UserId))
            {
                // ✅ Associar o passageiro convidado ao novo utilizador
                existingPassenger.UserId = user.Id;
                existingPassenger.FirstName = model.FirstName ?? existingPassenger.FirstName;
                existingPassenger.LastName = model.LastName ?? existingPassenger.LastName;
                existingPassenger.DocumentNumber = model.DocumentNumber ?? existingPassenger.DocumentNumber;
                existingPassenger.DocumentType = model.DocumentType ?? existingPassenger.DocumentType;

                await _passengerRepository.UpdateAsync(existingPassenger);
                await _passengerRepository.SaveAsync();

                // Log (opcional)
                // _logger.LogInformation("Passageiro convidado {Email} associado ao utilizador {UserId}", model.Username, user.Id);
            }
            else if (existingPassenger == null)
            {
                // ✅ Se não existir passageiro, criar novo (utilizador normal)
                var passenger = _converterHelper.ToPassenger(model, user.Id);
                await _passengerRepository.AddAsync(passenger);
                await _passengerRepository.SaveAsync();
            }
            // Se existingPassenger != null e já tem UserId, não faz nada (já está associado)

            // ================================================================
            // 5. Gerar token e enviar email de confirmação
            // ================================================================
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

            var response = await _mailHelper.SendEmailAsync(model.Username!, "Confirmação de Email — LisAeroGest", emailBody);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Utilizador criado, mas não foi possível enviar o email de confirmação.");
                return View(model);
            }

            // ================================================================
            // 6. Verificar se o utilizador veio do checkout convidado
            // ================================================================
            var pendingEmail = HttpContext.Session.GetString("PendingRegistration");
            if (!string.IsNullOrEmpty(pendingEmail) && pendingEmail == model.Username)
            {
                // Limpar a sessão
                HttpContext.Session.Remove("PendingRegistration");

                // Redirecionar para a página de bilhetes com mensagem de boas-vindas
                TempData["Success"] = "Conta criada com sucesso! Os seus bilhetes foram associados à sua conta.";
                return RedirectToAction("MyTickets", "Shop");
            }

            ViewBag.Message = "Registo efetuado com sucesso! Verifique o seu email para confirmar a conta.";
            return View("RegisterConfirmation");
        }

        // ─── Confirmação de Email ─────────────────────────────────────────────


        /// <summary>
        /// Procura reservas de convidado associadas ao e-mail do utilizador
        /// e associa-as ao perfil de passageiro recém-autenticado/registado.
        /// </summary>
        private async Task ClaimGuestTicketsAsync(string email, string userId)
        {
            var guestPassenger = await _passengerRepository.GetByEmailAsync(email);

            if (guestPassenger != null)
            {
                // 1. Associa o UserId do novo utilizador ao passageiro
                guestPassenger.UserId = userId;
                await _passengerRepository.UpdateAsync(guestPassenger);
                await _passengerRepository.SaveAsync();
            }
        }

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
            {
               await ClaimGuestTicketsAsync(user.Email!, user.Id);

                ViewBag.Message = "Email confirmado com sucesso! Os seus bilhetes anteriores, se existirem, já estão associados à sua conta. Já pode fazer login.";
            }
            else
            {
                ViewBag.Message = "Erro ao confirmar email. O link pode ter expirado.";
            }

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

               
                await _mailHelper.SendEmailAsync(model.Email!, "Recuperação de Password — LisAeroGest", emailBody);
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