using Microsoft.EntityFrameworkCore;
using OnlineSinavPortali.API.Data;
using OnlineSinavPortali.API.Models;

namespace OnlineSinavPortali.API.Repositories;

public class SoruRepository : ISoruRepository
{
    private readonly ApplicationDbContext _context;

    public SoruRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Soru>> SinavaAitSorulariGetirAsync(int sinavId)
    {
        return await _context.Sorular
            .Where(s => s.SinavId == sinavId)
            .Include(s => s.Secenekler)
            .ToListAsync();
    }

    public async Task<Soru?> SoruGetirByIdAsync(int id)
    {
        return await _context.Sorular
            .Include(s => s.Secenekler)
            .FirstOrDefaultAsync(s => s.SoruId == id);
    }

    public async Task SoruEkleAsync(Soru soru)
    {
        await _context.Sorular.AddAsync(soru);
        await _context.SaveChangesAsync();
    }

    public async Task SoruSilAsync(int id)
    {
        var soru = await _context.Sorular.FindAsync(id);
        if (soru != null)
        {
            _context.Sorular.Remove(soru);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoruGuncelleAsync(Soru soru)
    {
        // Önce mevcut soruyu ve şıklarını temizleyip yenilerini ekleyebiliriz 
        // veya tek tek güncelleyebiliriz. En sağlamı mevcut şıkları silip güncel olanları eklemek.
        var mevcutSoru = await _context.Sorular
            .Include(s => s.Secenekler)
            .FirstOrDefaultAsync(s => s.SoruId == soru.SoruId);

        if (mevcutSoru != null)
        {
            mevcutSoru.SoruMetni = soru.SoruMetni;
            mevcutSoru.SoruTipi = soru.SoruTipi;
            
            // Eski seçenekleri kaldır
            _context.Secenekler.RemoveRange(mevcutSoru.Secenekler);
            
            // Yeni seçenekleri ekle
            mevcutSoru.Secenekler = soru.Secenekler;

            _context.Sorular.Update(mevcutSoru);
            await _context.SaveChangesAsync();
        }
    }
}
