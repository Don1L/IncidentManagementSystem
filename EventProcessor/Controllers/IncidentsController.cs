using EventProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Enums;

namespace EventProcessor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly IncidentService _incidentService;

    public IncidentsController(IncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<IncidentDto>>> GetIncidents()
    {
        var incidents = await _incidentService.GetIncidentsAsync();

        var incidentDtos = incidents.Select(i => new IncidentDto
        {
            Id = i.Id,
            Type = i.Type,
            Time = i.Time,
            Events = i.IncidentEvents.Select(ie => new EventDto
            {
                Id = ie.Event.Id,
                Type = ie.Event.Type,
                Time = ie.Event.Time
            }).ToList()
        }).ToList();

        return Ok(incidentDtos);
    }
}