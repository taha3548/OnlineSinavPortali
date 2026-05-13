using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortal.API.Models;
using OnlineExamPortal.API.Repositories;

namespace OnlineExamPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionRepository _questionRepository;

    public QuestionsController(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var question = await _questionRepository.GetQuestionByIdAsync(id);
        if (question == null) return NotFound();
        return Ok(question);
    }

    [HttpGet("exam/{examId}")]
    public async Task<IActionResult> GetByExamId(int examId)
    {
        var questions = await _questionRepository.GetQuestionsByExamIdAsync(examId);
        return Ok(questions);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> Post([FromBody] Question question)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _questionRepository.AddQuestionAsync(question);
        return Ok(question);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> Put(int id, [FromBody] Question question)
    {
        if (id != question.QuestionId) return BadRequest();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _questionRepository.UpdateQuestionAsync(question);
        return Ok(new { Message = "Soru başarıyla güncellendi." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> Delete(int id)
    {
        await _questionRepository.DeleteQuestionAsync(id);
        return NoContent();
    }
}
