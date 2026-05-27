namespace ForecastFlow.Core.Interfaces;

public interface IWeatherService
{
    Task<string> GetWeatherForecastAsync(double latitude, double longitude);
}
