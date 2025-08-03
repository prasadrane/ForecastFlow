using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ForecastFlow.Api.Data;
using ForecastFlow.Api.Data.Repository;
using ForecastFlow.Core.Models;

namespace ForecastFlow.Tests.Unit;

public class AppTaskRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AppTaskRepository _repository;

    public AppTaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        _context = new ApplicationDbContext(options, mockConfiguration.Object);
        _repository = new AppTaskRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTasks()
    {
        // Arrange
        var tasks = new[]
        {
            new AppTask { Title = "Task 1", LocationName = "Location 1", UserId = 1 },
            new AppTask { Title = "Task 2", LocationName = "Location 2", UserId = 2 }
        };
        
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Title == "Task 1");
        Assert.Contains(result, t => t.Title == "Task 2");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var task = new AppTask { Title = "Test Task", LocationName = "Test Location", UserId = 1 };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Location", result.LocationName);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnTasksForSpecificUser()
    {
        // Arrange
        var tasks = new[]
        {
            new AppTask { Title = "User 1 Task 1", LocationName = "Location 1", UserId = 1 },
            new AppTask { Title = "User 1 Task 2", LocationName = "Location 2", UserId = 1 },
            new AppTask { Title = "User 2 Task 1", LocationName = "Location 3", UserId = 2 }
        };
        
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(1, t.UserId));
        Assert.Contains(result, t => t.Title == "User 1 Task 1");
        Assert.Contains(result, t => t.Title == "User 1 Task 2");
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNoTasks_ShouldReturnEmptyCollection()
    {
        // Act
        var result = await _repository.GetByUserIdAsync(999);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTaskToDatabase()
    {
        // Arrange
        var task = new AppTask 
        { 
            Title = "New Task", 
            LocationName = "New Location",
            UserId = 1,
            Latitude = 40.7128,
            Longitude = -74.0060,
            TaskDateTime = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await _repository.AddAsync(task);

        // Assert
        var savedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == "New Task");
        Assert.NotNull(savedTask);
        Assert.Equal("New Task", savedTask.Title);
        Assert.Equal("New Location", savedTask.LocationName);
        Assert.Equal(1, savedTask.UserId);
        Assert.Equal(40.7128, savedTask.Latitude, 4);
        Assert.Equal(-74.0060, savedTask.Longitude, 4);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingTask()
    {
        // Arrange
        var task = new AppTask { Title = "Original Title", LocationName = "Original Location", UserId = 1 };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        task.Title = "Updated Title";
        task.LocationName = "Updated Location";
        task.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(task);

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("Updated Location", updatedTask.LocationName);
        Assert.NotNull(updatedTask.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTaskFromDatabase()
    {
        // Arrange
        var task = new AppTask { Title = "Task to Delete", LocationName = "Location", UserId = 1 };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(task.Id);

        // Assert
        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldNotThrow()
    {
        // Act & Assert
        await _repository.DeleteAsync(999); // Should not throw
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}