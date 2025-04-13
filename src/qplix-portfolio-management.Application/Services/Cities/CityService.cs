using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Application.Services.Cities.Dtos;
using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Services.Cities;

/// <summary>
/// Service implementation for managing city-related operations
/// </summary>
public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;

    /// <summary>
    /// Constructor for CityService
    /// </summary>
    /// <param name="cityRepository">Repository for city data access</param>
    /// <exception cref="ArgumentNullException">Thrown when repository is null</exception>
    public CityService(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
    }

    /// <summary>
    /// Retrieves all cities from the database
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>Collection of city DTOs</returns>
    public async Task<IEnumerable<GetCityResponseDto>> GetAllCitiesAsync(CancellationToken cancellationToken = default)
    {
        // Get all cities from the repository
        var cities = await _cityRepository.GetAllAsync(cancellationToken);

        // Map entity objects to DTOs
        return cities.Select(city => new GetCityResponseDto()
        {
            CityId = city.CityId,
            CityCode = city.CityCode,
            CityName = city.CityName
        }).ToList();
    }

    /// <summary>
    /// Retrieves a specific city by its ID
    /// </summary>
    /// <param name="id">ID of the city to retrieve</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The city if found, null otherwise</returns>
    public async Task<City?> GetCityByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _cityRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Creates a new city in the database
    /// </summary>
    /// <param name="city">City entity to create</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The created city with assigned ID</returns>
    /// <exception cref="ArgumentException">Thrown when city data is invalid</exception>
    public async Task<City> CreateCityAsync(City city, CancellationToken cancellationToken = default)
    {
        // Validate city data before saving
        if (string.IsNullOrWhiteSpace(city.CityCode))
        {
            throw new ArgumentException("City code cannot be empty", nameof(city));
        }

        // Add the city to the database
        return await _cityRepository.AddAsync(city, cancellationToken);
    }

    /// <summary>
    /// Updates an existing city in the database
    /// </summary>
    /// <param name="id">ID of the city to update</param>
    /// <param name="city">Updated city data</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The updated city if found, null otherwise</returns>
    public async Task<City?> UpdateCityAsync(int id, City city, CancellationToken cancellationToken = default)
    {
        // Check if the city exists
        var existingCity = await _cityRepository.GetByIdAsync(id, cancellationToken);

        if (existingCity == null)
        {
            return null;
        }

        // Additional business logic can be added here
        // For example, validation, checking changes, etc.

        // Ensure ID consistency
        city.CityId = id;
        return await _cityRepository.UpdateAsync(city, cancellationToken);
    }

    /// <summary>
    /// Deletes a city from the database
    /// </summary>
    /// <param name="id">ID of the city to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>True if the city was deleted, false if not found</returns>
    public async Task<bool> DeleteCityAsync(int id, CancellationToken cancellationToken = default)
    {
        // Additional business logic can be added here
        // For example, checking related data, etc.

        return await _cityRepository.DeleteByIdAsync(id, cancellationToken);
    }
}