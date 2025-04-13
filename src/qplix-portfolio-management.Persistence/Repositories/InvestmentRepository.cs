using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence.Contexts;

namespace qplix_portfolio_management.Persistence.Repositories;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly ApplicationDbContext _context;

    public InvestmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Investment> GetInvestmentWithDetailsAsync(int investmentId)
    {
        return await _context.Investments
            .Include(i => i.InvestmentType) // Make sure this is included
            .FirstOrDefaultAsync(i => i.InvestmentId == investmentId);
    }

    public async Task<InvestmentType?> GetInvestmentTypeAsync(int investmentTypeId)
    {
        return await _context.InvestmentTypes
            .FirstOrDefaultAsync(it => it.InvestmentTypeId == investmentTypeId);
    }

    public async Task<IEnumerable<Investment>> GetFundInvestmentsAsync(int fundId)
    {
        return await _context.Investments
            .Where(i => i.FundId == fundId)
            .ToListAsync();
    }
}