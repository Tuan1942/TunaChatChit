using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TunaChatChit.Context;
using TunaChatChit.Models;

namespace TunaChatChit.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ChatContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(ChatContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]AccountInput account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (await _context.Accounts.AnyAsync(a => a.Username == account.Username))
            {
                return BadRequest("Tên người dùng đã tồn tại.");
            }

            var acc = new Account
            {
                Username = account.Username,
                PasswordHash = HashPassword(account.Password)
            };

            _context.Accounts.Add(acc);
            await _context.SaveChangesAsync();

            var accountRole = new AccountRole
            {
                UserId = acc.Id,
                RoleId = 2
            };

            _context.AccountRoles.Add(accountRole);
            await _context.SaveChangesAsync();

            return Ok(acc);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountInput account)
        {
            var dbAcc = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == account.Username);
            if (dbAcc == null || !VerifyPassword(account.Password, dbAcc.PasswordHash))
            {
                return Unauthorized("Thông tin không hợp lệ.");
            }

            var token = GenerateJwtToken(dbAcc, _configuration);
            return Ok(token);
        }

        public string AddCookie(Account account, IConfiguration configuration)
        {
            var token = GenerateJwtToken(account, configuration);
            HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax
            });
            return token;
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null)
            {
                return BadRequest("Token không hợp lệ.");
            }
            HttpContext.Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = HttpContext.User.Identity.Name;

            var roles = HttpContext.User.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();

            return Ok(new
            {
                Id = userId,
                Username = username,
                Roles = roles
            });
        }
        [HttpGet("admin-only")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAdminOnlyData()
        {
            return Ok(new { Message = "This data is accessible only by admin." });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHash;
        }

        public string GenerateJwtToken(Account account, IConfiguration configuration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Username)
            };
            var userRoles = GetUserRoles(account.Id);

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(16),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private List<string> GetUserRoles(int userId)
        {
            var roles = new List<string>();
            var userRoles = _context.AccountRoles.Where(ur => ur.UserId == userId).ToList();
            foreach (var userRole in userRoles)
            {
                roles.Add(_context.Roles.First(r => r.Id == userRole.RoleId).RoleName);
            }
            return roles;
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> GetListUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userList = await _context.Users
                .Where(u => u.AccountId != int.Parse(userIdClaim))
                .Select(u => new {
                    u.Id,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();
            return Ok(userList);
        }
    }
}
