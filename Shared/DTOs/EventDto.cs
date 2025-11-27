using Shared.Enums;

namespace Shared.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public EventType Type { get; set; }
    public DateTime Time { get; set; }
}