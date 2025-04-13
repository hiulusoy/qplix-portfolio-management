namespace qplix_portfolio_management.Domain.Entities;

public partial class City
{
    public int CityId { get; set; }

    public string CityCode { get; set; } = null!;

    public string? CityName { get; set; }

    public virtual ICollection<Investment> Investments { get; set; } = new List<Investment>();
}
