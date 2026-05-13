using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavPortali.API.Models;
using OnlineSinavPortali.API.Repositories;

namespace OnlineSinavPortali.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SorularController : ControllerBase
{
    private readonly ISoruRepository _soruRepository;

    public SorularController(ISoruRepository soruRepository)
    {
        _soruRepository = soruRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var soru = await _soruRepository.SoruGetirByIdAsync(id);
        if (soru == null) return NotFound();
        return Ok(soru);
    }

    [HttpGet("sinav/{sinavId}")]
    public async Task<IActionResult> GetBySinavId(int sinavId)
    {
        var sorular = await _soruRepository.SinavaAitSorulariGetirAsync(sinavId);
        return Ok(sorular);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> Post([FromBody] Soru soru)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _soruRepository.SoruEkleAsync(soru);
        return Ok(soru);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> Put(int id, [FromBody] Soru soru)
    {
        if (id != soru.SoruId) return BadRequest();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _soruRepository.SoruGuncelleAsync(soru);
        return Ok(new { Mesaj = "Soru başarıyla güncellendi." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> Delete(int id)
    {
        await _soruRepository.SoruSilAsync(id);
        return NoContent();
    }
}
