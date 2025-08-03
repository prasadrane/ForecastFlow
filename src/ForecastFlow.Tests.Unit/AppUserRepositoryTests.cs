using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ForecastFlow.Api.Data;
using ForecastFlow.Api.Data.Repository;
using ForecastFlow.Core.Models;

namespace ForecastFlow.Tests.Unit;

public class AppUserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AppUserRepository _repository;

    public AppUserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        _context = new ApplicationDbContext(options, mockConfiguration.Object);
        _repository = new AppUserRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new[]
        {
            new AppUser { Username = "user1", Email = "user1@test.com" },
            new AppUser { Username = "user2", Email = "user2@test.com" }
        };
        
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, u => u.Username == "user1");
        Assert.Contains(result, u => u.Username == "user2");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var user = new AppUser { Username = "testuser", Email = "test@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@test.com", result.Email);
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
    public async Task GetByUsernameAsync_WithValidUsername_ShouldReturnUser()
    {
        // Arrange
        var user = new AppUser { Username = "testuser", Email = "test@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@test.com", result.Email);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = new AppUser { Username = "newuser", Email = "new@test.com" };

        // Act
        await _repository.AddAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        Assert.NotNull(savedUser);
        Assert.Equal("newuser", savedUser.Username);
        Assert.Equal("new@test.com", savedUser.Email);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingUser()
    {
        // Arrange
        var user = new AppUser { Username = "testuser", Email = "test@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        user.Email = "updated@test.com";
        await _repository.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal("updated@test.com", updatedUser.Email);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = new AppUser { Username = "testuser", Email = "test@test.com" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user.Id);

        // Assert
        var deletedUser = await _context.Users.FindAsync(user.Id);
        Assert.Null(deletedUser);
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