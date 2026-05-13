using Microsoft.EntityFrameworkCore;
using OnlineExamPortal.API.Data;
using OnlineExamPortal.API.Models;

namespace OnlineExamPortal.API.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly ApplicationDbContext _context;

    public ExamRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Exam>> GetAllExamsAsync()
    {
        return await _context.Exams
            .Include(s => s.Questions)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<Exam?> GetExamByIdAsync(int id)
    {
        return await _context.Exams
            .Include(s => s.Questions)
            .ThenInclude(question => question.Choices)
            .FirstOrDefaultAsync(s => s.ExamId == id);
    }

    public async Task AddExamAsync(Exam exam)
    {
        await _context.Exams.AddAsync(exam);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateExamAsync(Exam exam)
    {
        _context.Exams.Update(exam);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExamAsync(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam != null)
        {
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
        }
    }
}
