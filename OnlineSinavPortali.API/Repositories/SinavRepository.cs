using Microsoft.EntityFrameworkCore;
using OnlineSinavPortali.API.Data;
using OnlineSinavPortali.API.Models;

namespace OnlineSinavPortali.API.Repositories;

public class SinavRepository : ISinavRepository
{
    private readonly ApplicationDbContext _context;

    public SinavRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sinav>> TumSinavlariGetirAsync()
    {
        return await _context.Sinavlar
            .Include(s => s.Sorular)
            .OrderByDescending(s => s.OlusturulmaTarihi)
            .ToListAsync();
    }

    public async Task<Sinav?> SinavGetirBtyIdAsync(int id)
    {
        return await _context.Sinavlar
            .Include(s => s.Sorular)
            .ThenInclude(soru => soru.Secenekler)
            .FirstOrDefaultAsync(s => s.SinavId == id);
    }

    public async Task SinavEkleAsync(Sinav sinav)
    {
        await _context.Sinavlar.AddAsync(sinav);
        await _context.SaveChangesAsync();
    }

    public async Task SinavGuncelleAsync(Sinav sinav)
    {
        _context.Sinavlar.Update(sinav);
        await _context.SaveChangesAsync();
    }

    public async Task SinavSilAsync(int id)
    {
        var sinav = await _context.Sinavlar.FindAsync(id);
        if (sinav != null)
        {
            _context.Sinavlar.Remove(sinav);
            await _context.SaveChangesAsync();
        }
    }
}
