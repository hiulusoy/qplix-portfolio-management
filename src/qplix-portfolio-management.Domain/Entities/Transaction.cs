namespace qplix_portfolio_management.Domain.Entities;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int InvestmentId { get; set; }

    public int TransactionTypeId { get; set; }

    public DateOnly TransactionDate { get; set; }

    public decimal Value { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Investment Investment { get; set; } = null!;

    public virtual TransactionType TransactionType { get; set; } = null!;
}
