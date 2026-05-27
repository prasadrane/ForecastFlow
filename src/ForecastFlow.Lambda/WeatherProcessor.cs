using System.Net.Http.Json;
using System.Text.Json;
using ForecastFlow.Core.Models;
using ForecastFlow.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ForecastFlow.Lambda;

public class WeatherProcessor
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public WeatherProcessor(IWeatherService weatherService, ILogger logger, HttpClient httpClient, string apiBaseUrl)
    {
        _weatherService = weatherService;
        _logger = logger;
        _httpClient = httpClient;
        _apiBaseUrl = apiBaseUrl;
    }

    public async Task ProcessTaskSuggestionAsync(WeatherProcessingMessage message)
    {
        _logger.LogInformation($"Processing task suggestion for task: {message.Data["TaskId"]}");

        try
        {
            var taskId = int.Parse(message.Data["TaskId"].ToString()!);
            var lat = double.Parse(message.Data["Latitude"].ToString()!);
            var lon = double.Parse(message.Data["Longitude"].ToString()!);
            var taskDateTime = DateTime.Parse(message.Data["TaskDateTime"].ToString()!);

            // 1. Get Weather Forecast
            var weatherJson = await _weatherService.GetWeatherForecastAsync(lat, lon);

            // 2. Generate Suggestions (Happy Path Logic)
            var suggestions = GenerateSuggestions(weatherJson, taskDateTime);

            // 3. Send data back to API
            await UpdateTaskSuggestionsInApi(taskId, suggestions);

            _logger.LogInformation("Task suggestion processed and sent to API successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WeatherProcessor.ProcessTaskSuggestionAsync");
            throw;
        }
    }

    private List<string> GenerateSuggestions(string weatherJson, DateTime taskDateTime)
    {
        var suggestions = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(weatherJson);
            var list = doc.RootElement.GetProperty("list");

            // Find forecast closest to taskDateTime
            // For now, let's just take the first one or look at the "weather" main
            var mainWeather = list[0].GetProperty("weather")[0].GetProperty("main").GetString();
            var temp = list[0].GetProperty("main").GetProperty("temp").GetDouble();

            suggestions.Add($"Expected weather: {mainWeather} with temperature {temp}°C.");

            if (mainWeather == "Rain" || mainWeather == "Drizzle" || mainWeather == "Thunderstorm")
            {
                suggestions.Add("It might rain. Consider bringing an umbrella or moving the activity indoors.");
            }
            else if (temp > 30)
            {
                suggestions.Add("It's going to be very hot. Stay hydrated and use sunscreen.");
            }
            else if (temp < 5)
            {
                suggestions.Add("It's going to be cold. Dress warmly.");
            }
            else if (mainWeather == "Clear")
            {
                suggestions.Add("Great weather for outdoor activities!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse weather data for suggestions. Using defaults.");
            suggestions.Add("Weather data unavailable. Please check the forecast manually.");
        }

        return suggestions;
    }

    private async Task UpdateTaskSuggestionsInApi(int taskId, List<string> suggestions)
    {
        var url = $"{_apiBaseUrl}/api/tasks/{taskId}/suggestions";
        var content = JsonContent.Create(new { Suggestions = suggestions });

        var response = await _httpClient.PatchAsync(url, content);
        response.EnsureSuccessStatusCode();
    }
}
