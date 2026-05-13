namespace OnlineSinavPortali.API.DTOs;

public class LiderlikTablosuDTO
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public double ToplamPuan { get; set; }
    public int CozulenSinav { get; set; }
}

public class IstatistiklerDTO
{
    public GenelIstatistikDTO Genel { get; set; } = new();
    public PuanGrupDTO PuanGrup { get; set; } = new();
    public List<SinavBazliIstatistikDTO> SinavBazli { get; set; } = new();
}

public class GenelIstatistikDTO
{
    public int TotalSinav { get; set; }
    public int TotalKullanici { get; set; }
    public int ToplamKatilim { get; set; }
    public double OrtalamaPuan { get; set; }
}

public class PuanGrupDTO
{
    public int Basarili { get; set; }
    public int Orta { get; set; }
    public int Basarisiz { get; set; }
}

public class SinavBazliIstatistikDTO
{
    public string SinavAdi { get; set; } = string.Empty;
    public int KatilimSayisi { get; set; }
    public double OrtalamaPuan { get; set; }
}
