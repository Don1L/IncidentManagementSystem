using EventProcessor.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models;

namespace EventProcessor.Services;

public class IncidentService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IncidentService> _logger;
    
    // Очереди ожидания для составных шаблонов
    private readonly List<PendingEvent> _pendingType2Events = new();
    private readonly List<PendingEvent> _pendingType3Events = new();
    private readonly object _lock = new();

    public IncidentService(IServiceScopeFactory scopeFactory, ILogger<IncidentService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ProcessEventAsync(Event eventData)
    {
        lock (_lock)
        {
            // Очистка устаревших ожиданий
            CleanupExpiredPendingEvents();
        }

        switch (eventData.Type)
        {
            case EventType.Type1:
                await HandleType1EventAsync(eventData);
                break;
            case EventType.Type2:
                await HandleType2EventAsync(eventData);
                break;
            case EventType.Type3:
                await HandleType3EventAsync(eventData);
                break;
        }
    }

    private async Task HandleType1EventAsync(Event eventData)
    {
        await SaveEventAsync(eventData);
        
        await CreateIncidentAsync(IncidentType.Type1, new List<Event> { eventData });
        
        lock (_lock)
        {
            var pendingType2 = _pendingType2Events
                .Where(p => (DateTime.UtcNow - p.Event.Time).TotalSeconds <= 20)
                .OrderBy(p => p.Event.Time)
                .FirstOrDefault();

            if (pendingType2 != null)
            {
                _pendingType2Events.Remove(pendingType2);
                
                _ = Task.Run(async () =>
                {
                    await CreateIncidentAsync(IncidentType.Type2, 
                        new List<Event> { pendingType2.Event, eventData });
                });
            }
        }
    }

    private async Task HandleType2EventAsync(Event eventData)
    {
        await SaveEventAsync(eventData);
        
        lock (_lock)
        {
            _pendingType2Events.Add(new PendingEvent
            {
                Event = eventData,
                ExpiresAt = DateTime.UtcNow.AddSeconds(20)
            });
        }
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(20));
            await CheckType2TimeoutAsync(eventData);
        });
    }

    private async Task HandleType3EventAsync(Event eventData)
    {
        await SaveEventAsync(eventData);
        
        lock (_lock)
        {
            _pendingType3Events.Add(new PendingEvent
            {
                Event = eventData,
                ExpiresAt = DateTime.UtcNow.AddSeconds(60)
            });
        }
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(60));
            await CheckType3TimeoutAsync(eventData);
        });
    }

    private async Task CheckType2TimeoutAsync(Event eventData)
    {
        lock (_lock)
        {
            var pending = _pendingType2Events.FirstOrDefault(p => p.Event.Id == eventData.Id);
            if (pending != null)
            {
                _pendingType2Events.Remove(pending);
                
                _ = Task.Run(async () =>
                {
                    await CreateIncidentAsync(IncidentType.Type1, new List<Event> { eventData });
                });
            }
        }
    }

    private async Task CheckType3TimeoutAsync(Event eventData)
    {
        lock (_lock)
        {
            var pending = _pendingType3Events.FirstOrDefault(p => p.Event.Id == eventData.Id);
            if (pending != null)
            {
                _pendingType3Events.Remove(pending);
                
                _ = Task.Run(async () =>
                {
                    await CreateIncidentAsync(IncidentType.Type1, new List<Event> { eventData });
                });
            }
        }
    }

    private async Task OnIncidentType2CreatedAsync()
    {
        List<Event> eventsToProcess;
        
        lock (_lock)
        {
            eventsToProcess = _pendingType3Events
                .Where(p => (DateTime.UtcNow - p.Event.Time).TotalSeconds <= 60)
                .Select(p => p.Event)
                .ToList();

            _pendingType3Events.RemoveAll(p => eventsToProcess.Contains(p.Event));
        }

        foreach (var eventData in eventsToProcess)
        {
            await CreateIncidentAsync(IncidentType.Type3, new List<Event> { eventData });
        }
    }

    private async Task SaveEventAsync(Event eventData)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var existingEvent = await context.Events.FindAsync(eventData.Id);
        if (existingEvent == null)
        {
            context.Events.Add(eventData);
            await context.SaveChangesAsync();
        }
    }

    private async Task CreateIncidentAsync(IncidentType type, List<Event> events)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Type = type,
            Time = DateTime.UtcNow
        };

        context.Incidents.Add(incident);

        foreach (var evt in events)
        {
            context.IncidentEvents.Add(new IncidentEvent
            {
                IncidentId = incident.Id,
                EventId = evt.Id
            });
        }

        await context.SaveChangesAsync();

        _logger.LogInformation(
            $"Created Incident {incident.Id} of type {type} based on {events.Count} event(s)");
        
        if (type == IncidentType.Type2)
        {
            await OnIncidentType2CreatedAsync();
        }
    }

    private void CleanupExpiredPendingEvents()
    {
        var now = DateTime.UtcNow;
        _pendingType2Events.RemoveAll(p => p.ExpiresAt < now);
        _pendingType3Events.RemoveAll(p => p.ExpiresAt < now);
    }

    public async Task<List<Incident>> GetIncidentsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        return await context.Incidents
            .Include(i => i.IncidentEvents)
            .ThenInclude(ie => ie.Event)
            .OrderByDescending(i => i.Time)
            .ToListAsync();
    }
}

public class PendingEvent
{
    public Event Event { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}