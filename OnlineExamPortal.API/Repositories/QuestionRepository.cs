using Microsoft.EntityFrameworkCore;
using OnlineExamPortal.API.Data;
using OnlineExamPortal.API.Models;

namespace OnlineExamPortal.API.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Question>> GetQuestionsByExamIdAsync(int examId)
    {
        return await _context.Questions
            .Where(s => s.ExamId == examId)
            .Include(s => s.Choices)
            .ToListAsync();
    }

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await _context.Questions
            .Include(s => s.Choices)
            .FirstOrDefaultAsync(s => s.QuestionId == id);
    }

    public async Task AddQuestionAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        // Önce mevcut soruyu ve şıklarını temizleyip yenilerini ekleyebiliriz 
        // veya tek tek güncelleyebiliriz. En sağlamı mevcut şıkları silip güncel olanları eklemek.
        var existingQuestion = await _context.Questions
            .Include(s => s.Choices)
            .FirstOrDefaultAsync(s => s.QuestionId == question.QuestionId);

        if (existingQuestion != null)
        {
            existingQuestion.QuestionText = question.QuestionText;
            existingQuestion.QuestionType = question.QuestionType;
            
            // Eski seçenekleri kaldır
            _context.Choices.RemoveRange(existingQuestion.Choices);
            
            // Yeni seçenekleri ekle
            existingQuestion.Choices = question.Choices;

            _context.Questions.Update(existingQuestion);
            await _context.SaveChangesAsync();
        }
    }
}
