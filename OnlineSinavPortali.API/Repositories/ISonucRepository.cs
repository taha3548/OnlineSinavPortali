using OnlineSinavPortali.API.Models;
using OnlineSinavPortali.API.DTOs;

namespace OnlineSinavPortali.API.Repositories;

public interface ISonucRepository
{
    Task<IEnumerable<Sonuc>> GetAllWithRelationsAsync();
    Task<IEnumerable<Sonuc>> GetByKullaniciIdAsync(string kullaniciId);
    Task<IEnumerable<int>> GetGirilenSinavIdleriAsync(string kullaniciId);
    Task<Sonuc?> GetByIdAsync(int id);
    Task AddAsync(Sonuc sonuc);
    Task<bool> HasUserTakenSinavAsync(string kullaniciId, int sinavId);
    
    // Admin Yetkileri: Sonuç Sıfırlama ve Puan Müdahalesi
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateScoreAsync(int id, int newScore);

    // İstatistik ve Liderlik için DTO tabanlı metodlar
    Task<IEnumerable<LiderlikTablosuDTO>> GetLiderlikTablosuAsync();
    Task<IstatistiklerDTO> GetIstatistiklerAsync();
}
