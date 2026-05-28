using ForecastFlow.Core.Interfaces;
using ForecastFlow.Core.Models;
using ForecastFlow.Lambda;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ForecastFlow.Api.Services;

public class MockSqsBackgroundWorker : BackgroundService
{
    private readonly IMessageQueueService _queueService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MockSqsBackgroundWorker> _logger;

    public MockSqsBackgroundWorker(
        IMessageQueueService queueService,
        IServiceProvider serviceProvider,
        ILogger<MockSqsBackgroundWorker> logger)
    {
        _queueService = queueService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Mock SQS Background Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await _queueService.ReceiveMessageAsync<WeatherProcessingMessage>();
            if (message != null)
            {
                _logger.LogInformation("Received message from mock queue: {MessageType} for User {UserId}", message.MessageType, message.UserId);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
                        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                        // Assuming the API is running on localhost
                        // In a real scenario, this would be the actual API URL
                        var apiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5089";

                        var processor = new WeatherProcessor(
                            weatherService,
                            _logger,
                            httpClientFactory.CreateClient(),
                            apiBaseUrl);

                        if (message.MessageType == "TaskSuggestion")
                        {
                            await processor.ProcessTaskSuggestionAsync(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing mock SQS message");
                }
            }

            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Mock SQS Background Worker is stopping.");
    }
}
