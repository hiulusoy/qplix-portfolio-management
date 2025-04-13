using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Services.Portfolios.Dtos;

namespace qplix_portfolio_management.API.Endpoints;

/// <summary>
/// Contains API endpoints related to portfolio operations
/// </summary>
public static class PortfolioEndpoints
{
    /// <summary>
    /// Registers all portfolio-related endpoints with the web application
    /// </summary>
    /// <param name="app">The web application to register endpoints with</param>
    public static void MapPortfolioEndpoints(this WebApplication app)
    {
        // Endpoint to get portfolio value for a specific investor
        app.MapGet("/api/portfolio/investor/{investorId}/value", async (
                [FromRoute] int investorId,
                [FromQuery] DateTime? referenceDate,
                [FromServices] IPortfolioCalculationService portfolioService,
                [FromServices] ILogger<Program> logger) =>
            {
                try
                {
                    // Log the incoming request
                    logger.LogInformation(
                        "Portfolio value calculation requested for investor {InvestorId} with referenceDate {ReferenceDate}",
                        investorId, referenceDate);

                    // If referenceDate is not specified, use today's date
                    var calculationDate = referenceDate ?? DateTime.UtcNow.Date;

                    // Validate the reference date - reject dates before 2000-01-01 or future dates
                    if (calculationDate < new DateTime(2000, 1, 1) || calculationDate > DateTime.UtcNow.Date)
                    {
                        logger.LogWarning("Invalid reference date: {ReferenceDate}", calculationDate);
                        return Results.BadRequest(new
                        {
                            Error = "Invalid reference date. Date must be between 2000-01-01 and today."
                        });
                    }

                    // Calculate the portfolio value using the portfolio service
                    var result = await portfolioService.CalculatePortfolioValueAsync(investorId, calculationDate);
                    return Results.Ok(result);
                }
                catch (Exception ex) when (ex.Message.Contains("not found"))
                {
                    // Handle resource not found exceptions (like investor not found)
                    logger.LogWarning(ex, "Resource not found");
                    return Results.NotFound(new { Error = ex.Message });
                }
                catch (Exception ex)
                {
                    // Handle all other exceptions
                    logger.LogError(ex, "Error calculating portfolio value");
                    return Results.Problem(
                        title: "Error calculating portfolio value",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            // Configure API documentation and response types
            .WithName("GetPortfolioValue") // Operation name for the endpoint
            .WithTags("Portfolios") // Group the endpoint under "Portfolios" tag in Swagger
            .WithOpenApi() // Generate OpenAPI documentation
            .Produces<PortfolioValueResponseDto>(StatusCodes.Status200OK) // Successful response type
            .Produces(StatusCodes.Status400BadRequest) // Bad request response
            .Produces(StatusCodes.Status404NotFound) // Not found response
            .Produces(StatusCodes.Status500InternalServerError); // Server error response
    }
}