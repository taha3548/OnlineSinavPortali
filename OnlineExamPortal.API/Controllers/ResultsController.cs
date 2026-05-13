using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortal.API.Models;
using OnlineExamPortal.API.Repositories;
using System.Security.Claims;

namespace OnlineExamPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly IResultRepository _resultRepository;

    public ResultsController(IResultRepository resultRepository)
    {
        _resultRepository = resultRepository;
    }

    // Admin: Tüm öğrencilerin sonuçları
    [HttpGet("all")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> GetAll()
    {
        var results = await _resultRepository.GetAllWithRelationsAsync();

        var response = results.Select(s => new
        {
            resultId = s.ResultId,
            userName = s.User != null ? $"{s.User.FirstName} {s.User.LastName}" : "Bilinmiyor",
            userEmail = s.User?.Email ?? "—",
            examTitle = s.Exam?.Title ?? "—",
            score = s.Score,
            correctCount = s.CorrectCount,
            wrongCount = s.WrongCount,
            participationDate = s.ParticipationDate
        });

        return Ok(response);
    }

    // Liderlik Tablosu: Tüm sınavlardan en çok puan toplayan ilk 10 öğrenci
    [HttpGet("leaderboard")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeaderboard()
    {
        var leaderboard = await _resultRepository.GetLeaderboardAsync();
        return Ok(leaderboard);
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _resultRepository.GetStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserResults(string userId)
    {
        // [YETKİ KONTROLÜ] - Student -> Kendi Karnesi, Admin -> Her Şey
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        if (currentUserRole != "Admin" && currentUserRole != "SiteAdministrator" && currentUserId != userId)
            return Forbid();

        var results = await _resultRepository.GetByUserIdAsync(userId);
        return Ok(results);
    }

    // [YÖNERGE UYUMLULUĞU] - Dinamik Sınav Puanı Hesaplama Mantığı (POST)
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Result result)
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
            result.UserId = userId;

        // Aynı kullanıcı aynı sınava tekrar giremez
        var alreadyTaken = await _resultRepository.HasUserTakenExamAsync(result.UserId!, result.ExamId);
        if (alreadyTaken)
            return BadRequest(new { Message = "Bu sınava zaten katıldınız. Tekrar giremezsiniz." });

        ModelState.Remove("User");
        ModelState.Remove("Exam");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _resultRepository.AddAsync(result);

        return Ok(new { Message = "Sonuç başarıyla kaydedildi.", Score = result.Score });
    }

    // Kullanıcının daha önce girdiği sınav ID'lerini döner
    [HttpGet("taken/{userId}")]
    [HttpGet("takenexams/{userId}")]
    public async Task<IActionResult> GetTakenExams(string userId)
    {
        var examIds = await _resultRepository.GetTakenExamIdsAsync(userId);
        return Ok(examIds);
    }

    // Admin Yetkileri: Sonuç Sıfırlama (Silme)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _resultRepository.DeleteAsync(id);
        if (!success) return NotFound(new { Message = "Sonuç bulunamadı." });

        return Ok(new { Message = "Öğrencinin sınav sonucu sıfırlandı. Sınava yeniden girebilir." });
    }

    // Admin Yetkileri: Puan Müdahalesi (Güncelleme)
    [HttpPut("{id}/score")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> UpdateScore(int id, [FromBody] UpdateScoreDto dto)
    {
        var success = await _resultRepository.UpdateScoreAsync(id, dto.NewScore);
        if (!success) return NotFound(new { Message = "Sonuç bulunamadı." });

        return Ok(new { Message = "Öğrenci puanı başarıyla güncellendi." });
    }

    // [YÖNERGE UYUMLULUĞU] - Manuel Puan Girişi ve Bonus Sistemi
    [HttpPost("manual")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> ManualScoreEntry([FromBody] ManualScoreDto model)
    {
        var results = await _resultRepository.GetByUserIdAsync(model.UserId);
        var existingResult = results.FirstOrDefault(s => s.ExamId == model.ExamId);

        if (existingResult != null)
        {
            // Güncelleme
            existingResult.Score = model.Score;
            existingResult.ParticipationDate = DateTime.Now;
            await _resultRepository.UpdateScoreAsync(existingResult.ResultId, model.Score);
            return Ok(new { Message = "Öğrencinin mevcut sınav puanı güncellendi." });
        }
        else
        {
            // Yeni Kayıt
            var newResult = new Result
            {
                UserId = model.UserId,
                ExamId = model.ExamId,
                Score = model.Score,
                CorrectCount = 0, // Manuel girildiği için detaylar 0
                WrongCount = 0,
                ParticipationDate = DateTime.Now
            };
            await _resultRepository.AddAsync(newResult);
            return Ok(new { Message = "Öğrenci için sınav puanı tanımlandı." });
        }
    }
}

public class ManualScoreDto
{
    public string UserId { get; set; } = string.Empty;
    public int ExamId { get; set; }
    public int Score { get; set; }
}

public class UpdateScoreDto
{
    public int NewScore { get; set; }
}
