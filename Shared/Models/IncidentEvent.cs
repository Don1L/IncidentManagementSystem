namespace Shared.Models;

public class IncidentEvent
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
}