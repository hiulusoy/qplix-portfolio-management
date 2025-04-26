using qplix_portfolio_management.Application.Abstractions.Persistence;

namespace qplix_portfolio_management.API.Endpoints;

using Microsoft.AspNetCore.Mvc;
using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Services.Investors.Dtos;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence;

/// <summary>
/// Contains API endpoints related to investor operations
/// </summary>
public static class InvestorEndpoints
{
    /// <summary>
    /// Registers all investor-related endpoints with the web application
    /// </summary>
    /// <param name="app">The endpoint route builder to register endpoints with</param>
    public static void MapInvestorEndpoints(this IEndpointRouteBuilder app)
    {
        // Create a group for all investor endpoints with shared tag for Swagger documentation
        var group = app.MapGroup("/api/investors")
            .WithTags("Investors");

        // GET all investors
        group.MapGet("/",
                async ([FromServices] IInvestorService investorService, CancellationToken cancellationToken) =>
                {
                    // Retrieve all investors from the database
                    var investors = await investorService.GetAllInvestorsAsync(cancellationToken);
                    return Results.Ok(investors);
                })
            .WithName("GetAllInvestors")
            .WithOpenApi()
            .Produces<IEnumerable<InvestorResponseDto>>(StatusCodes.Status200OK);

        // GET investor by ID
        group.MapGet("/{id}",
                async (int id, [FromServices] IInvestorService investorService, CancellationToken cancellationToken) =>
                {
                    // Retrieve specific investor by its ID
                    var investor = await investorService.GetInvestorByIdAsync(id, cancellationToken);

                    // Return 404 if investor not found
                    if (investor == null)
                        return Results.NotFound();

                    return Results.Ok(investor);
                })
            .WithName("GetInvestorById")
            .WithOpenApi()
            .Produces<Investor>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST create new investor
        group.MapPost("/",
                async ([FromBody] Investor investor, [FromServices] IInvestorService investorService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        // Create new investor in the database
                        var newInvestor = await investorService.CreateInvestorAsync(investor, cancellationToken);

                        // Return 201 Created with the location header pointing to the new resource
                        return Results.Created($"/api/investors/{newInvestor.InvestorId}", newInvestor);
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle validation errors
                        return Results.BadRequest(ex.Message);
                    }
                })
            .WithName("CreateInvestor")
            .WithOpenApi()
            .Produces<Investor>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // PUT update investor
        group.MapPut("/{id}",
                async (int id, [FromBody] Investor investor, [FromServices] IInvestorService investorService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        // Update existing investor with new information
                        var updatedInvestor =
                            await investorService.UpdateInvestorAsync(id, investor, cancellationToken);

                        // Return 404 if investor not found
                        if (updatedInvestor == null)
                            return Results.NotFound();

                        return Results.Ok(updatedInvestor);
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle validation errors
                        return Results.BadRequest(ex.Message);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Handle business rule violations
                        return Results.BadRequest(ex.Message);
                    }
                })
            .WithName("UpdateInvestor")
            .WithOpenApi()
            .Produces<Investor>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // DELETE investor
        group.MapDelete("/{id}",
                async (int id, [FromServices] IInvestorService investorService, CancellationToken cancellationToken) =>
                {
                    try
                    {
                        // Delete investor from the database
                        var result = await investorService.DeleteInvestorAsync(id, cancellationToken);

                        // Return 404 if investor not found
                        if (!result)
                            return Results.NotFound();

                        // Return 204 No Content for successful deletion
                        return Results.NoContent();
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Handle when investor has related investments
                        return Results.BadRequest(ex.Message);
                    }
                })
            .WithName("DeleteInvestor")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // GET investor investments
        group.MapGet("/{id}/investments",
                async (int id, [FromServices] IInvestorService investorService,
                    [FromServices] IInvestorRepository investorRepository, CancellationToken cancellationToken) =>
                {
                    // Check if investor exists
                    var investor = await investorService.GetInvestorByIdAsync(id, cancellationToken);
                    if (investor == null)
                        return Results.NotFound();

                    // Retrieve investments for the specific investor
                    var investments = await investorRepository.GetInvestorInvestmentsAsync(id);
                    return Results.Ok(investments);
                })
            .WithName("GetInvestorInvestments")
            .WithOpenApi()
            .Produces<IEnumerable<InvestorInvestment>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}