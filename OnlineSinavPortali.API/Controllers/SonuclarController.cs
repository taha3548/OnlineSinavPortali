using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavPortali.API.Models;
using OnlineSinavPortali.API.Repositories;
using System.Security.Claims;

namespace OnlineSinavPortali.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SonuclarController : ControllerBase
{
    private readonly ISonucRepository _sonucRepository;

    public SonuclarController(ISonucRepository sonucRepository)
    {
        _sonucRepository = sonucRepository;
    }

    // Admin: Tüm öğrencilerin sonuçları
    [HttpGet("hepsi")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> GetHepsi()
    {
        var sonuclar = await _sonucRepository.GetAllWithRelationsAsync();

        var result = sonuclar.Select(s => new
        {
            sonucId = s.SonucId,
            kullaniciAdi = s.Kullanici != null ? $"{s.Kullanici.Ad} {s.Kullanici.Soyad}" : "Bilinmiyor",
            kullaniciEmail = s.Kullanici?.Email ?? "—",
            sinavBaslik = s.Sinav?.Baslik ?? "—",
            puan = s.Puan,
            dogruSayisi = s.DogruSayisi,
            yanlisSayisi = s.YanlisSayisi,
            katilimTarihi = s.KatilimTarihi
        });

        return Ok(result);
    }

    // Liderlik Tablosu: Tüm sınavlardan en çok puan toplayan ilk 10 öğrenci
    [HttpGet("liderlik")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLiderlikTablosu()
    {
        var liderler = await _sonucRepository.GetLiderlikTablosuAsync();
        return Ok(liderler);
    }

    [HttpGet("istatistikler")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> GetIstatistikler()
    {
        var stats = await _sonucRepository.GetIstatistiklerAsync();
        return Ok(stats);
    }

    [HttpGet("kullanici/{kullaniciId}")]
    public async Task<IActionResult> GetKullaniciSonuclari(string kullaniciId)
    {
        // [YETKİ KONTROLÜ] - Ogrenci -> Kendi Karnesi, Admin -> Her Şey
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        if (currentUserRole != "Admin" && currentUserRole != "SiteYoneticisi" && currentUserId != kullaniciId)
            return Forbid();

        var sonuclar = await _sonucRepository.GetByKullaniciIdAsync(kullaniciId);
        return Ok(sonuclar);
    }

    // [YÖNERGE UYUMLULUĞU] - Dinamik Sınav Puanı Hesaplama Mantığı (POST)
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Sonuc sonuc)
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
            sonuc.KullaniciId = userId;

        // Aynı kullanıcı aynı sınava tekrar giremez
        var dahaOnceGirdiMi = await _sonucRepository.HasUserTakenSinavAsync(sonuc.KullaniciId!, sonuc.SinavId);
        if (dahaOnceGirdiMi)
            return BadRequest(new { Mesaj = "Bu sınava zaten katıldınız. Tekrar giriş yapamazsınız." });

        ModelState.Remove("Kullanici");
        ModelState.Remove("Sinav");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _sonucRepository.AddAsync(sonuc);

        return Ok(new { Mesaj = "Sonuç başarıyla kaydedildi.", Puan = sonuc.Puan });
    }

    // Kullanıcının daha önce girdiği sınav ID'lerini döner
    [HttpGet("girilensinavlar/{kullaniciId}")]
    public async Task<IActionResult> GetGirilenSinavlar(string kullaniciId)
    {
        var sinavIdler = await _sonucRepository.GetGirilenSinavIdleriAsync(kullaniciId);
        return Ok(sinavIdler);
    }

    // Admin Yetkileri: Sonuç Sıfırlama (Silme)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _sonucRepository.DeleteAsync(id);
        if (!result) return NotFound(new { Mesaj = "Sonuç bulunamadı." });

        return Ok(new { Mesaj = "Öğrenci sınav sonucu başarıyla sıfırlandı. Artık sınava tekrar girebilir." });
    }

    // Admin Yetkileri: Puan Müdahalesi (Güncelleme)
    [HttpPut("{id}/puan")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> UpdateScore(int id, [FromBody] PuanGuncelleDTO dto)
    {
        var result = await _sonucRepository.UpdateScoreAsync(id, dto.YeniPuan);
        if (!result) return NotFound(new { Mesaj = "Sonuç bulunamadı." });

        return Ok(new { Mesaj = "Öğrenci puanı başarıyla güncellendi." });
    }

    // [YÖNERGE UYUMLULUĞU] - Manuel Puan Girişi ve Bonus Sistemi
    [HttpPost("manuel")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> ManuelPuanGiris([FromBody] ManuelPuanDTO model)
    {
        var sonuclar = await _sonucRepository.GetByKullaniciIdAsync(model.KullaniciId);
        var mevcutSonuc = sonuclar.FirstOrDefault(s => s.SinavId == model.SinavId);

        if (mevcutSonuc != null)
        {
            // Güncelleme
            mevcutSonuc.Puan = model.Puan;
            mevcutSonuc.KatilimTarihi = DateTime.Now;
            await _sonucRepository.UpdateScoreAsync(mevcutSonuc.SonucId, model.Puan);
            return Ok(new { Mesaj = "Öğrencinin mevcut puanı başarıyla güncellendi." });
        }
        else
        {
            // Yeni Kayıt
            var yeniSonuc = new Sonuc
            {
                KullaniciId = model.KullaniciId,
                SinavId = model.SinavId,
                Puan = model.Puan,
                DogruSayisi = 0, // Manuel girildiği için detaylar 0
                YanlisSayisi = 0,
                KatilimTarihi = DateTime.Now
            };
            await _sonucRepository.AddAsync(yeniSonuc);
            return Ok(new { Mesaj = "Öğrenciye sınav puanı başarıyla tanımlandı." });
        }
    }
}

public class ManuelPuanDTO
{
    public string KullaniciId { get; set; } = string.Empty;
    public int SinavId { get; set; }
    public int Puan { get; set; }
}

public class PuanGuncelleDTO
{
    public int YeniPuan { get; set; }
}
