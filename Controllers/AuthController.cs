using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TestApi.DTOS;
using TestApi.Models;
using TestApi.Repositories;

namespace TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenRepository _tokenRepository;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenRepository tokenRepository)
        {
            _signInManager = signInManager;
            _tokenRepository = tokenRepository;
            _userManager = userManager; 
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto  loginrequestdto)
        {
            var user = await _userManager.FindByEmailAsync(loginrequestdto.Email);

            var roles = await _userManager.GetRolesAsync(user);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginrequestdto.Password))
            {
                var token = _tokenRepository.CreateToken(user, roles.ToList());

                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                });

                return Ok("Logged in.");
            }

            return Unauthorized("Invalid credentials.");
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            var user = new AppUser
            {
                UserName = registerRequestDto.UserName,
                Email = registerRequestDto.Email
            };
            var result = await _userManager.CreateAsync(user, registerRequestDto.Password);
            if (result.Succeeded)
            {
                if (registerRequestDto.Roles != null && registerRequestDto.Roles.Any())
                {
                    result = await _userManager.AddToRolesAsync(user, registerRequestDto.Roles);

                    if (result.Succeeded)
                    {
                        var token = _tokenRepository.CreateToken(user, registerRequestDto.Roles.ToList());
                        Response.Cookies.Append("access_token", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddMinutes(15)
                        });
                        return Ok("User was registered! Please login.");
                    }
                }
            }
            return BadRequest("User registration failed.");
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return Ok("Logged out.");
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userId2 = User.
          
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found", userId });

            return Ok(new
            {
                user.UserName,
                user.Email,
                user.Id
            });
        }
    }
}
