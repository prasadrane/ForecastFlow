using System.Net.Http.Json;
using ForecastFlow.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ForecastFlow.Api.Services;

public class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<OpenWeatherService> _logger;

    public OpenWeatherService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenWeatherService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenWeatherMap:ApiKey"] ?? throw new InvalidOperationException("OpenWeatherMap ApiKey is missing");
        _logger = logger;
    }

    public async Task<string> GetWeatherForecastAsync(double latitude, double longitude)
    {
        try
        {
            // For the "happy path" and since we are told to generate mock data if needed,
            // we'll use the API key if possible, but handle failures gracefully.
            var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";

            _logger.LogInformation("Fetching weather from OpenWeatherMap for {Lat}, {Lon}", latitude, longitude);

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            _logger.LogWarning("OpenWeatherMap API returned {StatusCode}. Falling back to mock data.", response.StatusCode);
            return GetMockWeatherData(latitude, longitude);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather from OpenWeatherMap. Falling back to mock data.");
            return GetMockWeatherData(latitude, longitude);
        }
    }

    private string GetMockWeatherData(double lat, double lon)
    {
        // Simple mock response structure similar to OpenWeatherMap
        return $@"
        {{
            ""list"": [
                {{
                    ""dt"": {DateTimeOffset.UtcNow.ToUnixTimeSeconds()},
                    ""main"": {{ ""temp"": 22.5, ""humidity"": 60 }},
                    ""weather"": [ {{ ""main"": ""Clear"", ""description"": ""clear sky"" }} ]
                }}
            ],
            ""city"": {{ ""name"": ""Mock City"" }}
        }}";
    }
}
