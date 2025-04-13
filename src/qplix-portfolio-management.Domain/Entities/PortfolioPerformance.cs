namespace qplix_portfolio_management.Domain.Entities;

public partial class PortfolioPerformance
{
    public int? InvestorId { get; set; }

    public string? InvestorCode { get; set; }

    public int? InvestmentId { get; set; }

    public string? InvestmentCode { get; set; }

    public string? Isin { get; set; }

    public string? InvestmentType { get; set; }

    public string? FundCode { get; set; }

    public DateOnly? ValuationDate { get; set; }

    public decimal? PricePerShare { get; set; }

    public decimal? TransactionValue { get; set; }

    public string? TransactionType { get; set; }

    public DateOnly? TransactionDate { get; set; }

    public decimal? PortfolioValue { get; set; }
}
