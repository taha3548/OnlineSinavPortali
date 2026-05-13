using Microsoft.AspNetCore.Identity;

namespace OnlineSinavPortali.API.Models;

public class Kullanici : IdentityUser
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    /// <summary>Demo amaçlıdır — production'da kullanılmamalı</summary>
    public string SifreDuz { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    // Admin Yetkisi: Sınav dışı genel ödül/kanaat puanı
    public int BonusPuan { get; set; } = 0;
}
