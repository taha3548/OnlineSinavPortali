using System.ComponentModel.DataAnnotations;

namespace OnlineExamPortal.API.Models;

public class LoginLog
{
    [Key]
    public int LogId { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Student";
    public bool IsSuccessful { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LoginDate { get; set; } = DateTime.Now;
}
