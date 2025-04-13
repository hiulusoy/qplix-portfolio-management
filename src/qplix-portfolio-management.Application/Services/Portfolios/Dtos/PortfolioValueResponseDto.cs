using qplix_portfolio_management.Application.Services.Investments.Dtos;

namespace qplix_portfolio_management.Application.Services.Portfolios.Dtos;

public class PortfolioValueResponseDto
{
    public int InvestorId { get; set; }
    public string InvestorCode { get; set; } = string.Empty;
    public DateTime ReferenceDate { get; set; }
    public DateTime CalculationDate { get; set; }
    public decimal TotalValue { get; set; }
    public List<InvestmentResponseDto> InvestmentValues { get; set; } = new();
}