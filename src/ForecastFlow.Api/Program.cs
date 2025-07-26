using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ForecastFlow.Api.Data;
using ForecastFlow.Api.Data.Repository;

var builder = WebApplication.CreateBuilder(args);

// Load appsettings.Secrets.json for local dev
builder.Configuration
    .AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

// Retrieve JWT key from AWS Secrets Manager if in production
if (builder.Environment.IsProduction())
{
    var secretName = "ForecastFlow_JwtKey";
    var region = "us-east-1"; // Change to your AWS region

    var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));
    var request = new GetSecretValueRequest { SecretId = secretName };
    var response = await client.GetSecretValueAsync(request);
    var jwtKey = response.SecretString;

    builder.Configuration["Jwt:Key"] = jwtKey;
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext and repositories
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddScoped<AppUserRepository>();
builder.Services.AddScoped<AppTaskRepository>();

// JWT Authentication configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
