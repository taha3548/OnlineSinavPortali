using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineSinavPortali.API.Models;

public class Secenek
{
    [Key]
    public int SecenekId { get; set; }

    [Required]
    public string SecenekMetni { get; set; } = string.Empty;

    public bool DogruMu { get; set; }

    public int SoruId { get; set; }

    [ForeignKey("SoruId")]
    [JsonIgnore]
    public Soru? Soru { get; set; }
}
