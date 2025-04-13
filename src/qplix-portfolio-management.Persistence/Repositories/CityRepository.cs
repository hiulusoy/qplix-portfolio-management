using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence.Contexts;

namespace qplix_portfolio_management.Persistence.Repositories;

public class CityRepository : ICityRepository
{
    private readonly ApplicationDbContext _context;

    public CityRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<City> AddAsync(City entity, CancellationToken cancellationToken = default)
    {
        await _context.Cities.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<City?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is int cityId)
        {
            return await _context.Cities
                .FirstOrDefaultAsync(c => c.CityId == cityId, cancellationToken);
        }

        throw new ArgumentException($"Invalid id type: {id.GetType().Name}. Expected: Int32", nameof(id));
    }

    public async Task<IEnumerable<City>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cities.ToListAsync(cancellationToken);
    }

    public async Task<City> UpdateAsync(City entity, CancellationToken cancellationToken = default)
    {
        _context.Cities.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(City entity, CancellationToken cancellationToken = default)
    {
        _context.Cities.Remove(entity);
        int affectedRows = await _context.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is int cityId)
        {
            var city = await GetByIdAsync(cityId, cancellationToken);
            if (city == null)
            {
                return false;
            }

            return await DeleteAsync(city, cancellationToken);
        }

        throw new ArgumentException($"Invalid id type: {id.GetType().Name}. Expected: Int32", nameof(id));
    }
}