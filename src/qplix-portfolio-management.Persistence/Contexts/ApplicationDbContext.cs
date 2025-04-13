using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using qplix_portfolio_management.Domain.Entities;
using qplix_portfolio_management.Persistence;

namespace qplix_portfolio_management.Persistence.Contexts;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Fund> Funds { get; set; }

    public virtual DbSet<Investment> Investments { get; set; }

    public virtual DbSet<InvestmentType> InvestmentTypes { get; set; }

    public virtual DbSet<Investor> Investors { get; set; }

    public virtual DbSet<InvestorInvestment> InvestorInvestments { get; set; }

    public virtual DbSet<PortfolioPerformance> PortfolioPerformances { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=qplix_db;User Id=qplix_user;Password=qplix;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("cities_pkey");

            entity.ToTable("cities");

            entity.HasIndex(e => e.CityCode, "cities_city_code_key").IsUnique();

            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CityCode)
                .HasMaxLength(50)
                .HasColumnName("city_code");
            entity.Property(e => e.CityName)
                .HasMaxLength(100)
                .HasColumnName("city_name");
        });

        modelBuilder.Entity<Fund>(entity =>
        {
            entity.HasKey(e => e.FundId).HasName("funds_pkey");

            entity.ToTable("funds");

            entity.HasIndex(e => e.FundCode, "funds_fund_code_key").IsUnique();

            entity.Property(e => e.FundId).HasColumnName("fund_id");
            entity.Property(e => e.FundCode)
                .HasMaxLength(50)
                .HasColumnName("fund_code");
            entity.Property(e => e.FundName)
                .HasMaxLength(100)
                .HasColumnName("fund_name");
        });

        modelBuilder.Entity<Investment>(entity =>
        {
            entity.HasKey(e => e.InvestmentId).HasName("investments_pkey");

            entity.ToTable("investments");

            entity.HasIndex(e => e.Isin, "idx_investments_isin");

            entity.Property(e => e.InvestmentId)
                .ValueGeneratedNever()
                .HasColumnName("investment_id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FundId).HasColumnName("fund_id");
            entity.Property(e => e.InvestmentCode)
                .HasMaxLength(50)
                .HasColumnName("investment_code");
            entity.Property(e => e.InvestmentTypeId).HasColumnName("investment_type_id");
            entity.Property(e => e.Isin)
                .HasMaxLength(20)
                .HasColumnName("isin");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.City).WithMany(p => p.Investments)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("fk_investments_city");

            entity.HasOne(d => d.Fund).WithMany(p => p.Investments)
                .HasForeignKey(d => d.FundId)
                .HasConstraintName("fk_investments_fund");

            entity.HasOne(d => d.InvestmentType).WithMany(p => p.Investments)
                .HasForeignKey(d => d.InvestmentTypeId)
                .HasConstraintName("fk_investments_investment_type");
        });

        modelBuilder.Entity<InvestmentType>(entity =>
        {
            entity.HasKey(e => e.InvestmentTypeId).HasName("investment_types_pkey");

            entity.ToTable("investment_types");

            entity.HasIndex(e => e.TypeName, "investment_types_type_name_key").IsUnique();

            entity.Property(e => e.InvestmentTypeId).HasColumnName("investment_type_id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<Investor>(entity =>
        {
            entity.HasKey(e => e.InvestorId).HasName("investors_pkey");

            entity.ToTable("investors");

            entity.Property(e => e.InvestorId)
                .ValueGeneratedNever()
                .HasColumnName("investor_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.InvestorCode)
                .HasMaxLength(50)
                .HasColumnName("investor_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<InvestorInvestment>(entity =>
        {
            entity.HasKey(e => new { e.InvestorId, e.InvestmentId }).HasName("investor_investments_pkey");

            entity.ToTable("investor_investments");

            entity.HasIndex(e => e.InvestmentId, "idx_investor_investments_investment");

            entity.HasIndex(e => e.InvestorId, "idx_investor_investments_investor");

            entity.Property(e => e.InvestorId).HasColumnName("investor_id");
            entity.Property(e => e.InvestmentId).HasColumnName("investment_id");
            entity.Property(e => e.InitialAmount)
                .HasPrecision(20, 2)
                .HasColumnName("initial_amount");
            entity.Property(e => e.InitialInvestmentDate).HasColumnName("initial_investment_date");

            entity.HasOne(d => d.Investment).WithMany(p => p.InvestorInvestments)
                .HasForeignKey(d => d.InvestmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_investor_investments_investment");

            entity.HasOne(d => d.Investor).WithMany(p => p.InvestorInvestments)
                .HasForeignKey(d => d.InvestorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_investor_investments_investor");
        });

        modelBuilder.Entity<PortfolioPerformance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("portfolio_performance");

            entity.Property(e => e.FundCode)
                .HasMaxLength(50)
                .HasColumnName("fund_code");
            entity.Property(e => e.InvestmentCode)
                .HasMaxLength(50)
                .HasColumnName("investment_code");
            entity.Property(e => e.InvestmentId).HasColumnName("investment_id");
            entity.Property(e => e.InvestmentType)
                .HasMaxLength(50)
                .HasColumnName("investment_type");
            entity.Property(e => e.InvestorCode)
                .HasMaxLength(50)
                .HasColumnName("investor_code");
            entity.Property(e => e.InvestorId).HasColumnName("investor_id");
            entity.Property(e => e.Isin)
                .HasMaxLength(20)
                .HasColumnName("isin");
            entity.Property(e => e.PortfolioValue).HasColumnName("portfolio_value");
            entity.Property(e => e.PricePerShare)
                .HasPrecision(20, 6)
                .HasColumnName("price_per_share");
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");
            entity.Property(e => e.TransactionValue)
                .HasPrecision(20, 6)
                .HasColumnName("transaction_value");
            entity.Property(e => e.ValuationDate).HasColumnName("valuation_date");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.QuoteId).HasName("quotes_pkey");

            entity.ToTable("quotes");

            entity.HasIndex(e => e.QuoteDate, "idx_quotes_date");

            entity.HasIndex(e => new { e.Isin, e.QuoteDate }, "quotes_unique").IsUnique();

            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Isin)
                .HasMaxLength(20)
                .HasColumnName("isin");
            entity.Property(e => e.PricePerShare)
                .HasPrecision(20, 6)
                .HasColumnName("price_per_share");
            entity.Property(e => e.QuoteDate).HasColumnName("quote_date");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.HasIndex(e => new { e.InvestmentId, e.TransactionDate }, "idx_transactions_investment_date");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.InvestmentId).HasColumnName("investment_id");
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
            entity.Property(e => e.TransactionTypeId).HasColumnName("transaction_type_id");
            entity.Property(e => e.Value)
                .HasPrecision(20, 6)
                .HasColumnName("value");

            entity.HasOne(d => d.Investment).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.InvestmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transactions_investment");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transactions_transaction_type");
        });

        modelBuilder.Entity<TransactionType>(entity =>
        {
            entity.HasKey(e => e.TransactionTypeId).HasName("transaction_types_pkey");

            entity.ToTable("transaction_types");

            entity.HasIndex(e => e.TypeName, "transaction_types_type_name_key").IsUnique();

            entity.Property(e => e.TransactionTypeId).HasColumnName("transaction_type_id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("type_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
