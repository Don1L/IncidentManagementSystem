using Shared.Enums;

namespace Shared.DTOs;

public class IncidentDto
{
    public Guid Id { get; set; }
    public IncidentType Type { get; set; }
    public DateTime Time { get; set; }
    public List<EventDto> Events { get; set; }
}