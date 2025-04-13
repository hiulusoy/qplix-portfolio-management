using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Abstractions.Persistence;

public interface IQuoteRepository
{
    Task<Quote?> GetLatestQuoteBeforeDateAsync(string isin, DateOnly referenceDate);
}