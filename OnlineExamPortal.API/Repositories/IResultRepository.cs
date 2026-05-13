using OnlineExamPortal.API.Models;
using OnlineExamPortal.API.DTOs;

namespace OnlineExamPortal.API.Repositories;

public interface IResultRepository
{
    Task<IEnumerable<Result>> GetAllWithRelationsAsync();
    Task<IEnumerable<Result>> GetByUserIdAsync(string userId);
    Task<IEnumerable<int>> GetTakenExamIdsAsync(string userId);
    Task<Result?> GetByIdAsync(int id);
    Task AddAsync(Result result);
    Task<bool> HasUserTakenExamAsync(string userId, int examId);
    
    // Admin Yetkileri: Sonuç Sıfırlama ve Puan Müdahalesi
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateScoreAsync(int id, int newScore);

    // İstatistik ve Liderlik için DTO tabanlı metodlar
    Task<IEnumerable<LeaderboardDto>> GetLeaderboardAsync();
    Task<StatisticsDto> GetStatisticsAsync();
}
