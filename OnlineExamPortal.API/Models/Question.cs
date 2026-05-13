using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineExamPortal.API.Models;

public class Question
{
    [Key]
    public int QuestionId { get; set; }

    [Required]
    public string QuestionType { get; set; } = "MultipleChoice";

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public int ExamId { get; set; }

    [ForeignKey("ExamId")]
    [JsonIgnore]
    public Exam? Exam { get; set; }

    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}
