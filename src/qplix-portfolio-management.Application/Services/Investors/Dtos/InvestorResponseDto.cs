namespace qplix_portfolio_management.Application.Services.Investors.Dtos;

/// <summary>
/// Data Transfer Object for retrieving Investor information
/// </summary>
public class InvestorResponseDto
{
    /// <summary>
    /// Unique identifier for the Investor
    /// </summary>
    public int InvestorId { get; set; }

    /// <summary>
    /// Unique code for the Investor
    /// </summary>
    public string InvestorCode { get; set; } = null!;

    /// <summary>
    /// Timestamp when the Investor was created
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Timestamp of the last update to the Investor
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}