
using Transaction = qplix_portfolio_management.Domain.Entities.Transaction;

namespace qplix_portfolio_management.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetTransactionsForInvestmentAsync(
        int investmentId,
        int transactionTypeId,
        DateOnly endDate);
}