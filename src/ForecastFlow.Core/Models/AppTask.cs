namespace ForecastFlow.Core.Models;

public class AppTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime TaskDateTime { get; set; }
    public int UserId { get; set; }

    // Additional properties
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? Priority { get; set; }
    public string? Category { get; set; }
    public DateTime? ReminderDateTime { get; set; }
}