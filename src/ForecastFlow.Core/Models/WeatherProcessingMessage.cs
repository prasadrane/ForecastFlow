namespace ForecastFlow.Core.Models;

public class WeatherProcessingMessage
{
    public string MessageType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}
