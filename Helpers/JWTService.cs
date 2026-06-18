using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Task_Management_Project.Models;

namespace Task_Management_Project.Helpers
{
    public class JWTService
    {
        private readonly IConfiguration _config;
        public JWTService(IConfiguration config)
        {
            _config = config;
        }
        public string CreateToken(User user)
        {
            List<Claim> userClaim = new List<Claim>();
            userClaim.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
            userClaim.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            userClaim.Add(new Claim(ClaimTypes.Role, user.RoleName));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config["JWT:Issuer"],
                audience: _config["JWT:Audience"],
                claims: userClaim,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
