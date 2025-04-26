using qplix_portfolio_management.Persistence;
using qplix_portfolio_management.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using qplix_portfolio_management.API.Endpoints;
using qplix_portfolio_management.Application;
using qplix_portfolio_management.Infrastructure;

// Create a new web application builder instance
var builder = WebApplication.CreateBuilder(args);

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllTraffic", policy =>
    {
        policy.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure Swagger documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Qplix Portfolio Management API",
        Version = "v1",
        Description = "A simple API for portfolio management"
    });
});

// Add OpenAPI support for API documentation
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register persistence layer services including database context
builder.Services.AddPersistenceServices(builder.Configuration);

// Register repositories for data access
builder.Services.RegisterRepositories();

// Register application services
builder.Services.RegisterServices();

// Register external services
builder.Services.RegisterExternalServices();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline based on environment
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI in development environment
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Qplix Portfolio Management API v1"));

    // Map OpenAPI endpoints
    app.MapOpenApi();
}
// Dummy commit

app.UseCors("AllowAllTraffic");

// Enforce HTTPS redirection for security
app.UseHttpsRedirection();

// Map a simple health check endpoint
app.MapGet("/ping", () => "Pong! Server is running");

// Register domain-specific endpoints from endpoint extension classes
app.MapCityEndpoints(); // Register all city-related endpoints
app.MapPortfolioEndpoints(); // Register all portfolio-related endpoints
app.MapAIAdvisorEndpoints(); // Register all AI related endpoints
app.MapInvestorEndpoints(); // Register all Investor related endpoints

// Start the application
app.Run();