using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamPortal.API.Models;
using OnlineExamPortal.API.Repositories;

namespace OnlineExamPortal.API.Controllers;

// [YÖNERGE UYUMLULUĞU] - Web API (Servis Katmanı)
[Route("api/[controller]")]
[ApiController]
public class ExamsController : ControllerBase
{
    private readonly IExamRepository _examRepository;

    public ExamsController(IExamRepository examRepository)
    {
        _examRepository = examRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        var exams = await _examRepository.GetAllExamsAsync();
        return Ok(exams);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var exam = await _examRepository.GetExamByIdAsync(id);
        if (exam == null)
            return NotFound();

        return Ok(exam);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> Post([FromBody] Exam exam)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _examRepository.AddExamAsync(exam);
        return CreatedAtAction(nameof(Get), new { id = exam.ExamId }, exam);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> Put(int id, [FromBody] Exam exam)
    {
        if (id != exam.ExamId)
            return BadRequest();

        await _examRepository.UpdateExamAsync(exam);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SiteAdministrator")]
    public async Task<IActionResult> Delete(int id)
    {
        await _examRepository.DeleteExamAsync(id);
        return NoContent();
    }
}
