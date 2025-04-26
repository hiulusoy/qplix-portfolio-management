using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Application.Services.Investors.Dtos;
using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Services.Investors;

/// <summary>
/// Service implementation for managing investor-related operations
/// </summary>
public class InvestorService : IInvestorService
{
    private readonly IInvestorRepository _investorRepository;

    /// <summary>
    /// Constructor for InvestorService
    /// </summary>
    /// <param name="investorRepository">Repository for investor data access</param>
    /// <exception cref="ArgumentNullException">Thrown when repository is null</exception>
    public InvestorService(IInvestorRepository investorRepository)
    {
        _investorRepository = investorRepository ?? throw new ArgumentNullException(nameof(investorRepository));
    }

    /// <summary>
    /// Retrieves all investors from the database
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>Collection of investor DTOs</returns>
    public async Task<IEnumerable<InvestorResponseDto>> GetAllInvestorsAsync(CancellationToken cancellationToken = default)
    {
        // Get all investors from the repository
        var investors = await _investorRepository.GetAllAsync(cancellationToken);

        // Map entity objects to DTOs
        return investors.Select(investor => new InvestorResponseDto
        {
            InvestorId = investor.InvestorId,
            InvestorCode = investor.InvestorCode,
            CreatedAt = investor.CreatedAt,
            UpdatedAt = investor.UpdatedAt
        }).ToList();
    }

    /// <summary>
    /// Retrieves a specific investor by its ID
    /// </summary>
    /// <param name="id">ID of the investor to retrieve</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The investor if found, null otherwise</returns>
    public async Task<Investor?> GetInvestorByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _investorRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Creates a new investor in the database
    /// </summary>
    /// <param name="investor">Investor entity to create</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The created investor with assigned ID</returns>
    /// <exception cref="ArgumentException">Thrown when investor data is invalid</exception>
    public async Task<Investor> CreateInvestorAsync(Investor investor, CancellationToken cancellationToken = default)
    {
        // Validate investor data before saving
        if (string.IsNullOrWhiteSpace(investor.InvestorCode))
        {
            throw new ArgumentException("Investor code cannot be empty", nameof(investor));
        }

        // Add the investor to the database
        return await _investorRepository.AddAsync(investor, cancellationToken);
    }

    /// <summary>
    /// Updates an existing investor in the database
    /// </summary>
    /// <param name="id">ID of the investor to update</param>
    /// <param name="investor">Updated investor data</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The updated investor if found, null otherwise</returns>
    public async Task<Investor?> UpdateInvestorAsync(int id, Investor investor, CancellationToken cancellationToken = default)
    {
        // Check if the investor exists
        var existingInvestor = await _investorRepository.GetByIdAsync(id, cancellationToken);

        if (existingInvestor == null)
        {
            return null;
        }

        // Validate investor data
        if (string.IsNullOrWhiteSpace(investor.InvestorCode))
        {
            throw new ArgumentException("Investor code cannot be empty", nameof(investor));
        }

        // Ensure ID consistency and preserve creation timestamp
        investor.InvestorId = id;
        investor.CreatedAt = existingInvestor.CreatedAt;
        
        return await _investorRepository.UpdateAsync(investor, cancellationToken);
    }

    /// <summary>
    /// Deletes an investor from the database
    /// </summary>
    /// <param name="id">ID of the investor to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>True if the investor was deleted, false if not found</returns>
    public async Task<bool> DeleteInvestorAsync(int id, CancellationToken cancellationToken = default)
    {
        // Check if the investor has related investments
        var investor = await _investorRepository.GetByIdAsync(id, cancellationToken);
        if (investor == null)
        {
            return false;
        }

        // Check if the investor has related investments
        if (investor.InvestorInvestments.Any())
        {
            throw new InvalidOperationException($"Cannot delete investor with ID {id} because it has related investments");
        }

        return await _investorRepository.DeleteByIdAsync(id, cancellationToken);
    }
}