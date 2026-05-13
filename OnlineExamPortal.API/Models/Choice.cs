using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineExamPortal.API.Models;

public class Choice
{
    [Key]
    public int ChoiceId { get; set; }

    [Required]
    public string ChoiceText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey("QuestionId")]
    [JsonIgnore]
    public Question? Question { get; set; }
}
