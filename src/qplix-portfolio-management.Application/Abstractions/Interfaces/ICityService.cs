using qplix_portfolio_management.Application.Services.Cities.Dtos;
using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Abstractions.Interfaces;

public interface ICityService
{
    Task<IEnumerable<GetCityResponseDto>> GetAllCitiesAsync(CancellationToken cancellationToken = default);
    Task<City?> GetCityByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<City> CreateCityAsync(City city, CancellationToken cancellationToken = default);
    Task<City?> UpdateCityAsync(int id, City city, CancellationToken cancellationToken = default);
    Task<bool> DeleteCityAsync(int id, CancellationToken cancellationToken = default);
}