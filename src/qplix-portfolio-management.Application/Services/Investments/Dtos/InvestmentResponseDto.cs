namespace qplix_portfolio_management.Application.Services.Investments.Dtos;

public class InvestmentResponseDto
{
    public int InvestmentId { get; set; }
    public string InvestmentCode { get; set; } = string.Empty;
    public string InvestmentType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal PercentageOfPortfolio { get; set; }
}