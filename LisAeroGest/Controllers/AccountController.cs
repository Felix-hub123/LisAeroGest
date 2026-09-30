using LisAeroGest.Data.Interfaces;
using LisAeroGest.Helpers;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IImageHelper _imageHelper;

        public AccountController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConverterHelper converterHelper,
            IPassengerRepository passengerRepository,
            IImageHelper imageHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _converterHelper = converterHelper;
            _passengerRepository = passengerRepository;
            _imageHelper = imageHelper;
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
            return RedirectToAction("Index", "Home");
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
            // ================================================================
            // 1. VALIDAR FORMULÁRIO
            // ================================================================

            if (!ModelState.IsValid)
                return View(model);


            // ================================================================
            // 2. VERIFICAR SE O EMAIL JÁ EXISTE
            // ================================================================

            var existingUser =
                await _userHelper.GetUserByEmailAsync(
                    model.Username!);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Este email já está registado.");

                return View(model);
            }


            // ================================================================
            // 3. CRIAR UTILIZADOR
            // ================================================================

            var user =
                _converterHelper.ToUser(model);

            var result =
                await _userHelper.AddUserAsync(
                    user,
                    model.Password!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }


            // ================================================================
            // 4. ATRIBUIR ROLE PASSENGER
            // ================================================================

            await _userHelper.AddUserToRoleAsync(
                user,
                "Passenger");


            // ================================================================
            // 5. CRIAR OU ASSOCIAR PASSAGEIRO
            // ================================================================

            var existingPassenger =
                await _passengerRepository.GetByEmailAsync(
                    model.Username!);


            // Passageiro convidado já existente
            if (existingPassenger != null &&
                string.IsNullOrEmpty(existingPassenger.UserId))
            {
                existingPassenger.UserId =
                    user.Id;

                existingPassenger.FirstName =
                    model.FirstName
                    ?? existingPassenger.FirstName;

                existingPassenger.LastName =
                    model.LastName
                    ?? existingPassenger.LastName;

                existingPassenger.DocumentNumber =
                    model.DocumentNumber
                    ?? existingPassenger.DocumentNumber;

                existingPassenger.DocumentType =
                    model.DocumentType
                    ?? existingPassenger.DocumentType;


                await _passengerRepository.UpdateAsync(
                    existingPassenger);

                await _passengerRepository.SaveAsync();
            }

            // Passageiro ainda não existe
            else if (existingPassenger == null)
            {
                var passenger =
                    _converterHelper.ToPassenger(
                        model,
                        user.Id);

                await _passengerRepository.AddAsync(
                    passenger);

                await _passengerRepository.SaveAsync();
            }


            // ================================================================
            // 6. GERAR LINK DE CONFIRMAÇÃO
            // ================================================================

            var token =
                await _userHelper
                    .GenerateEmailConfirmationTokenAsync(
                        user);


            var confirmationLink =
                Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new
                    {
                        userId = user.Id,
                        token
                    },
                    protocol:
                        HttpContext.Request.Scheme);


            // ================================================================
            // 7. EMAIL DE CONFIRMAÇÃO
            // ================================================================

            var emailBody = $@"
        <div style='font-family:Arial,sans-serif;
                    max-width:600px;
                    margin:auto;
                    padding:30px;'>

            <h2 style='color:#1F5C99;'>
                Bem-vindo ao LisAeroGest!
            </h2>

            <p>
                Olá {user.FirstName},
            </p>

            <p>
                Obrigado por se registar no LisAeroGest.
            </p>

            <p>
                Para ativar a sua conta,
                confirme o seu endereço de email.
            </p>

            <p style='margin:30px 0;'>

                <a href='{confirmationLink}'
                   style='
                       background:#c8e629;
                       color:#10151f;
                       padding:12px 24px;
                       border-radius:8px;
                       text-decoration:none;
                       font-weight:bold;'>

                    Confirmar Email

                </a>

            </p>

            <p style='color:#666;font-size:13px;'>
                Se não criou esta conta,
                pode ignorar esta mensagem.
            </p>

            <hr style='border:0;
                       border-top:1px solid #ddd;
                       margin:25px 0;' />

            <p style='color:#777;font-size:12px;'>
                LisAeroGest — Aeroporto de Lisboa
            </p>

        </div>";


            var emailResponse =
                await _mailHelper.SendEmailAsync(
                    model.Username!,
                    "Confirmação de Email — LisAeroGest",
                    emailBody);


            // ================================================================
            // 8. LIMPAR EVENTUAL REGISTO PENDENTE
            // ================================================================

            var pendingEmail =
                HttpContext.Session.GetString(
                    "PendingRegistration");

            if (!string.IsNullOrEmpty(pendingEmail) &&
                string.Equals(
                    pendingEmail,
                    model.Username,
                    StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Session.Remove(
                    "PendingRegistration");
            }


            // ================================================================
            // 9. IR SEMPRE PARA A PÁGINA DE CONFIRMAÇÃO
            // ================================================================

            ViewBag.Email =
                model.Username;


            if (emailResponse.IsSuccess)
            {
                ViewBag.EmailSent =
                    true;

                ViewBag.Message =
                    "A sua conta foi criada com sucesso. " +
                    "Enviámos um email de confirmação para o endereço indicado.";
            }
            else
            {
                ViewBag.EmailSent =
                    false;

                ViewBag.Message =
                    "A sua conta foi criada, mas não foi possível enviar " +
                    "o email de confirmação neste momento.";
            }


            return View(
                "RegisterConfirmation");
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

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (user == null)
                return RedirectToAction(nameof(Login));

            var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            return View("ChangePasswordSuccess");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (user == null)
                return RedirectToAction(nameof(Login));

            return View(new EditProfileViewModel
            {
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                PhoneNumber = user.PhoneNumber,
                Email = user.Email ?? "",
                ImageId = user.ImageId,
                ImageUrl = _imageHelper.GetImageUrl(user.ImageId, "users")
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity!.Name!);
            if (user == null)
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? "";
                model.ImageId = user.ImageId;
                model.ImageUrl = _imageHelper.GetImageUrl(user.ImageId, "users");
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                if (user.ImageId != Guid.Empty)
                    await _imageHelper.DeleteImageAsync(user.ImageId, "users");

                user.ImageId = await _imageHelper.UploadImageAsync(model.ImageFile, "users");
            }

            await _userHelper.UpdateUserAsync(user);
            TempData["Success"] = "Perfil atualizado.";
            return RedirectToAction(nameof(EditProfile));
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