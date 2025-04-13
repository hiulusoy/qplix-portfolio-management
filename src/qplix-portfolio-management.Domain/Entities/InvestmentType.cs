namespace qplix_portfolio_management.Domain.Entities;

public partial class InvestmentType
{
    public int InvestmentTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();
}
