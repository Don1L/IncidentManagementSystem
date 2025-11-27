using EventProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Enums;
using Shared.Models;

namespace EventProcessor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IncidentService _incidentService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IncidentService incidentService, ILogger<EventsController> logger)
    {
        _incidentService = incidentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveEvent([FromBody] EventDto eventDto)
    {
        _logger.LogInformation($"Received event: {eventDto.Id}, Type: {eventDto.Type}");

        var eventData = new Event
        {
            Id = eventDto.Id,
            Type = eventDto.Type,
            Time = eventDto.Time
        };

        await _incidentService.ProcessEventAsync(eventData);

        return Ok();
    }
}