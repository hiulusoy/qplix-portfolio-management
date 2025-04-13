using qplix_portfolio_management.Application.Services.Portfolios.Dtos;

namespace qplix_portfolio_management.Application.Abstractions.Interfaces;

public interface IPortfolioCalculationService
{
    Task<PortfolioValueResponseDto> CalculatePortfolioValueAsync(int investorId, DateTime referenceDate);
}