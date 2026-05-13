// =====================================
// Online Sınav Portalı - AppDbContext
// Identity tabloları otomatik oluşturulur:
// AspNetUsers, AspNetRoles, AspNetUserRoles vb.
// SQL Server veritabanına bağlantısı:
// appsettings.json → ConnectionStrings → DefaultConnection
// =====================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineExamPortal.API.Models;

namespace OnlineExamPortal.API.Data;

// SQL Server Veri Tabanı Yapılandırması ve Identity Entegrasyonu
// IdentityDbContext kullanılarak ASP.NET Identity tabloları otomatik olarak sisteme dahil edilir.
public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Code-First Yaklaşımı ile Tanımlanan Modeller (Veri Tabanı Tabloları)
    public DbSet<Exam> Exams { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<Result> Results { get; set; }
    public DbSet<LoginLog> LoginLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Veri Tabanı İlişkileri (Fluent API) ve Silme Kısıtlamaları (Cascade Delete)
        // Sınav silindiğinde ona bağlı soruların otomatik silinmesi (Derste yapılan standartlar).
        
        modelBuilder.Entity<Question>()
            .HasOne(s => s.Exam)
            .WithMany(exam => exam.Questions)
            .HasForeignKey(s => s.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Choice>()
            .HasOne(se => se.Question)
            .WithMany(s => s.Choices)
            .HasForeignKey(se => se.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kullanıcı ve Sonuç İlişkisi
        // Kullanıcı silindiğinde sınav sonuçlarının da silinmesi.
        modelBuilder.Entity<Result>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Result>()
            .HasOne(s => s.Exam)
            .WithMany()
            .HasForeignKey(s => s.ExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
