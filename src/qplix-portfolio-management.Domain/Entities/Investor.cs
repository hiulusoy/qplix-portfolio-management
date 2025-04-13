namespace qplix_portfolio_management.Domain.Entities;

public partial class Investor
{
    public int InvestorId { get; set; }

    public string InvestorCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<InvestorInvestment> InvestorInvestments { get; set; } = new List<InvestorInvestment>();
}
