using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence.Contexts;

namespace qplix_portfolio_management.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsForInvestmentAsync(
        int investmentId,
        int transactionTypeId,
        DateOnly endDate)
    {
        return await _context.Transactions
            .Where(t => t.InvestmentId == investmentId &&
                        t.TransactionTypeId == transactionTypeId &&
                        t.TransactionDate <= endDate)
            .ToListAsync();
    }
}