using System.ComponentModel.DataAnnotations;

namespace OnlineExamPortal.API.Models;

public class Exam
{
    [Key]
    public int ExamId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public int DurationMinutes { get; set; }

    // Sınavın hangi tarihler arasında aktif olacağı
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Geçme notu (yüzde olarak, ör: 60 = %60)
    public int PassingGrade { get; set; } = 60;

    // Sınavın toplam kaç puan üzerinden değerlendirileceği (Örn: 100, 500, 1000)
    public int TotalScore { get; set; } = 100;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
