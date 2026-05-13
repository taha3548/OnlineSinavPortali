using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineExamPortal.API.Models;
using OnlineExamPortal.API.DTOs;
using OnlineExamPortal.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace OnlineExamPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
// [YÖNERGE UYUMLULUĞU] - Web API & Identity Kimlik Doğrulama Katmanı
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
    }

    // Account/Register (Identity Kayıt)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Bu e-posta adresi zaten kayıtlı." });

        AppUser user = new AppUser()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PasswordPlain = model.Password  // Yönerge: Şifrelerin admin panelinde görünebilmesi için yedeklenir.
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Kayıt tamamlanamadı.", Errors = result.Errors });

        // Default 'Student' Role Assignment
        if (!await _roleManager.RoleExistsAsync("Student"))
            await _roleManager.CreateAsync(new IdentityRole("Student"));

        await _userManager.AddToRoleAsync(user, "Student");

        return Ok(new { Message = "Kullanıcı başarıyla oluşturuldu." });
    }

    // Kullanıcı Girişi (Login) ve JWT Token Üretimi
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        var success = user != null && await _userManager.CheckPasswordAsync(user, model.Password);

        // Login Logging (Security Audit)
        var userRolesForLog = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
        var log = new LoginLog
        {
            Email = model.Email,
            FullName = user != null ? $"{user.FirstName} {user.LastName}" : "Bilinmiyor",
            Role = userRolesForLog.FirstOrDefault() ?? "Student",
            IsSuccessful = success,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            LoginDate = DateTime.Now
        };
        _context.LoginLogs.Add(log);
        await _context.SaveChangesAsync();

        if (success)
        {
            // JWT Claims (Identity Context)
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user!.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", $"{user.FirstName} {user.LastName}")
            };

            foreach (var userRole in userRolesForLog)
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));

            // [YÖNERGE UYUMLULUĞU] - JWT (JSON Web Token) Üretim Mantığı
            var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                role = userRolesForLog.FirstOrDefault() ?? "Student",
                fullName = $"{user.FirstName} {user.LastName}",
                userId = user.Id,
                avatar = user.Avatar
            });
        }

        return Unauthorized();
    }

    // JWT Token Yapılandırması (Secret Key, Issuer, Audience)
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(5), // Token süresi (Piyasa standardı)
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    [HttpGet("profile/{id}")]
    [Authorize]
    public async Task<IActionResult> GetProfile(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new ProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Avatar = user.Avatar
        });
    }

    [HttpPut("profile/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(string id, [FromBody] ProfileDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Avatar = model.Avatar;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
            return Ok(new { Message = "Profil başarıyla güncellendi.", FullName = $"{user.FirstName} {user.LastName}", Avatar = user.Avatar });

        return BadRequest(result.Errors);
    }
}
