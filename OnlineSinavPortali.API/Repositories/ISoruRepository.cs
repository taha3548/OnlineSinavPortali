using OnlineSinavPortali.API.Models;

namespace OnlineSinavPortali.API.Repositories;

public interface ISoruRepository
{
    Task<IEnumerable<Soru>> SinavaAitSorulariGetirAsync(int sinavId);
    Task<Soru?> SoruGetirByIdAsync(int id);
    Task SoruEkleAsync(Soru soru);
    Task SoruGuncelleAsync(Soru soru);
    Task SoruSilAsync(int id);
}
