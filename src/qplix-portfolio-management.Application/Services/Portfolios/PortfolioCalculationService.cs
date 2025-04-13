using Microsoft.Extensions.Logging;
using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Abstractions.Persistence;
using qplix_portfolio_management.Application.Services.Investments.Dtos;
using qplix_portfolio_management.Application.Services.Portfolios.Dtos;
using qplix_portfolio_management.Domain.Entities;

namespace qplix_portfolio_management.Application.Services.Portfolios
{
    public class PortfolioCalculationService : IPortfolioCalculationService
    {
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestmentRepository _investmentRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IQuoteRepository _quoteRepository;
        private readonly ILogger<PortfolioCalculationService> _logger;

        // Sabit değerler - veritabanı yapınıza göre
        private const int FONDS_TYPE_ID = 1;
        private const int STOCK_TYPE_ID = 2;
        private const int REALESTATE_TYPE_ID = 3;

        private const int PERCENTAGE_TRANSACTION_ID = 1;
        private const int SHARES_TRANSACTION_ID = 2;
        private const int ESTATE_TRANSACTION_ID = 3;
        private const int BUILDING_TRANSACTION_ID = 4;

        public PortfolioCalculationService(
            IInvestorRepository investorRepository,
            IInvestmentRepository investmentRepository,
            ITransactionRepository transactionRepository,
            IQuoteRepository quoteRepository,
            ILogger<PortfolioCalculationService> logger)
        {
            _investorRepository = investorRepository;
            _investmentRepository = investmentRepository;
            _transactionRepository = transactionRepository;
            _quoteRepository = quoteRepository;
            _logger = logger;
        }

        /// <summary>
        /// Calculates the portfolio value for a specific investor at the given reference date
        /// </summary>
        /// <param name="investorId">The ID of the investor whose portfolio will be calculated</param>
        /// <param name="referenceDate">The reference date for the portfolio valuation</param>
        /// <returns>The investor's portfolio value and details of each investment</returns>
        public async Task<PortfolioValueResponseDto> CalculatePortfolioValueAsync(int investorId,
            DateTime referenceDate)
        {
            _logger.LogInformation(
                $"Calculating portfolio value for investor {investorId} as of {referenceDate:yyyy-MM-dd}");

            var investor = await GetInvestorOrThrowAsync(investorId);
            var investorInvestments = await _investorRepository.GetInvestorInvestmentsAsync(investorId);

            if (!investorInvestments.Any())
            {
                _logger.LogInformation($"No investments found for investor {investorId}");
                return CreateEmptyPortfolioResponse(investor, referenceDate);
            }

            var (totalValue, investmentValues) =
                await CalculateAllInvestmentValuesAsync(investorInvestments, investorId, referenceDate);
            CalculatePortfolioPercentages(investmentValues, totalValue);

            var result = CreatePortfolioResponse(investor, referenceDate, totalValue, investmentValues);

            _logger.LogInformation(
                $"Portfolio calculation complete for investor {investorId}: Total value {totalValue}");

            return result;
        }

        /// <summary>
        /// Retrieves an investor with the specified ID or throws an exception if not found
        /// </summary>
        /// <param name="investorId">The ID of the investor to find</param>
        /// <returns>The found investor object</returns>
        /// <exception cref="Exception">Thrown when the investor is not found</exception>
        private async Task<Investor> GetInvestorOrThrowAsync(int investorId)
        {
            var investor = await _investorRepository.GetInvestorByIdAsync(investorId);
            if (investor == null)
            {
                _logger.LogWarning($"Investor with ID {investorId} not found");
                throw new Exception($"Investor with ID {investorId} not found");
            }

            return investor;
        }

        /// <summary>
        /// Creates an empty portfolio response for an investor without investments
        /// </summary>
        /// <param name="investor">The investor information</param>
        /// <param name="referenceDate">The portfolio valuation date</param>
        /// <returns>A portfolio response with zero value and an empty investment list</returns>
        private PortfolioValueResponseDto CreateEmptyPortfolioResponse(Investor investor, DateTime referenceDate)
        {
            return new PortfolioValueResponseDto
            {
                InvestorId = investor.InvestorId,
                InvestorCode = investor.InvestorCode,
                ReferenceDate = referenceDate,
                CalculationDate = DateTime.UtcNow,
                TotalValue = 0,
                InvestmentValues = new List<InvestmentResponseDto>()
            };
        }

        /// <summary>
        /// Calculates the values of all investments for an investor and returns the total portfolio value
        /// </summary>
        /// <param name="investorInvestments">Collection of investor's investments</param>
        /// <param name="investorId">The ID of the investor</param>
        /// <param name="referenceDate">The reference date for value calculation</param>
        /// <returns>Tuple containing the total portfolio value and a list of investment response DTOs</returns>
        private async Task<(decimal TotalValue, List<InvestmentResponseDto> InvestmentValues)>
            CalculateAllInvestmentValuesAsync(
                IEnumerable<InvestorInvestment> investorInvestments, int investorId, DateTime referenceDate)
        {
            decimal totalPortfolioValue = 0;
            var investmentValues = new List<InvestmentResponseDto>();

            foreach (var investorInvestment in investorInvestments)
            {
                // Retrieve the investment details
                var investment =
                    await _investmentRepository.GetInvestmentWithDetailsAsync(investorInvestment.InvestmentId);
                if (investment == null)
                {
                    _logger.LogWarning($"Investment with ID {investorInvestment.InvestmentId} not found");
                    continue;
                }

                try
                {
                    // Calculate the value of this investment
                    var investmentValue = await CalculateInvestmentValueAsync(investment, investorId, referenceDate);
                    if (investmentValue > 0)
                    {
                        var investmentType = investment.InvestmentType;
                        investmentValues.Add(CreateInvestmentResponse(investment, investmentType, investmentValue));
                        totalPortfolioValue += investmentValue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error calculating value for investment {investment.InvestmentId}");
                }
            }

            return (totalPortfolioValue, investmentValues);
        }

        /// <summary>
        /// Creates an investment response DTO from investment data
        /// </summary>
        /// <param name="investment">The investment entity</param>
        /// <param name="investmentType">The type of the investment</param>
        /// <param name="investmentValue">The calculated value of the investment</param>
        /// <returns>Investment response DTO with investment details</returns>
        private InvestmentResponseDto CreateInvestmentResponse(
            Investment investment, InvestmentType? investmentType, decimal investmentValue)
        {
            string typeName = "Unknown";

            if (investmentType != null)
            {
                typeName = investmentType.TypeName;
            }
            else if (investment.InvestmentTypeId.HasValue)
            {
                // Fallback to determine type name based on known type IDs
                typeName = investment.InvestmentTypeId.Value switch
                {
                    FONDS_TYPE_ID => "Fund",
                    STOCK_TYPE_ID => "Stock",
                    REALESTATE_TYPE_ID => "Real Estate",
                    _ => "Unknown"
                };
            }

            return new InvestmentResponseDto
            {
                InvestmentId = investment.InvestmentId,
                InvestmentCode = investment.InvestmentCode,
                InvestmentType = typeName,
                Value = investmentValue,
                PercentageOfPortfolio = 0 // Will be filled after total is calculated
            };
        }

        /// <summary>
        /// Calculates the percentage of each investment in the portfolio
        /// </summary>
        /// <param name="investmentValues">List of investment response DTOs</param>
        /// <param name="totalPortfolioValue">The total value of the portfolio</param>
        /// <remarks>The percentages are calculated only if the total portfolio value is greater than zero</remarks>
        private void CalculatePortfolioPercentages(List<InvestmentResponseDto> investmentValues,
            decimal totalPortfolioValue)
        {
            if (totalPortfolioValue > 0)
            {
                foreach (var investment in investmentValues)
                {
                    // Calculate and round the percentage to 2 decimal places
                    investment.PercentageOfPortfolio = Math.Round((investment.Value / totalPortfolioValue) * 100, 2);
                }
            }
        }

        /// <summary>
        /// Creates a portfolio value response DTO with calculated values
        /// </summary>
        /// <param name="investor">The investor entity</param>
        /// <param name="referenceDate">The reference date for the valuation</param>
        /// <param name="totalValue">The total portfolio value</param>
        /// <param name="investmentValues">List of investment response DTOs</param>
        /// <returns>Complete portfolio value response with all details</returns>
        private PortfolioValueResponseDto CreatePortfolioResponse(
            Investor investor, DateTime referenceDate, decimal totalValue, List<InvestmentResponseDto> investmentValues)
        {
            return new PortfolioValueResponseDto
            {
                InvestorId = investor.InvestorId,
                InvestorCode = investor.InvestorCode,
                ReferenceDate = referenceDate,
                CalculationDate = DateTime.UtcNow,
                TotalValue = totalValue,
                InvestmentValues = investmentValues
            };
        }

        /// <summary>
        /// Calculates the value of a single investment considering its type
        /// </summary>
        /// <param name="investment">The investment entity to calculate value for</param>
        /// <param name="investorId">The ID of the investor</param>
        /// <param name="referenceDate">The reference date for the valuation</param>
        /// <param name="processedFunds">HashSet to track processed funds and prevent circular references</param>
        /// <returns>The calculated value of the investment</returns>
        private async Task<decimal> CalculateInvestmentValueAsync(
            Investment investment, int investorId, DateTime referenceDate, HashSet<int>? processedFunds = null)
        {
            // Prevent circular fund references
            processedFunds ??= new HashSet<int>();

            if (IsCycleDetected(investment, processedFunds))
            {
                _logger.LogWarning($"Circular fund reference detected for fund {investment.InvestmentId}");
                return 0;
            }

            if (investment.FundId.HasValue)
                processedFunds.Add(investment.InvestmentId);

            // Calculate value based on investment type
            return await CalculateValueByInvestmentTypeAsync(investment, investorId, referenceDate, processedFunds);
        }

        /// <summary>
        /// Detects if there is a circular reference in fund investments
        /// </summary>
        /// <param name="investment">The investment to check</param>
        /// <param name="processedFunds">Set of already processed fund IDs</param>
        /// <returns>True if a cycle is detected, false otherwise</returns>
        private bool IsCycleDetected(Investment investment, HashSet<int> processedFunds)
        {
            return investment.FundId.HasValue && processedFunds.Contains(investment.InvestmentId);
        }

        /// <summary>
        /// Calculates investment value based on its type (stock, real estate, or fund)
        /// </summary>
        /// <param name="investment">The investment entity</param>
        /// <param name="investorId">The ID of the investor</param>
        /// <param name="referenceDate">The reference date for the valuation</param>
        /// <param name="processedFunds">Set of already processed fund IDs</param>
        /// <returns>The calculated value based on the investment type</returns>
        private async Task<decimal> CalculateValueByInvestmentTypeAsync(
            Investment investment, int investorId, DateTime referenceDate, HashSet<int>? processedFunds)
        {
            if (!investment.InvestmentTypeId.HasValue)
            {
                _logger.LogWarning($"Investment {investment.InvestmentId} has no investment type");
                return 0;
            }

            switch (investment.InvestmentTypeId.Value)
            {
                case STOCK_TYPE_ID: // Stock
                    return await CalculateStockValueAsync(investment, referenceDate);

                case REALESTATE_TYPE_ID: // Real Estate
                    return await CalculateRealEstateValueAsync(investment, referenceDate);

                case FONDS_TYPE_ID: // Fund
                    return await CalculateFundValueAsync(investment, investorId, referenceDate, processedFunds);

                default:
                    _logger.LogWarning($"Unknown investment type ID: {investment.InvestmentTypeId}");
                    return 0;
            }
        }

        /// <summary>
        /// Calculates the value of a stock investment
        /// </summary>
        /// <param name="investment">The stock investment entity</param>
        /// <param name="referenceDate">The reference date for the valuation</param>
        /// <returns>The calculated stock value (share count × share price)</returns>
        private async Task<decimal> CalculateStockValueAsync(Investment investment, DateTime referenceDate)
        {
            if (string.IsNullOrEmpty(investment.Isin))
            {
                _logger.LogWarning($"Stock investment {investment.InvestmentId} has no ISIN");
                return 0;
            }

            // Calculate share count
            var shareCount = await CalculateShareCountAsync(investment, referenceDate);
            if (shareCount <= 0)
            {
                _logger.LogInformation($"No shares held for stock {investment.Isin} as of {referenceDate:yyyy-MM-dd}");
                return 0;
            }

            // Get latest price
            var stockPrice = await GetLatestStockPriceAsync(investment.Isin, referenceDate);
            if (stockPrice <= 0)
                return 0;

            // Calculate stock value: share count × share price
            decimal stockValue = shareCount * stockPrice;

            _logger.LogDebug(
                $"Stock {investment.Isin} value: {shareCount} shares × {stockPrice} = {stockValue}");

            return stockValue;
        }

        /// <summary>
        /// Calculates the total number of shares held for a stock investment as of the reference date
        /// </summary>
        /// <param name="investment">The stock investment entity</param>
        /// <param name="referenceDate">The reference date for share count calculation</param>
        /// <returns>The total number of shares held, which can be positive or negative</returns>
        private async Task<decimal> CalculateShareCountAsync(Investment investment, DateTime referenceDate)
        {
            var transactionDate = DateOnly.FromDateTime(referenceDate);
            var transactions = await _transactionRepository.GetTransactionsForInvestmentAsync(
                investment.InvestmentId,
                SHARES_TRANSACTION_ID,
                transactionDate);

            decimal shareCount = 0;
            foreach (var transaction in transactions)
            {
                shareCount += transaction.Value;
            }

            return shareCount;
        }

        /// <summary>
        /// Retrieves the latest stock price for the specified ISIN before the reference date
        /// </summary>
        /// <param name="isin">The ISIN code of the stock</param>
        /// <param name="referenceDate">The reference date to find quotes before</param>
        /// <returns>The price per share or 0 if no quote is found</returns>
        private async Task<decimal> GetLatestStockPriceAsync(string isin, DateTime referenceDate)
        {
            var quoteDate = DateOnly.FromDateTime(referenceDate);
            var latestQuote = await _quoteRepository.GetLatestQuoteBeforeDateAsync(isin, quoteDate);

            if (latestQuote == null)
            {
                _logger.LogWarning($"No quote found for stock {isin} before {referenceDate:yyyy-MM-dd}");
                return 0;
            }

            return latestQuote.PricePerShare;
        }

        /// <summary>
        /// Calculates the total value of a real estate investment (land value + building value)
        /// </summary>
        /// <param name="investment">The real estate investment entity</param>
        /// <param name="referenceDate">The reference date for valuation</param>
        /// <returns>The total real estate value</returns>
        private async Task<decimal> CalculateRealEstateValueAsync(Investment investment, DateTime referenceDate)
        {
            var transactionDate = DateOnly.FromDateTime(referenceDate);

            // Calculate land value
            var estateValue = await CalculateEstateValueAsync(investment, transactionDate);

            // Calculate building value
            var buildingValue = await CalculateBuildingValueAsync(investment, transactionDate);

            // Real estate value = land value + building value
            decimal totalValue = estateValue + buildingValue;

            _logger.LogDebug(
                $"Real estate {investment.InvestmentId} value: Land {estateValue} + Building {buildingValue} = {totalValue}");

            return totalValue;
        }

        /// <summary>
        /// Calculates the land value component of a real estate investment
        /// </summary>
        /// <param name="investment">The real estate investment entity</param>
        /// <param name="transactionDate">The reference date for finding transactions</param>
        /// <returns>The latest land value or 0 if no estate transactions are found</returns>
        private async Task<decimal> CalculateEstateValueAsync(Investment investment, DateOnly transactionDate)
        {
            var estateTransactions = await _transactionRepository.GetTransactionsForInvestmentAsync(
                investment.InvestmentId,
                ESTATE_TRANSACTION_ID,
                transactionDate);

            if (!estateTransactions.Any())
            {
                _logger.LogWarning($"No estate value found for property {investment.InvestmentId}");
                return 0;
            }

            // Find the value from the latest transaction
            return FindLatestTransactionValue(estateTransactions);
        }

        /// <summary>
        /// Calculates the building value component of a real estate investment
        /// </summary>
        /// <param name="investment">The real estate investment entity</param>
        /// <param name="transactionDate">The reference date for finding transactions</param>
        /// <returns>The latest building value or 0 if no building transactions are found</returns>
        private async Task<decimal> CalculateBuildingValueAsync(Investment investment, DateOnly transactionDate)
        {
            var buildingTransactions = await _transactionRepository.GetTransactionsForInvestmentAsync(
                investment.InvestmentId,
                BUILDING_TRANSACTION_ID,
                transactionDate);

            if (!buildingTransactions.Any())
            {
                _logger.LogWarning($"No building value found for property {investment.InvestmentId}");
                return 0;
            }

            // Find the value from the latest transaction
            return FindLatestTransactionValue(buildingTransactions);
        }

        /// <summary>
        /// Finds the value of the transaction with the most recent date
        /// </summary>
        /// <param name="transactions">Collection of transactions to search through</param>
        /// <returns>The value of the most recent transaction</returns>
        /// <remarks>Assumes the collection contains at least one transaction</remarks>
        private decimal FindLatestTransactionValue(IEnumerable<Transaction> transactions)
        {
            var latestTransaction = transactions.First();
            foreach (var transaction in transactions)
            {
                if (transaction.TransactionDate > latestTransaction.TransactionDate)
                {
                    latestTransaction = transaction;
                }
            }

            return latestTransaction.Value;
        }

        /// <summary>
        /// Calculates the value of a fund investment for an investor
        /// </summary>
        /// <param name="fund">The fund investment entity</param>
        /// <param name="investorId">The ID of the investor</param>
        /// <param name="referenceDate">The reference date for valuation</param>
        /// <param name="processedFunds">Set of already processed fund IDs to prevent circular references</param>
        /// <returns>The investor's share of the fund value based on their investment percentage</returns>
        private async Task<decimal> CalculateFundValueAsync(
            Investment fund, int investorId, DateTime referenceDate, HashSet<int> processedFunds)
        {
            // Calculate the investment percentage in the fund
            var transactionDate = DateOnly.FromDateTime(referenceDate);
            decimal investmentPercentage =
                await CalculateFundInvestmentPercentageAsync(fund.InvestmentId, transactionDate);

            if (investmentPercentage <= 0)
            {
                _logger.LogInformation($"No investment percentage in fund {fund.InvestmentId}");
                return 0;
            }

            // Calculate the total value of the fund
            decimal totalFundValue =
                await CalculateTotalFundValueAsync(fund, investorId, referenceDate, processedFunds);

            // Calculate investor's share: investment percentage * total fund value
            decimal investorShare = (investmentPercentage / 100m) * totalFundValue;

            _logger.LogDebug(
                $"Fund {fund.InvestmentId} value calculation: {investmentPercentage}% of {totalFundValue} = {investorShare}");

            return investorShare;
        }

        /// <summary>
        /// Calculates the total value of all investments within a fund
        /// </summary>
        /// <param name="fund">The fund investment entity</param>
        /// <param name="investorId">The ID of the investor</param>
        /// <param name="referenceDate">The reference date for valuation</param>
        /// <param name="processedFunds">Set of already processed fund IDs to prevent circular references</param>
        /// <returns>The sum of all investment values within the fund</returns>
        private async Task<decimal> CalculateTotalFundValueAsync(
            Investment fund, int investorId, DateTime referenceDate, HashSet<int> processedFunds)
        {
            var fundInvestments = await _investmentRepository.GetFundInvestmentsAsync(fund.InvestmentId);

            if (!fundInvestments.Any())
            {
                _logger.LogWarning($"Fund {fund.InvestmentId} has no investments");
                return 0;
            }

            decimal totalFundValue = 0;
            foreach (var investment in fundInvestments)
            {
                decimal investmentValue =
                    await CalculateInvestmentValueAsync(investment, investorId, referenceDate, processedFunds);
                totalFundValue += investmentValue;
            }

            return totalFundValue;
        }

        /// <summary>
        /// Calculates the total investment percentage in a fund as of the reference date
        /// </summary>
        /// <param name="fundId">The ID of the fund</param>
        /// <param name="referenceDate">The reference date for finding transactions</param>
        /// <returns>The sum of all percentage transactions, ensuring it's not negative</returns>
        private async Task<decimal> CalculateFundInvestmentPercentageAsync(int fundId, DateOnly referenceDate)
        {
            // Retrieve percentage transactions for the fund
            var percentageTransactions = await _transactionRepository.GetTransactionsForInvestmentAsync(
                fundId,
                PERCENTAGE_TRANSACTION_ID,
                referenceDate);

            // Calculate total investment percentage
            decimal totalPercentage = 0;
            foreach (var transaction in percentageTransactions)
            {
                totalPercentage += transaction.Value;
            }

            // Ensure percentage is not negative
            return Math.Max(0, totalPercentage);
        }
    }
}