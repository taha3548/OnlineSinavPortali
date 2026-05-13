using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSinavPortali.API.Models;

public class Sonuc
{
    [Key]
    public int SonucId { get; set; }

    public string KullaniciId { get; set; } = string.Empty;

    [ForeignKey("KullaniciId")]
    public Kullanici? Kullanici { get; set; }

    public int SinavId { get; set; }

    [ForeignKey("SinavId")]
    public Sinav? Sinav { get; set; }

    public int Puan { get; set; }
    
    public int DogruSayisi { get; set; }
    public int YanlisSayisi { get; set; }

    public DateTime KatilimTarihi { get; set; } = DateTime.Now;
}
