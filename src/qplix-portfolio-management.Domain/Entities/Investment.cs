namespace qplix_portfolio_management.Domain.Entities;

public partial class Investment
{
    public int InvestmentId { get; set; }

    public string InvestmentCode { get; set; } = null!;

    public int? InvestmentTypeId { get; set; }

    public string? Isin { get; set; }

    public int? CityId { get; set; }

    public int? FundId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual City? City { get; set; }

    public virtual Fund? Fund { get; set; }

    public virtual InvestmentType? InvestmentType { get; set; }

    public virtual ICollection<InvestorInvestment> InvestorInvestments { get; set; } = new List<InvestorInvestment>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
