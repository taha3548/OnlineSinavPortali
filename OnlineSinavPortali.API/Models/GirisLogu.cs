using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSinavPortali.API.Models;

public class GirisLogu
{
    [Key]
    public int LogId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = "Ogrenci";
    public bool Basarili { get; set; }
    public string IpAdresi { get; set; } = string.Empty;
    public DateTime GirisTarihi { get; set; } = DateTime.Now;
}
