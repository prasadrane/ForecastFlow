using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SecretsManager;
using Microsoft.Extensions.Logging;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ForecastFlow.Lambda;

public class WeatherProcessingFunction
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly ILogger<WeatherProcessingFunction> _logger;
    private readonly HttpClient _httpClient;

    public WeatherProcessingFunction()
    {
        _sqsClient = new AmazonSQSClient();
        _secretsManager = new AmazonSecretsManagerClient();
        _httpClient = new HttpClient();
        
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<WeatherProcessingFunction>();
    }

    /// <summary>
    /// Lambda function handler for processing SQS messages containing weather-related tasks
    /// </summary>
    /// <param name="evnt">SQS event containing messages to process</param>
    /// <param name="context">Lambda context</param>
    /// <returns>Task</returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        _logger.LogInformation($"Processing {evnt.Records.Count} SQS messages");

        foreach (var record in evnt.Records)
        {
            try
            {
                await ProcessMessageAsync(record, context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message {record.MessageId}");
                throw; // Re-throw to ensure message goes to DLQ after retries
            }
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        _logger.LogInformation($"Processing message: {message.MessageId}");

        try
        {
            // Parse the message body
            var messageData = JsonSerializer.Deserialize<WeatherProcessingMessage>(message.Body);
            if (messageData == null)
            {
                _logger.LogWarning($"Failed to deserialize message {message.MessageId}");
                return;
            }

            // Process based on message type
            switch (messageData.MessageType)
            {
                case "WeatherUpdate":
                    await ProcessWeatherUpdateAsync(messageData);
                    break;
                case "TaskSuggestion":
                    await ProcessTaskSuggestionAsync(messageData);
                    break;
                default:
                    _logger.LogWarning($"Unknown message type: {messageData.MessageType}");
                    break;
            }

            _logger.LogInformation($"Successfully processed message {message.MessageId}");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"Failed to parse JSON for message {message.MessageId}");
            throw;
        }
    }

    private async Task ProcessWeatherUpdateAsync(WeatherProcessingMessage message)
    {
        _logger.LogInformation($"Processing weather update for location: {message.Location}");
        
        // Here you would:
        // 1. Fetch current weather data from OpenWeatherMap API
        // 2. Update database with new weather information
        // 3. Check for weather alerts that might affect scheduled tasks
        // 4. Send notifications if needed
        
        // For now, just log the processing
        await Task.Delay(100); // Simulate processing time
        _logger.LogInformation("Weather update processed successfully");
    }

    private async Task ProcessTaskSuggestionAsync(WeatherProcessingMessage message)
    {
        _logger.LogInformation($"Processing task suggestion for user: {message.UserId}");
        
        // Here you would:
        // 1. Analyze user's scheduled tasks
        // 2. Get weather forecast for task locations
        // 3. Generate weather-appropriate task suggestions
        // 4. Update task recommendations in database
        
        // For now, just log the processing
        await Task.Delay(100); // Simulate processing time
        _logger.LogInformation("Task suggestion processed successfully");
    }
}

public class WeatherProcessingMessage
{
    public string MessageType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}