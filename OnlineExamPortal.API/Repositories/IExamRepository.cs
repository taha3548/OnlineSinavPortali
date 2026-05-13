using OnlineExamPortal.API.Models;

namespace OnlineExamPortal.API.Repositories;

public interface IExamRepository
{
    Task<IEnumerable<Exam>> GetAllExamsAsync();
    Task<Exam?> GetExamByIdAsync(int id);
    Task AddExamAsync(Exam exam);
    Task UpdateExamAsync(Exam exam);
    Task DeleteExamAsync(int id);
}
