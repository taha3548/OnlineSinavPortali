using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineSinavPortali.API.Models;
using OnlineSinavPortali.API.DTOs;
using OnlineSinavPortali.API.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace OnlineSinavPortali.API.Controllers;

[Route("api/[controller]")]
[ApiController]
// [YÖNERGE UYUMLULUĞU] - Web API & Identity Kimlik Doğrulama Katmanı
public class KimlikDogrulamaController : ControllerBase
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public KimlikDogrulamaController(UserManager<Kullanici> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
    }

    // Account/Register (Identity Kayıt)
    [HttpPost("kayit")]
    public async Task<IActionResult> KayitOl([FromBody] KayitDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
            return StatusCode(StatusCodes.Status500InternalServerError, new { Mesaj = "Kullanıcı zaten mevcut!" });

        Kullanici user = new Kullanici()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Email,
            Ad = model.Ad,
            Soyad = model.Soyad,
            SifreDuz = model.Sifre  // Yönerge: Şifrelerin admin panelinde görünebilmesi için yedeklenir.
        };
        var result = await _userManager.CreateAsync(user, model.Sifre);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError, new { Mesaj = "Kayıt işlemi başarısız.", Hatalar = result.Errors });

        // Varsayılan 'Ogrenci' Rol Ataması
        if (!await _roleManager.RoleExistsAsync("Ogrenci"))
            await _roleManager.CreateAsync(new IdentityRole("Ogrenci"));

        await _userManager.AddToRoleAsync(user, "Ogrenci");

        return Ok(new { Mesaj = "Kullanıcı başarıyla oluşturuldu!" });
    }

    // Kullanıcı Girişi (Login) ve JWT Token Üretimi
    [HttpPost("giris")]
    public async Task<IActionResult> GirisYap([FromBody] GirisDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        var basarili = user != null && await _userManager.CheckPasswordAsync(user, model.Sifre);

        // Giriş Loglama (Security Audit)
        // Başarılı/Başarısız tüm giriş denemeleri kaydedilir.
        var userRolesForLog = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
        var log = new GirisLogu
        {
            Email = model.Email,
            AdSoyad = user != null ? $"{user.Ad} {user.Soyad}" : "Bilinmiyor",
            Rol = userRolesForLog.FirstOrDefault() ?? "Ogrenci",
            Basarili = basarili,
            IpAdresi = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            GirisTarihi = DateTime.Now
        };
        _context.GirisLoglari.Add(log);
        await _context.SaveChangesAsync();

        if (basarili)
        {
            // JWT Claims (Identity Context)
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user!.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("AdSoyad", $"{user.Ad} {user.Soyad}")
            };

            foreach (var userRole in userRolesForLog)
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));

            // [YÖNERGE UYUMLULUĞU] - JWT (JSON Web Token) Üretim Mantığı
            var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                gecerlilikTarihi = token.ValidTo,
                rol = userRolesForLog.FirstOrDefault() ?? "Ogrenci",
                adSoyad = $"{user.Ad} {user.Soyad}",
                kullaniciId = user.Id,
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

    [HttpGet("profil/{id}")]
    [Authorize]
    public async Task<IActionResult> GetProfil(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new ProfilDTO
        {
            Ad = user.Ad,
            Soyad = user.Soyad,
            Avatar = user.Avatar
        });
    }

    [HttpPut("profil/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProfil(string id, [FromBody] ProfilDTO model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.Ad = model.Ad;
        user.Soyad = model.Soyad;
        user.Avatar = model.Avatar;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
            return Ok(new { Mesaj = "Profil başarıyla güncellendi.", AdSoyad = $"{user.Ad} {user.Soyad}", Avatar = user.Avatar });

        return BadRequest(result.Errors);
    }
}
