using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using ForecastFlow.Api.Controllers;
using ForecastFlow.Core.Interfaces;
using ForecastFlow.Core.Models;

namespace ForecastFlow.Tests.Unit;

public class AuthControllerTests
{
    private readonly Mock<IAppUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockJwtSection;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockUserRepository = new Mock<IAppUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockJwtSection = new Mock<IConfigurationSection>();

        // Setup JWT configuration mock
        _mockJwtSection.Setup(x => x["Key"]).Returns("ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast256BitsLong");
        _mockJwtSection.Setup(x => x["Issuer"]).Returns("ForecastFlow");
        _mockJwtSection.Setup(x => x["Audience"]).Returns("ForecastFlow");
        _mockJwtSection.Setup(x => x["ExpiresInMinutes"]).Returns("60");
        _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(_mockJwtSection.Object);

        _controller = new AuthController(_mockUserRepository.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "testuser", Password = "password123" };
        
        using var hmac = new System.Security.Cryptography.HMACSHA512();
        var user = new AppUser
        {
            Id = 1,
            Username = "testuser",
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123"))
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        var tokenProperty = response.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);
        var token = tokenProperty.GetValue(response) as string;
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "nonexistent", Password = "password123" };
        
        _mockUserRepository.Setup(x => x.GetByUsernameAsync("nonexistent"))
            .ReturnsAsync((AppUser?)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid username or password.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "testuser", Password = "wrongpassword" };
        
        using var hmac = new System.Security.Cryptography.HMACSHA512();
        var user = new AppUser
        {
            Id = 1,
            Username = "testuser",
            PasswordSalt = hmac.Key,
            PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("correctpassword"))
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid username or password.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Register_WithNewUsername_ShouldReturnSuccess()
    {
        // Arrange
        var registerDto = new UserForRegisterDto 
        { 
            Username = "newuser", 
            Password = "password123",
            Email = "newuser@test.com"
        };

        _mockUserRepository.Setup(x => x.GetByUsernameAsync("newuser"))
            .ReturnsAsync((AppUser?)null);
        
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<AppUser>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        var messageProperty = response.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        var message = messageProperty.GetValue(response) as string;
        Assert.Equal("User registered successfully.", message);

        _mockUserRepository.Verify(x => x.AddAsync(It.Is<AppUser>(u => 
            u.Username == "newuser" && 
            u.Email == "newuser@test.com" &&
            u.PasswordHash.Length > 0 &&
            u.PasswordSalt.Length > 0)), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var registerDto = new UserForRegisterDto 
        { 
            Username = "existinguser", 
            Password = "password123",
            Email = "existing@test.com"
        };

        var existingUser = new AppUser { Username = "existinguser" };
        _mockUserRepository.Setup(x => x.GetByUsernameAsync("existinguser"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username is already taken.", badRequestResult.Value);
        
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<AppUser>()), Times.Never);
    }
}