using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Management_Project.Controllers.Base;
using Task_Management_Project.DTOs.AuthDTOs;
using Task_Management_Project.Helpers;
using Task_Management_Project.Models;

namespace Task_Management_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly Context _context;
        private readonly JWTService _jWTService;

        public AuthController(Context context, JWTService jWTService)
        {
            _context = context;
            _jWTService = jWTService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO userFromRequestDto)
        {
            ValidateModel();

            User existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == userFromRequestDto.Email);
            if (existing != null)
            {
                return BadRequest("User with this email already exists.");
            }

            string hashed = PasswordHasher.HashPassword(userFromRequestDto.Password);
            User user = new User
            {
                Username = userFromRequestDto.UserName,
                Email = userFromRequestDto.Email,
                PasswordHash = hashed,
                RoleName = "User",
                ProfileImageUrl = "default.png"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.Id, user.Username, user.Email, user.RoleName, user.ProfileImageUrl });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            bool isPasswordValid = PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized("Invalid email or password.");
            }
            string token = _jWTService.CreateToken(user);
            return Ok(new { user.Id, user.Username, user.Email, user.RoleName, user.ProfileImageUrl, token });
        }

    }
}
