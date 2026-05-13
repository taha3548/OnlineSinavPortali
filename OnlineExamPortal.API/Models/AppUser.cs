using Microsoft.AspNetCore.Identity;

namespace OnlineExamPortal.API.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    /// <summary>Demo amaçlıdır — production'da kullanılmamalı</summary>
    public string PasswordPlain { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.Now;

    // Admin Yetkisi: Sınav dışı genel ödül/kanaat puanı
    public int BonusScore { get; set; } = 0;
}
