using Microsoft.EntityFrameworkCore;
using OnlineExamPortal.API.Data;
using OnlineExamPortal.API.Models;
using OnlineExamPortal.API.DTOs;

namespace OnlineExamPortal.API.Repositories;

// Repository Pattern Uygulaması
// Veri erişim mantığını merkezi hale getirerek kod tekrarını önler ve test edilebilirliği artırır.
public class ResultRepository : IResultRepository
{
    private readonly ApplicationDbContext _context;

    public ResultRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // İlişkili Veri Çekme (Eager Loading)
    // '.Include' metodları ile sonuçlarla birlikte sınav ve kullanıcı bilgileri tek seferde çekilir (Piyasa Standardı).
    public async Task<IEnumerable<Result>> GetAllWithRelationsAsync()
    {
        return await _context.Results
            .Include(s => s.Exam)
            .Include(s => s.User)
            .OrderByDescending(s => s.ParticipationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Result>> GetByUserIdAsync(string userId)
    {
        return await _context.Results
            .Include(s => s.Exam)
            .Where(s => s.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<int>> GetTakenExamIdsAsync(string userId)
    {
        return await _context.Results
            .Where(s => s.UserId == userId)
            .Select(s => s.ExamId)
            .ToListAsync();
    }

    public async Task<Result?> GetByIdAsync(int id)
    {
        return await _context.Results
            .Include(s => s.Exam)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.ResultId == id);
    }

    public async Task AddAsync(Result result)
    {
        await _context.Results.AddAsync(result);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUserTakenExamAsync(string userId, int examId)
    {
        return await _context.Results
            .AnyAsync(s => s.UserId == userId && s.ExamId == examId);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _context.Results.FindAsync(id);
        if (result == null) return false;

        _context.Results.Remove(result);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateScoreAsync(int id, int newScore)
    {
        var result = await _context.Results.FindAsync(id);
        if (result == null) return false;

        result.Score = newScore;
        await _context.SaveChangesAsync();
        return true;
    }

    // Liderlik Tablosu Hesaplama — [YÖNERGE] Sadece Öğrenci Rolündekileri ve Puanlarını Gösterir
    public async Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync()
    {
        // Önce "Student" rolünün ID'sini buluyoruz
        var studentRoleId = await _context.Roles
            .Where(r => r.Name == "Student")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(studentRoleId))
            return Enumerable.Empty<LeaderboardDto>();

        // Sadece bu role sahip kullanıcıları ve puanlarını çekiyoruz
        return await _context.Users
            .Cast<AppUser>()
            .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == studentRoleId))
            .Select(u => new LeaderboardDto
            {
                UserName = $"{u.FirstName} {u.LastName}",
                TotalScore = u.BonusScore + _context.Results.Where(s => s.UserId == u.Id).Sum(s => s.Score),
                SolvedExams = _context.Results.Count(s => s.UserId == u.Id)
            })
            .OrderByDescending(x => x.TotalScore)
            .Take(10)
            .ToListAsync();
    }

    // Grafik ve İstatistik Verileri (DTO Kullanımı)
    // Sitenin Dashboard kısmındaki pasta ve bar grafiklerini besleyen veriler burada sarmalanır.
    public async Task<StatisticsDto> GetStatisticsAsync()
    {
        var results = await _context.Results.Include(s => s.Exam).ToListAsync();
        var totalExams = await _context.Exams.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        
        var totalParticipation = results.Count;
        var averageScore = totalParticipation > 0 ? results.Average(s => s.Score) : 0;
        
        var successful = results.Count(s => s.Score >= 60);
        var average = results.Count(s => s.Score >= 45 && s.Score < 60);
        var unsuccessful = results.Count(s => s.Score < 45);

        var examStatistics = results
            .Where(s => s.Exam != null)
            .GroupBy(s => s.Exam!.Title)
            .Select(g => new ExamBasedStatisticsDto
            {
                ExamName = g.Key,
                ParticipationCount = g.Count(),
                AverageScore = Math.Round(g.Average(x => x.Score), 1)
            })
            .OrderByDescending(x => x.ParticipationCount)
            .Take(5)
            .ToList();

        return new StatisticsDto
        {
            General = new GeneralStatisticsDto { 
                TotalExams = totalExams, 
                TotalUsers = totalUsers, 
                TotalParticipation = totalParticipation, 
                AverageScore = Math.Round(averageScore, 1) 
            },
            ScoreGroup = new ScoreGroupDto { 
                Successful = successful, 
                Average = average, 
                Unsuccessful = unsuccessful 
            },
            ExamBased = examStatistics
        };
    }
}
