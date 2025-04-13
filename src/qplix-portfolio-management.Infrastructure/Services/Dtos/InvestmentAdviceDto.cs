namespace qplix_portfolio_management.Application.Services.Advisors.Dtos
{
    /// <summary>
    /// Data transfer object for investment advice and predictions
    /// </summary>
    public class InvestmentAdviceDto
    {
        /// <summary>
        /// List of investment recommendations
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();

        /// <summary>
        /// List of future predictions
        /// </summary>
        public List<string> Predictions { get; set; } = new List<string>();
    }
}