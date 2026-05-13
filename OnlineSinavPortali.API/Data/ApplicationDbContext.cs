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
using OnlineSinavPortali.API.Models;

namespace OnlineSinavPortali.API.Data;

// SQL Server Veri Tabanı Yapılandırması ve Identity Entegrasyonu
// IdentityDbContext kullanılarak ASP.NET Identity tabloları otomatik olarak sisteme dahil edilir.
public class ApplicationDbContext : IdentityDbContext<Kullanici, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Code-First Yaklaşımı ile Tanımlanan Modeller (Veri Tabanı Tabloları)
    public DbSet<Sinav> Sinavlar { get; set; }
    public DbSet<Soru> Sorular { get; set; }
    public DbSet<Secenek> Secenekler { get; set; }
    public DbSet<Sonuc> Sonuclar { get; set; }
    public DbSet<GirisLogu> GirisLoglari { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Veri Tabanı İlişkileri (Fluent API) ve Silme Kısıtlamaları (Cascade Delete)
        // Sınav silindiğinde ona bağlı soruların otomatik silinmesi (Derste yapılan standartlar).
        
        modelBuilder.Entity<Soru>()
            .HasOne(s => s.Sinav)
            .WithMany(sinav => sinav.Sorular)
            .HasForeignKey(s => s.SinavId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Secenek>()
            .HasOne(se => se.Soru)
            .WithMany(s => s.Secenekler)
            .HasForeignKey(se => se.SoruId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kullanıcı ve Sonuç İlişkisi
        // Kullanıcı silindiğinde sınav sonuçlarının da silinmesi.
        modelBuilder.Entity<Sonuc>()
            .HasOne(s => s.Kullanici)
            .WithMany()
            .HasForeignKey(s => s.KullaniciId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sonuc>()
            .HasOne(s => s.Sinav)
            .WithMany()
            .HasForeignKey(s => s.SinavId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
