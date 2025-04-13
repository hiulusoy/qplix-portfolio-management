namespace qplix_portfolio_management.Domain.Entities;

public partial class Quote
{
    public int QuoteId { get; set; }

    public string Isin { get; set; } = null!;

    public DateOnly QuoteDate { get; set; }

    public decimal PricePerShare { get; set; }

    public DateTime? CreatedAt { get; set; }
}
