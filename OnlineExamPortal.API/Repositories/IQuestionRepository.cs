using OnlineExamPortal.API.Models;

namespace OnlineExamPortal.API.Repositories;

public interface IQuestionRepository
{
    Task<IEnumerable<Question>> GetQuestionsByExamIdAsync(int examId);
    Task<Question?> GetQuestionByIdAsync(int id);
    Task AddQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(int id);
}
