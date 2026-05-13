namespace OnlineSinavPortali.API.DTOs;

public class KayitDTO
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
}

public class GirisDTO
{
    public string Email { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
}

public class ProfilDTO
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}
