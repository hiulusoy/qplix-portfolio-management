using Microsoft.AspNetCore.Mvc;
using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Services.Cities.Dtos;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence;

namespace qplix_portfolio_management.API.Endpoints;

/// <summary>
/// Contains API endpoints related to city operations
/// </summary>
public static class CityEndpoints
{
    /// <summary>
    /// Registers all city-related endpoints with the web application
    /// </summary>
    /// <param name="app">The endpoint route builder to register endpoints with</param>
    public static void MapCityEndpoints(this IEndpointRouteBuilder app)
    {
        // Create a group for all city endpoints with shared tag for Swagger documentation
        var group = app.MapGroup("/api/cities")
            .WithTags("Cities");

        // GET all cities
        group.MapGet("/", async ([FromServices] ICityService cityService, CancellationToken cancellationToken) =>
            {
                // Retrieve all cities from the database
                var cities = await cityService.GetAllCitiesAsync(cancellationToken);
                return Results.Ok(cities);
            })
            .WithName("GetAllCities")
            .WithOpenApi()
            .Produces<IEnumerable<GetCityResponseDto>>(StatusCodes.Status200OK);

        // GET city by ID
        group.MapGet("/{id}",
                async (int id, [FromServices] ICityService cityService, CancellationToken cancellationToken) =>
                {
                    // Retrieve specific city by its ID
                    var city = await cityService.GetCityByIdAsync(id, cancellationToken);

                    // Return 404 if city not found
                    if (city == null)
                        return Results.NotFound();

                    return Results.Ok(city);
                })
            .WithName("GetCityById")
            .WithOpenApi()
            .Produces<City>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST create new city
        group.MapPost("/",
                async ([FromBody] City city, [FromServices] ICityService cityService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        // Create new city in the database
                        var newCity = await cityService.CreateCityAsync(city, cancellationToken);

                        // Return 201 Created with the location header pointing to the new resource
                        return Results.Created($"/api/cities/{newCity.CityId}", newCity);
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle validation errors
                        return Results.BadRequest(ex.Message);
                    }
                })
            .WithName("CreateCity")
            .WithOpenApi()
            .Produces<City>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        // PUT update city
        group.MapPut("/{id}",
                async (int id, [FromBody] City city, [FromServices] ICityService cityService,
                    CancellationToken cancellationToken) =>
                {
                    try
                    {
                        // Update existing city with new information
                        var updatedCity = await cityService.UpdateCityAsync(id, city, cancellationToken);

                        // Return 404 if city not found
                        if (updatedCity == null)
                            return Results.NotFound();

                        return Results.Ok(updatedCity);
                    }
                    catch (ArgumentException ex)
                    {
                        // Handle validation errors
                        return Results.BadRequest(ex.Message);
                    }
                })
            .WithName("UpdateCity")
            .WithOpenApi()
            .Produces<City>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        // DELETE city
        group.MapDelete("/{id}",
                async (int id, [FromServices] ICityService cityService, CancellationToken cancellationToken) =>
                {
                    // Delete city from the database
                    var result = await cityService.DeleteCityAsync(id, cancellationToken);

                    // Return 404 if city not found
                    if (!result)
                        return Results.NotFound();

                    // Return 204 No Content for successful deletion
                    return Results.NoContent();
                })
            .WithName("DeleteCity")
            .WithOpenApi()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}