namespace qplix_portfolio_management.Domain.Entities;

public partial class InvestorInvestment
{
    public int InvestorId { get; set; }

    public int InvestmentId { get; set; }

    public DateOnly? InitialInvestmentDate { get; set; }

    public decimal? InitialAmount { get; set; }

    public virtual Investment Investment { get; set; } = null!;

    public virtual Investor Investor { get; set; } = null!;
}
