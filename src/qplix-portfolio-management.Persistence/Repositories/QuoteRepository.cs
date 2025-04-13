using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence.Contexts;

namespace qplix_portfolio_management.Persistence.Repositories;

public class QuoteRepository : IQuoteRepository
{
    private readonly ApplicationDbContext _context;

    public QuoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Quote?> GetLatestQuoteBeforeDateAsync(string isin, DateOnly referenceDate)
    {
        return await _context.Quotes
            .Where(q => q.Isin == isin && q.QuoteDate <= referenceDate)
            .OrderByDescending(q => q.QuoteDate)
            .FirstOrDefaultAsync();
    }
}