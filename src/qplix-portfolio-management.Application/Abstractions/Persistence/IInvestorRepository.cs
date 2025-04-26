using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Abstractions.Persistence;

public interface IInvestorRepository: IBaseRepository<Investor>
{
    Task<Investor?> GetInvestorByIdAsync(int investorId);
    Task<IEnumerable<InvestorInvestment>> GetInvestorInvestmentsAsync(int investorId);
}