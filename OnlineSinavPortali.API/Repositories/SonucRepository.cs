using Microsoft.EntityFrameworkCore;
using OnlineSinavPortali.API.Data;
using OnlineSinavPortali.API.Models;
using OnlineSinavPortali.API.DTOs;

namespace OnlineSinavPortali.API.Repositories;

// Repository Pattern Uygulaması
// Veri erişim mantığını merkezi hale getirerek kod tekrarını önler ve test edilebilirliği artırır.
public class SonucRepository : ISonucRepository
{
    private readonly ApplicationDbContext _context;

    public SonucRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // İlişkili Veri Çekme (Eager Loading)
    // '.Include' metodları ile sonuçlarla birlikte sınav ve kullanıcı bilgileri tek seferde çekilir (Piyasa Standardı).
    public async Task<IEnumerable<Sonuc>> GetAllWithRelationsAsync()
    {
        return await _context.Sonuclar
            .Include(s => s.Sinav)
            .Include(s => s.Kullanici)
            .OrderByDescending(s => s.KatilimTarihi)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sonuc>> GetByKullaniciIdAsync(string kullaniciId)
    {
        return await _context.Sonuclar
            .Include(s => s.Sinav)
            .Where(s => s.KullaniciId == kullaniciId)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetGirilenSinavIdleriAsync(string kullaniciId)
    {
        return await _context.Sonuclar
            .Where(s => s.KullaniciId == kullaniciId)
            .Select(s => s.SinavId)
            .ToListAsync();
    }

    public async Task<Sonuc?> GetByIdAsync(int id)
    {
        return await _context.Sonuclar
            .Include(s => s.Sinav)
            .Include(s => s.Kullanici)
            .FirstOrDefaultAsync(s => s.SonucId == id);
    }

    public async Task AddAsync(Sonuc sonuc)
    {
        await _context.Sonuclar.AddAsync(sonuc);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUserTakenSinavAsync(string kullaniciId, int sinavId)
    {
        return await _context.Sonuclar
            .AnyAsync(s => s.KullaniciId == kullaniciId && s.SinavId == sinavId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sonuc = await _context.Sonuclar.FindAsync(id);
        if (sonuc == null) return false;

        _context.Sonuclar.Remove(sonuc);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateScoreAsync(int id, int newScore)
    {
        var sonuc = await _context.Sonuclar.FindAsync(id);
        if (sonuc == null) return false;

        sonuc.Puan = newScore;
        await _context.SaveChangesAsync();
        return true;
    }

    // Liderlik Tablosu Hesaplama — [YÖNERGE] Sadece Öğrenci Rolündekileri ve Puanlarını Gösterir
    public async Task<IEnumerable<LiderlikTablosuDTO>> GetLiderlikTablosuAsync()
    {
        // Önce "Ogrenci" rolünün ID'sini buluyoruz
        var ogrenciRoleId = await _context.Roles
            .Where(r => r.Name == "Ogrenci")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        // Sadece bu role sahip kullanıcıları ve puanlarını çekiyoruz
        return await _context.Users
            .Cast<Kullanici>()
            .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == ogrenciRoleId))
            .Select(u => new LiderlikTablosuDTO
            {
                KullaniciAdi = $"{u.Ad} {u.Soyad}",
                ToplamPuan = u.BonusPuan + _context.Sonuclar.Where(s => s.KullaniciId == u.Id).Sum(s => s.Puan),
                CozulenSinav = _context.Sonuclar.Count(s => s.KullaniciId == u.Id)
            })
            .OrderByDescending(x => x.ToplamPuan)
            .Take(10)
            .ToListAsync();
    }

    // Grafik ve İstatistik Verileri (DTO Kullanımı)
    // Sitenin Dashboard kısmındaki pasta ve bar grafiklerini besleyen veriler burada sarmalanır.
    public async Task<IstatistiklerDTO> GetIstatistiklerAsync()
    {
        var sonuclar = await _context.Sonuclar.Include(s => s.Sinav).ToListAsync();
        var totalSinav = await _context.Sinavlar.CountAsync();
        var totalKullanici = await _context.Users.CountAsync();
        
        var toplamKatilim = sonuclar.Count;
        var ortalamaPuan = toplamKatilim > 0 ? sonuclar.Average(s => s.Puan) : 0;
        
        var basarili = sonuclar.Count(s => s.Puan >= 60);
        var orta = sonuclar.Count(s => s.Puan >= 45 && s.Puan < 60);
        var basarisiz = sonuclar.Count(s => s.Puan < 45);

        var sinavIstatistikleri = sonuclar
            .Where(s => s.Sinav != null)
            .GroupBy(s => s.Sinav!.Baslik)
            .Select(g => new SinavBazliIstatistikDTO
            {
                SinavAdi = g.Key,
                KatilimSayisi = g.Count(),
                OrtalamaPuan = Math.Round(g.Average(x => x.Puan), 1)
            })
            .OrderByDescending(x => x.KatilimSayisi)
            .Take(5)
            .ToList();

        return new IstatistiklerDTO
        {
            Genel = new GenelIstatistikDTO { 
                TotalSinav = totalSinav, 
                TotalKullanici = totalKullanici, 
                ToplamKatilim = toplamKatilim, 
                OrtalamaPuan = Math.Round(ortalamaPuan, 1) 
            },
            PuanGrup = new PuanGrupDTO { 
                Basarili = basarili, 
                Orta = orta, 
                Basarisiz = basarisiz 
            },
            SinavBazli = sinavIstatistikleri
        };
    }
}
