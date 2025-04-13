// qplix-portfolio-management.Application/Abstractions/Interfaces/IAiAdvisorService.cs

using System.Threading;
using System.Threading.Tasks;
using qplix_portfolio_management.Application.Services.Advisors.Dtos;
using qplix_portfolio_management.Application.Services.Portfolios.Dtos;

namespace qplix_portfolio_management.Application.Abstractions.Interfaces
{
    /// <summary>
    /// Service for generating AI-powered investment advice and predictions
    /// </summary>
    public interface IAiAdvisorService
    {
        /// <summary>
        /// Generates investment recommendations and future predictions based on portfolio data
        /// </summary>
        /// <param name="portfolioData">Portfolio data used as basis for recommendations</param>
        /// <param name="cancellationToken">Token to cancel the operation if needed</param>
        /// <returns>Investment advice containing recommendations and predictions</returns>
        Task<InvestmentAdviceDto> GenerateAdviceFromPortfolioAsync(
            PortfolioValueResponseDto portfolioData,
            CancellationToken cancellationToken = default);
    }
}