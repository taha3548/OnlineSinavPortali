using OnlineSinavPortali.API.Models;

namespace OnlineSinavPortali.API.Repositories;

public interface ISinavRepository
{
    Task<IEnumerable<Sinav>> TumSinavlariGetirAsync();
    Task<Sinav?> SinavGetirBtyIdAsync(int id);
    Task SinavEkleAsync(Sinav sinav);
    Task SinavGuncelleAsync(Sinav sinav);
    Task SinavSilAsync(int id);
}
