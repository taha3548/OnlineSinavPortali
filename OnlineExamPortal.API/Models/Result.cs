using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamPortal.API.Models;

public class Result
{
    [Key]
    public int ResultId { get; set; }

    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public AppUser? User { get; set; }

    public int ExamId { get; set; }

    [ForeignKey("ExamId")]
    public Exam? Exam { get; set; }

    public int Score { get; set; }
    
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }

    public DateTime ParticipationDate { get; set; } = DateTime.Now;
}
