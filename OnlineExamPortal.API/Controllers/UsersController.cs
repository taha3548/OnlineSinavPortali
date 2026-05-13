using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineExamPortal.API.Data;
using OnlineExamPortal.API.Models;
using System.Security.Claims;

namespace OnlineExamPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SiteAdministrator")]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<AppUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.ToListAsync();
        var tempResult = new List<dynamic>(); // Temporary list for sorting
        var requestingRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Student";

            // Sorting Priority: SiteAdministrator(1) > Admin(2) > Student(3)
            int priority = userRole switch
            {
                "SiteAdministrator" => 1,
                "Admin" => 2,
                _ => 3
            };

            // Password visible only to authorized roles:
            bool showPassword = false;
            if (requestingRole == "SiteAdministrator") showPassword = true;
            else if (requestingRole == "Admin" && userRole == "Student") showPassword = true;

            tempResult.Add(new
            {
                priority = priority,
                data = new
                {
                    id = user.Id,
                    fullName = $"{user.FirstName} {user.LastName}",
                    email = user.Email,
                    role = userRole,
                    password = showPassword ? (string.IsNullOrEmpty(user.PasswordPlain) ? "—" : user.PasswordPlain) : "••••••••",
                    registrationDate = user.RegistrationDate,
                    bonusScore = user.BonusScore
                }
            });
        }

        // Sort by hierarchy (OrderBy) and return only the data (Select).
        var finalResult = tempResult.OrderBy(x => x.priority).Select(x => x.data).ToList();

        return Ok(finalResult);
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _context.LoginLogs
            .OrderByDescending(l => l.LoginDate)
            .Take(200)
            .ToListAsync();
        return Ok(logs);
    }

    // User deletion
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var target = await _userManager.FindByIdAsync(id);
        if (target == null) return NotFound();

        var targetRole = (await _userManager.GetRolesAsync(target)).FirstOrDefault() ?? "Student";
        var requestingRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";

        if (targetRole == "SiteAdministrator")
            return BadRequest(new { Message = "Site yöneticisi hesabı silinemez." });

        if (requestingRole == "Admin" && targetRole == "Admin")
            return StatusCode(403, new { Message = "Yöneticiler birbirini silemez." });

        var result = await _userManager.DeleteAsync(target);
        if (!result.Succeeded)
            return StatusCode(500, new { Message = "Silme işlemi başarısız oldu." });

        return Ok(new { Message = "Kullanıcı silindi." });
    }

    // Role change — ONLY SiteAdministrator
    [HttpPut("{id}/role")]
    [Authorize(Roles = "SiteAdministrator")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleDto dto)
    {
        var target = await _userManager.FindByIdAsync(id);
        if (target == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(target);

        if (currentRoles.Contains("SiteAdministrator"))
            return BadRequest(new { Message = "Site yöneticisinin rolü değiştirilemez." });

        if (dto.NewRole != "Admin" && dto.NewRole != "Student")
            return BadRequest(new { Message = "Geçersiz rol." });

        await _userManager.RemoveFromRolesAsync(target, currentRoles);
        await _userManager.AddToRoleAsync(target, dto.NewRole);

        var roleLabel = dto.NewRole == "Admin" ? "Yönetici" : "Öğrenci";
        return Ok(new { Message = $"Rol '{roleLabel}' olarak güncellendi." });
    }

    // [YÖNERGE UYUMLULUĞU] - General Bonus Score (Opinion Grade) System
    [HttpPut("{id}/bonus-score")]
    public async Task<IActionResult> UpdateBonusScore(string id, [FromBody] BonusScoreDto dto)
    {
        var target = await _userManager.FindByIdAsync(id);
        if (target == null) return NotFound();

        target.BonusScore = dto.NewScore;
        var result = await _userManager.UpdateAsync(target);
        if (!result.Succeeded)
            return StatusCode(500, new { Message = "Bonus puan güncellenemedi." });

        return Ok(new { Message = "Bonus puan başarıyla tanımlandı." });
    }
}

public class BonusScoreDto
{
    public int NewScore { get; set; }
}

public class ChangeRoleDto
{
    public string NewRole { get; set; } = string.Empty;
}
