using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineSinavPortali.API.Data;
using OnlineSinavPortali.API.Models;
using OnlineSinavPortali.API.Repositories;

// [YÖNERGE UYUMLULUĞU] - N-Layered (Web API + MVC) Mimari Yapısı
var builder = WebApplication.CreateBuilder(args);

// Veri Tabanı Bağlantısı (ConnectionString) ve DbContext Kaydı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// [YÖNERGE UYUMLULUĞU] - Repository Pattern & Dependency Injection (Servis Kayıtları)
// Veri erişimi soyutlanarak (Interface) Dependency Injection ile yönetilmektedir.
builder.Services.AddScoped<ISinavRepository, SinavRepository>();
builder.Services.AddScoped<ISoruRepository, SoruRepository>();
builder.Services.AddScoped<ISonucRepository, SonucRepository>();

// [YÖNERGE UYUMLULUĞU] - ASP.NET Identity & Role Based Access Control (RBAC)
builder.Services.AddIdentity<Kullanici, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// CORS Politikası - MVC Projesinden gelen AJAX isteklerine izin verir.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// [YÖNERGE UYUMLULUĞU] - JWT (JSON Web Token) Tabanlı Kimlik Doğrulama
// API güvenliği ve MVC katmanı ile güvenli haberleşme için kullanılır.
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "API v1")); // Swagger UI support
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();

    // [YÖNERGE UYUMLULUĞU] - Entity Framework Core Code-First Migrations
    await context.Database.MigrateAsync();

    // Tüm rollerin var olduğundan emin ol
    foreach (var rol in new[] { "SiteYoneticisi", "Admin", "Ogrenci" })
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new IdentityRole(rol));

    // Site Yöneticisi seed — varsa rolünü güncelle
    var siteYoneticisi = await userManager.FindByEmailAsync("admin@onlinesinav.com");
    if (siteYoneticisi == null)
    {
        siteYoneticisi = new Kullanici
        {
            UserName = "admin@onlinesinav.com",
            Email = "admin@onlinesinav.com",
            Ad = "Site",
            Soyad = "Yöneticisi",
            SifreDuz = "Taha123",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(siteYoneticisi, "Taha123");
    }
    // SiteYoneticisi rolü yoksa ekle
    var mevcutRoller = await userManager.GetRolesAsync(siteYoneticisi);
    if (!mevcutRoller.Contains("SiteYoneticisi"))
    {
        await userManager.RemoveFromRolesAsync(siteYoneticisi, mevcutRoller);
        await userManager.AddToRoleAsync(siteYoneticisi, "SiteYoneticisi");
    }
}

app.Run();
