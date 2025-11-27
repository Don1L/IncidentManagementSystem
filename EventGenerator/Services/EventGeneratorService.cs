using Shared.DTOs;
using Shared.Enums;

namespace EventGenerator.Services;

public class EventGeneratorService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventGeneratorService> _logger;
    private readonly Random _random = new();

    public EventGeneratorService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<EventGeneratorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventGeneratorService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = _random.Next(0, 2000);
                await Task.Delay(delay, stoppingToken);

                // Генерируем событие
                var eventDto = GenerateRandomEvent();
                
                // Отправляем в Processor
                await SendEventToProcessor(eventDto);
                
                _logger.LogInformation(
                    $"Auto-generated and sent event: {eventDto.Id}, Type: {eventDto.Type}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EventGeneratorService");
            }
        }
    }

    public EventDto GenerateRandomEvent()
    {
        // Случайный тип события (1, 2 или 3)
        var eventType = (EventType)_random.Next(1, 4);

        return new EventDto
        {
            Id = Guid.NewGuid(),
            Type = eventType,
            Time = DateTime.UtcNow
        };
    }

    public async Task SendEventToProcessor(EventDto eventDto)
    {
        var processorUrl = _configuration["ProcessorUrl"];
        var client = _httpClientFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"{processorUrl}/api/events", 
            eventDto);

        response.EnsureSuccessStatusCode();
    }
}