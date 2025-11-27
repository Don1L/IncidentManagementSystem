using EventGenerator.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace EventGenerator.Controllers;
[ApiController]
[Route("api/[controller]")]
public class GeneratorController : ControllerBase
{
    private readonly ILogger<GeneratorController> _logger;
    private readonly EventGeneratorService _generatorService;

    public GeneratorController(
        EventGeneratorService generatorService,
        ILogger<GeneratorController> logger)
    {
        _generatorService = generatorService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<EventDto>> GenerateEvent()
    {
        var eventDto = _generatorService.GenerateRandomEvent();
        
        await _generatorService.SendEventToProcessor(eventDto);
        
        _logger.LogInformation(
            $"Manually generated and sent event: {eventDto.Id}, Type: {eventDto.Type}");
        
        return Ok(eventDto);
    }
}