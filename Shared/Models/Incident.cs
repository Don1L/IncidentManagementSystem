using Shared.Enums;

namespace Shared.Models;

public class Incident
{
    public Guid Id { get; set; }
    public IncidentType Type { get; set; }
    public DateTime Time { get; set; }
    
    //Navigation properties
    public List<IncidentEvent> IncidentEvents { get; set; } = new();
}