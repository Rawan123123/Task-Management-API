using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Task_Management_Project.Controllers.Base;
using Task_Management_Project.DTOs;
using Task_Management_Project.DTOs.UserDTOs;
using Task_Management_Project.Exeptions;
using Task_Management_Project.Helpers;
using Task_Management_Project.Models;


namespace Task_Management_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly Context _context;
        public UserController(Context context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,TeamLeader")]
        [HttpGet] // get all users
        public async Task<IActionResult> GetAllUsers()
        {
            ValidateModel();
            var users =await _context.Users.Select(user => new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.RoleName,
                ProfileImgURL = user.ProfileImageUrl,
                IsActive = user.IsActive
            }).ToListAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpGet("Profile")] // get my profile
        public async Task<IActionResult> GetMyProfile() 
        {
            ValidateModel();
            int UserId = GetCurrentUserId();

            User currentUser =await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);

            if (currentUser == null)
            {
                throw new NotFoundException("Current user not found.");
            }

            return Ok(new UserResponseDTO
            {
                Id = UserId,
                Username = currentUser.Username,
                Email = currentUser.Email,
                RoleName = currentUser.RoleName,
                ProfileImgURL = currentUser.ProfileImageUrl,
                IsActive = currentUser.IsActive
            });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("Profile/{id}")] // get user by id
        public async Task<IActionResult> GetUserById(int id)
        {
            ValidateModel();
            User userFromDb =await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userFromDb == null) throw new NotFoundException($"User with ID {id} not found.");

            var userResponse = new UserResponseDTO
            {
                Id = userFromDb.Id,
                Username = userFromDb.Username,
                Email = userFromDb.Email,
                RoleName = userFromDb.RoleName,
                ProfileImgURL = userFromDb.ProfileImageUrl,
                IsActive = userFromDb.IsActive
            };
            return Ok(userResponse);
        }

        [Authorize]
        [HttpPut("Profile")] // update my profile
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileDTO profileFromRequest)
        {
            int userId = GetCurrentUserId();
            ValidateModel();
            User userFromDb =await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userFromDb == null) throw new NotFoundException($"User not found.");

            userFromDb.Username = profileFromRequest.UserName;

            if (!string.IsNullOrEmpty(profileFromRequest.ProfileImgUrl))
                userFromDb.ProfileImageUrl = profileFromRequest.ProfileImgUrl;

            await _context.SaveChangesAsync();

            return Ok(new UserResponseDTO
            {
                Id = userFromDb.Id,
                Username = userFromDb.Username,
                Email = userFromDb.Email,
                RoleName = userFromDb.RoleName,
                ProfileImgURL = userFromDb.ProfileImageUrl,
                IsActive = userFromDb.IsActive
            });

        }



        [Authorize(Roles = "Admin")] // update user by admin
        [HttpPut("Profile/{id}")]
        public async Task<IActionResult> UpdateUserByAdmin(int id, UpdateProfileDTO profileFromRequest)
        {
            ValidateModel();
            User userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userFromDb == null)
                throw new NotFoundException($"User with ID {id} not found.");

            userFromDb.Username = profileFromRequest.UserName;

            if (!string.IsNullOrEmpty(profileFromRequest.ProfileImgUrl))
                userFromDb.ProfileImageUrl = profileFromRequest.ProfileImgUrl;

            await _context.SaveChangesAsync();

            return Ok(new UserResponseDTO
            {
                Id = userFromDb.Id,
                Username = userFromDb.Username,
                Email = userFromDb.Email,
                RoleName = userFromDb.RoleName,
                ProfileImgURL = userFromDb.ProfileImageUrl,
                IsActive = userFromDb.IsActive
            });
        }


        [Authorize]
        [HttpPut("ChangePassword")] // change my password
        public async Task<IActionResult> ChangeMyPassword(ChangePasswordDTO dto)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            User userFromDb =await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userFromDb == null) throw new NotFoundException($"User with ID {userId} not found.");

            var passwordVerification = PasswordHasher.VerifyPassword(dto.CurrentPassword, userFromDb.PasswordHash);

            if (!passwordVerification)
            {
                return BadRequest("Current password is incorrect");
            }
            string newPasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            userFromDb.PasswordHash = newPasswordHash;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Password changed successfully" });
        }


        [Authorize(Roles = "Admin,TeamLeader")]
        [HttpDelete("{id}")] // delete user by id
        public async Task<IActionResult> DeleteUserById(int id)
        {
            ValidateModel();
            User userFromDb =await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userFromDb == null) throw new NotFoundException($"User with ID {id} not found.");

            _context.Users.Remove(userFromDb);
             await _context.SaveChangesAsync();
            return Ok(new { Message = $"User with ID {id} has been deleted." });
        }
    }

}
