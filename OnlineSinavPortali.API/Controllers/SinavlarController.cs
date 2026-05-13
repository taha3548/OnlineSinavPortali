using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSinavPortali.API.Models;
using OnlineSinavPortali.API.Repositories;

namespace OnlineSinavPortali.API.Controllers;

// [YÖNERGE UYUMLULUĞU] - Web API (Servis Katmanı)
[Route("api/[controller]")]
[ApiController]
public class SinavlarController : ControllerBase
{
    private readonly ISinavRepository _sinavRepository;

    public SinavlarController(ISinavRepository sinavRepository)
    {
        _sinavRepository = sinavRepository;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        var sinavlar = await _sinavRepository.TumSinavlariGetirAsync();
        return Ok(sinavlar);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var sinav = await _sinavRepository.SinavGetirBtyIdAsync(id);
        if (sinav == null)
            return NotFound();

        return Ok(sinav);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> Post([FromBody] Sinav sinav)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _sinavRepository.SinavEkleAsync(sinav);
        return CreatedAtAction(nameof(Get), new { id = sinav.SinavId }, sinav);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> Put(int id, [FromBody] Sinav sinav)
    {
        if (id != sinav.SinavId)
            return BadRequest();

        await _sinavRepository.SinavGuncelleAsync(sinav);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SiteYoneticisi")]
    public async Task<IActionResult> Delete(int id)
    {
        await _sinavRepository.SinavSilAsync(id);
        return NoContent();
    }
}
