using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSinavPortali.API.Data;
using OnlineSinavPortali.API.Models;
using System.Security.Claims;

namespace OnlineSinavPortali.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SiteYoneticisi")]
public class KullanicilarController : ControllerBase
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly ApplicationDbContext _context;

    public KullanicilarController(UserManager<Kullanici> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var tempResult = new List<dynamic>(); // Sıralama için geçici liste
        var isteyenRol = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRol = roles.FirstOrDefault() ?? "Ogrenci";

            // Sıralama Önceliği Belirleme: SiteYoneticisi(1) > Admin(2) > Ogrenci(3)
            int siraOnceligi = userRol switch
            {
                "SiteYoneticisi" => 1,
                "Admin" => 2,
                _ => 3
            };

            // Şifreyi sadece yetkili roller görebilir:
            bool sifreGoster = false;
            if (isteyenRol == "SiteYoneticisi") sifreGoster = true;
            else if (isteyenRol == "Admin" && userRol == "Ogrenci") sifreGoster = true;

            tempResult.Add(new
            {
                siraOnceligi = siraOnceligi,
                data = new
                {
                    id = user.Id,
                    adSoyad = $"{user.Ad} {user.Soyad}",
                    email = user.Email,
                    rol = userRol,
                    sifre = sifreGoster ? (string.IsNullOrEmpty(user.SifreDuz) ? "—" : user.SifreDuz) : "••••••••",
                    kayitTarihi = user.KayitTarihi,
                    bonusPuan = user.BonusPuan
                }
            });
        }

        // Hiyerarşiye göre sıralayıp (OrderBy) sadece asıl datayı (Select) döndürüyoruz.
        var finalResult = tempResult.OrderBy(x => x.siraOnceligi).Select(x => x.data).ToList();

        return Ok(finalResult);
    }

    [HttpGet("loglar")]
    public async Task<IActionResult> GetLoglar()
    {
        var loglar = await _context.GirisLoglari
            .OrderByDescending(l => l.GirisTarihi)
            .Take(200)
            .ToListAsync();
        return Ok(loglar);
    }

    // Kullanıcı silme
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var hedef = await _userManager.FindByIdAsync(id);
        if (hedef == null) return NotFound();

        var hedefRol = (await _userManager.GetRolesAsync(hedef)).FirstOrDefault() ?? "Ogrenci";
        var isteyenRol = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";

        if (hedefRol == "SiteYoneticisi")
            return BadRequest(new { Mesaj = "Site Yöneticisi hesabı silinemez." });

        if (isteyenRol == "Admin" && hedefRol == "Admin")
            return StatusCode(403, new { Mesaj = "Adminler birbirini silemez." });

        var result = await _userManager.DeleteAsync(hedef);
        if (!result.Succeeded)
            return StatusCode(500, new { Mesaj = "Silme işlemi başarısız." });

        return Ok(new { Mesaj = "Kullanıcı silindi." });
    }

    // Rol değiştirme — SADECE SiteYoneticisi
    [HttpPut("{id}/rol")]
    [Authorize(Roles = "SiteYoneticisi")]
    public async Task<IActionResult> RolDegistir(string id, [FromBody] RolDegistirDTO dto)
    {
        var hedef = await _userManager.FindByIdAsync(id);
        if (hedef == null) return NotFound();

        var mevcutRoller = await _userManager.GetRolesAsync(hedef);

        if (mevcutRoller.Contains("SiteYoneticisi"))
            return BadRequest(new { Mesaj = "Site Yöneticisinin rolü değiştirilemez." });

        if (dto.YeniRol != "Admin" && dto.YeniRol != "Ogrenci")
            return BadRequest(new { Mesaj = "Geçersiz rol." });

        await _userManager.RemoveFromRolesAsync(hedef, mevcutRoller);
        await _userManager.AddToRoleAsync(hedef, dto.YeniRol);

        return Ok(new { Mesaj = $"Rol '{dto.YeniRol}' olarak güncellendi." });
    }

    // [YÖNERGE UYUMLULUĞU] - Genel Bonus Puan (Sınavsız Kanaat Notu) Sistemi
    [HttpPut("{id}/bonus-puan")]
    public async Task<IActionResult> UpdateBonusPuan(string id, [FromBody] BonusPuanDTO dto)
    {
        var hedef = await _userManager.FindByIdAsync(id);
        if (hedef == null) return NotFound();

        hedef.BonusPuan = dto.YeniPuan;
        var result = await _userManager.UpdateAsync(hedef);
        if (!result.Succeeded)
            return StatusCode(500, new { Mesaj = "Bonus puan güncellenemedi." });

        return Ok(new { Mesaj = "Bonus puan başarıyla tanımlandı." });
    }
}

public class BonusPuanDTO
{
    public int YeniPuan { get; set; }
}

public class RolDegistirDTO
{
    public string YeniRol { get; set; } = string.Empty;
}
