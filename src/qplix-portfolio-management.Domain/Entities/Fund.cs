namespace qplix_portfolio_management.Domain.Entities;

public partial class Fund
{
    public int FundId { get; set; }

    public string FundCode { get; set; } = null!;

    public string? FundName { get; set; }

    public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();
}
