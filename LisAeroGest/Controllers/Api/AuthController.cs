using LisAeroGest.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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

        /// <summary>
        /// Inicializa o AuthController com as dependências necessárias.
        /// </summary>
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Autentica um utilizador e devolve um token JWT.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user == null)
                return Unauthorized(new { message = "Email ou password incorretos." });

            if (!user.EmailConfirmed)
                return Unauthorized(new { message = "Email não confirmado." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, false);
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
