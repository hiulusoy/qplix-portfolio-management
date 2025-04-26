using qplix_portfolio_management.Application.Services.Investors.Dtos;
using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Abstractions.Interfaces;

/// <summary>
/// Interface for Investor service operations
/// </summary>
public interface IInvestorService
{
    /// <summary>
    /// Retrieves all investors from the database
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>Collection of investor DTOs</returns>
    Task<IEnumerable<InvestorResponseDto>> GetAllInvestorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific investor by its ID
    /// </summary>
    /// <param name="id">ID of the investor to retrieve</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The investor if found, null otherwise</returns>
    Task<Investor?> GetInvestorByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new investor in the database
    /// </summary>
    /// <param name="investor">Investor entity to create</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The created investor with assigned ID</returns>
    Task<Investor> CreateInvestorAsync(Investor investor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing investor in the database
    /// </summary>
    /// <param name="id">ID of the investor to update</param>
    /// <param name="investor">Updated investor data</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>The updated investor if found, null otherwise</returns>
    Task<Investor?> UpdateInvestorAsync(int id, Investor investor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an investor from the database
    /// </summary>
    /// <param name="id">ID of the investor to delete</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>True if the investor was deleted, false if not found</returns>
    Task<bool> DeleteInvestorAsync(int id, CancellationToken cancellationToken = default);
}