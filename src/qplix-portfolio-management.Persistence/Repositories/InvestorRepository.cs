using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence.Contexts;

namespace qplix_portfolio_management.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;


public class InvestorRepository : IInvestorRepository
{
    private readonly ApplicationDbContext _context;
    
    public InvestorRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Investor?> GetInvestorByIdAsync(int investorId)
    {
        return await _context.Investors
            .FirstOrDefaultAsync(i => i.InvestorId == investorId);
    }
    
    public async Task<IEnumerable<InvestorInvestment>> GetInvestorInvestmentsAsync(int investorId)
    {
        return await _context.InvestorInvestments
            .Where(ii => ii.InvestorId == investorId)
            .ToListAsync();
    }
}