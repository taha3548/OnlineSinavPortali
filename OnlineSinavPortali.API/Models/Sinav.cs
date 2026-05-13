using System.ComponentModel.DataAnnotations;

namespace OnlineSinavPortali.API.Models;

public class Sinav
{
    [Key]
    public int SinavId { get; set; }

    [Required]
    public string Baslik { get; set; } = string.Empty;

    public string Aciklama { get; set; } = string.Empty;

    [Required]
    public int SureDakika { get; set; }

    // Sınavın hangi tarihler arasında aktif olacağı
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }

    // Geçme notu (yüzde olarak, ör: 60 = %60)
    public int GecmeNotu { get; set; } = 60;

    // Sınavın toplam kaç puan üzerinden değerlendirileceği (Örn: 100, 500, 1000)
    public int ToplamPuan { get; set; } = 100;

    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

    public ICollection<Soru> Sorular { get; set; } = new List<Soru>();
}
