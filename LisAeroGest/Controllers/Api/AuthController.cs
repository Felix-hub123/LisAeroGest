using LisAeroGest.Data.Entities;
using LisAeroGest.Data.Interfaces;
using LisAeroGest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LisAeroGest.Controllers.Api
{
    /// <summary>
    /// API REST para autenticação mobile com JWT.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IPassengerRepository _passengerRepository;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
             IPassengerRepository passengerRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _passengerRepository = passengerRepository;
        }

        /// <summary>
        /// Autentica um utilizador e devolve um token JWT.
        /// POST: api/auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(new { message = "Email ou password incorretos." });

            if (!user.EmailConfirmed)
                return Unauthorized(new { message = "Email não confirmado." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Email ou password incorretos." });

            var roles = await _userManager.GetRolesAsync(user);
            var expiration = DateTime.UtcNow.AddDays(7);
            var token = GenerateJwtToken(user, roles, expiration);

            return Ok(new
            {
                token,
                expiration,
                user.FullName,
                user.Email,
                roles
            });
        }


        /// <summary>
        /// Regista um novo utilizador (passageiro) e devolve um token JWT.
        /// POST: api/auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Verificar se o email já existe
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Este email já está registado." });

            // 2. Criar o utilizador
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return BadRequest(ModelState);
            }

            // 3. Atribuir a role "Passenger"
            await _userManager.AddToRoleAsync(user, "Passenger");

            // 🔥 4. CRIAR O PASSENGER ASSOCIADO
            var existingPassenger = await _passengerRepository.GetByEmailAsync(request.Email);
            if (existingPassenger == null)
            {
                var passenger = new Passenger
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    DocumentNumber = request.DocumentNumber,
                    DocumentType = request.DocumentType ?? "CC",
                    UserId = user.Id,
                    RegistrationDate = DateTime.UtcNow
                };

                await _passengerRepository.AddAsync(passenger);
                await _passengerRepository.SaveAsync();
            }
            else if (string.IsNullOrEmpty(existingPassenger.UserId))
            {
                // Se já existia como convidado, associar ao novo utilizador
                existingPassenger.UserId = user.Id;
                existingPassenger.FirstName = request.FirstName;
                existingPassenger.LastName = request.LastName;
                existingPassenger.DocumentNumber = request.DocumentNumber;
                await _passengerRepository.UpdateAsync(existingPassenger);
                await _passengerRepository.SaveAsync();
            }

            // 5. Gerar JWT
            var roles = await _userManager.GetRolesAsync(user);
            var expiration = DateTime.UtcNow.AddDays(7);
            var token = GenerateJwtToken(user, roles, expiration);

            return Ok(new
            {
                token,
                expiration,
                user.FullName,
                user.Email,
                roles,
                message = "Registo efetuado com sucesso."
            });
        }

        /// <summary>
        /// Gera um token JWT para o utilizador.
        /// </summary>
        private string GenerateJwtToken(User user, IList<string> roles, DateTime expiration)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}