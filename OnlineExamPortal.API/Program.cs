using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineExamPortal.API.Data;
using OnlineExamPortal.API.Models;
using OnlineExamPortal.API.Repositories;

// [YÖNERGE UYUMLULUĞU] - N-Layered (Web API + MVC) Mimari Yapısı
var builder = WebApplication.CreateBuilder(args);

// Veri Tabanı Bağlantısı (ConnectionString) ve DbContext Kaydı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// [YÖNERGE UYUMLULUĞU] - Repository Pattern & Dependency Injection (Servis Kayıtları)
// Veri erişimi soyutlanarak (Interface) Dependency Injection ile yönetilmektedir.
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();

// [YÖNERGE UYUMLULUĞU] - ASP.NET Identity & Role Based Access Control (RBAC)
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // [YÖNERGE UYUMLULUĞU] - Entity Framework Core Code-First Migrations
    await context.Database.MigrateAsync();

    // Ensure all roles exist
    foreach (var role in new[] { "SiteAdministrator", "Admin", "Student" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Seed Site Administrator
    var siteAdmin = await userManager.FindByEmailAsync("admin@onlinesinav.com");
    if (siteAdmin == null)
    {
        siteAdmin = new AppUser
        {
            UserName = "admin@onlinesinav.com",
            Email = "admin@onlinesinav.com",
            FirstName = "Site",
            LastName = "Administrator",
            PasswordPlain = "Taha123",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await userManager.CreateAsync(siteAdmin, "Taha123");
    }
    // Add SiteAdministrator role if missing
    var currentRoles = await userManager.GetRolesAsync(siteAdmin);
    if (!currentRoles.Contains("SiteAdministrator"))
    {
        await userManager.RemoveFromRolesAsync(siteAdmin, currentRoles);
        await userManager.AddToRoleAsync(siteAdmin, "SiteAdministrator");
    }
}

app.Run();
