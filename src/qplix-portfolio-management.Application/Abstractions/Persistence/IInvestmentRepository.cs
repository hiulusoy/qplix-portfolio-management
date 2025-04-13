using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Abstractions.Persistence;

public interface IInvestmentRepository
{
    Task<Investment?> GetInvestmentWithDetailsAsync(int investmentId);
    Task<InvestmentType?> GetInvestmentTypeAsync(int investmentTypeId);
    Task<IEnumerable<Investment>> GetFundInvestmentsAsync(int fundId);
}