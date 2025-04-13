using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Services.Portfolios.Dtos;
using Microsoft.AspNetCore.Mvc;


namespace qplix_portfolio_management.API.Endpoints
{
    /// <summary>
    /// Contains API endpoints related to investment advice operations
    /// </summary>
    public static class AIAdvisorEndpoints
    {
        /// <summary>
        /// Registers all advisor-related endpoints with the web application
        /// </summary>
        /// <param name="app">The web application to register endpoints with</param>
        public static void MapAIAdvisorEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/advisor")
                .WithTags("Advisor");

            // Endpoint to generate advice from portfolio data
            group.MapPost("/advice", async (
                    [FromBody] PortfolioValueResponseDto portfolioData,
                    [FromServices] IAiAdvisorService advisorService,
                    [FromServices] ILogger<Program> logger,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        // Log the incoming request
                        logger.LogInformation(
                            "Investment advice requested for portfolio data of investor {InvestorId}",
                            portfolioData.InvestorId);

                        // Validate the portfolio data
                        if (portfolioData.InvestmentValues == null || portfolioData.InvestmentValues.Count == 0)
                        {
                            logger.LogWarning("Portfolio data is empty for investor {InvestorId}",
                                portfolioData.InvestorId);
                            return Results.BadRequest(new
                            {
                                Error = "Portfolio data is empty. Cannot generate investment advice."
                            });
                        }

                        // Generate investment advice
                        var result =
                            await advisorService.GenerateAdviceFromPortfolioAsync(portfolioData, cancellationToken);
                        return Results.Ok(result);
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions
                        logger.LogError(ex, "Error generating investment advice for investor {InvestorId}",
                            portfolioData?.InvestorId);

                        return Results.Problem(
                            title: "Error generating investment advice",
                            detail: ex.Message,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
                .WithName("GenerateAdviceFromPortfolio")
                .WithOpenApi()
                .Produces<Application.Services.Advisors.Dtos.InvestmentAdviceDto>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError);

            // Endpoint to get advice for a specific portfolio (convenience method)
            group.MapGet("/portfolio/{investorId}/advice", async (
                    [FromRoute] int investorId,
                    [FromQuery] DateTime? referenceDate,
                    [FromServices] IPortfolioCalculationService portfolioService,
                    [FromServices] IAiAdvisorService advisorService,
                    [FromServices] ILogger<Program> logger,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        // Log the incoming request
                        logger.LogInformation(
                            "Combined portfolio calculation and advice requested for investor {InvestorId} with referenceDate {ReferenceDate}",
                            investorId, referenceDate);

                        // If referenceDate is not specified, use today's date
                        var calculationDate = referenceDate ?? DateTime.UtcNow.Date;

                        // Validate the reference date
                        if (calculationDate < new DateTime(2000, 1, 1) || calculationDate > DateTime.UtcNow.Date)
                        {
                            logger.LogWarning("Invalid reference date: {ReferenceDate}", calculationDate);
                            return Results.BadRequest(new
                            {
                                Error = "Invalid reference date. Date must be between 2000-01-01 and today."
                            });
                        }

                        // First, calculate the portfolio value
                        var portfolioData =
                            await portfolioService.CalculatePortfolioValueAsync(investorId, calculationDate);

                        // Then, generate advice based on the portfolio data
                        var advice =
                            await advisorService.GenerateAdviceFromPortfolioAsync(portfolioData, cancellationToken);

                        // Return combined result
                        var result = new
                        {
                            Portfolio = portfolioData,
                            Advice = advice
                        };

                        return Results.Ok(result);
                    }
                    catch (Exception ex) when (ex.Message.Contains("not found"))
                    {
                        logger.LogWarning(ex, "Resource not found");
                        return Results.NotFound(new { Error = ex.Message });
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions
                        logger.LogError(ex, "Error generating portfolio advice");
                        return Results.Problem(
                            title: "Error generating portfolio advice",
                            detail: ex.Message,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
                .WithName("GetPortfolioAdvice")
                .WithOpenApi()
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}