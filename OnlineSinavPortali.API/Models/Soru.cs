using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSinavPortali.API.Models;

public class Soru
{
    [Key]
    public int SoruId { get; set; }

    [Required]
    public string SoruTipi { get; set; } = "CoktanSecmeli";

    [Required]
    public string SoruMetni { get; set; } = string.Empty;

    public int SinavId { get; set; }

    [ForeignKey("SinavId")]
    [JsonIgnore]
    public Sinav? Sinav { get; set; }

    public ICollection<Secenek> Secenekler { get; set; } = new List<Secenek>();
}
