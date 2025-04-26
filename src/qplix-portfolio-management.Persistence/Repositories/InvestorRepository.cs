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


    /// <summary>
    /// Adds a new investor to the database
    /// </summary>
    /// <param name="entity">Investor entity to add</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The added investor with assigned ID</returns>
    public async Task<Investor> AddAsync(Investor entity, CancellationToken cancellationToken = default)
    {
        // Set creation timestamp
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;

        await _context.Investors.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Retrieves an investor by its ID
    /// </summary>
    /// <param name="id">ID of the investor to retrieve</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The investor if found, null otherwise</returns>
    public async Task<Investor?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is int investorId)
        {
            return await _context.Investors
                .FirstOrDefaultAsync(i => i.InvestorId == investorId, cancellationToken);
        }

        throw new ArgumentException($"Invalid id type: {id.GetType().Name}. Expected: Int32", nameof(id));
    }

    /// <summary>
    /// Retrieves all investors from the database
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>Collection of all investors</returns>
    public async Task<IEnumerable<Investor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Investors.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing investor in the database
    /// </summary>
    /// <param name="entity">Updated investor data</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The updated investor</returns>
    public async Task<Investor> UpdateAsync(Investor entity, CancellationToken cancellationToken = default)
    {
        // Update timestamp
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Investors.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// Deletes an investor from the database
    /// </summary>
    /// <param name="entity">Investor entity to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>True if the investor was deleted, false otherwise</returns>
    public async Task<bool> DeleteAsync(Investor entity, CancellationToken cancellationToken = default)
    {
        _context.Investors.Remove(entity);
        int affectedRows = await _context.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }

    /// <summary>
    /// Deletes an investor by its ID
    /// </summary>
    /// <param name="id">ID of the investor to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>True if the investor was deleted, false if not found</returns>
    public async Task<bool> DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is int investorId)
        {
            var investor = await GetByIdAsync(investorId, cancellationToken);
            if (investor == null)
            {
                return false;
            }

            return await DeleteAsync(investor, cancellationToken);
        }

        throw new ArgumentException($"Invalid id type: {id.GetType().Name}. Expected: Int32", nameof(id));
    }
}