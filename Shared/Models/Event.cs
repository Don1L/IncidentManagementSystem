using Shared.Enums;

namespace Shared.Models;

public class Event
{
    public Guid Id { get; set; }
    public EventType Type { get; set; }
    public DateTime Time { get; set; }
}