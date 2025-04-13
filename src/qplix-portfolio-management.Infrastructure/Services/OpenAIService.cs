// qplix-portfolio-management.Infrastructure/Services/OpenAIService.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Services.Advisors.Dtos;
using qplix_portfolio_management.Application.Services.Portfolios.Dtos;
using System.Text;
using System.Text.Json;
using qplix_portfolio_management.Application.Services.Investments.Dtos;

namespace qplix_portfolio_management.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IAiAdvisorService using OpenAI's API
    /// </summary>
    public class OpenAIService : IAiAdvisorService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _modelName;

        /// <summary>
        /// Constructor for OpenAIService
        /// </summary>
        public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;

            // Get configuration values
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey");
            _apiUrl = configuration["OpenAI:ApiUrl"] ?? "https://api.openai.com/v1/chat/completions";
            _modelName = configuration["OpenAI:ModelName"] ?? "gpt-4o";
        }

        /// <summary>
        /// Generates investment advice based on portfolio data
        /// </summary>
        public async Task<InvestmentAdviceDto> GenerateAdviceFromPortfolioAsync(
            PortfolioValueResponseDto portfolioData,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Generating investment advice for portfolio of investor {InvestorId}",
                    portfolioData.InvestorId);

                // Create prompt for OpenAI
                string prompt = CreatePrompt(portfolioData);

                // Call OpenAI API
                var response = await CallOpenAiAsync(prompt, cancellationToken);

                // Parse the response into recommendations and predictions
                var result = ParseOpenAiResponse(response);

                _logger.LogInformation("Successfully generated investment advice for investor {InvestorId}",
                    portfolioData.InvestorId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating investment advice for investor {InvestorId}",
                    portfolioData.InvestorId);

                // Return fallback advice in case of error
                return new InvestmentAdviceDto
                {
                    Recommendations = new List<string>
                    {
                        "Consider reviewing your portfolio allocation",
                        "Consult with a financial advisor",
                        "Ensure your investments align with your long-term goals"
                    },
                    Predictions = new List<string>
                    {
                        "Markets may experience volatility in the coming months",
                        "Economic indicators suggest cautious optimism"
                    }
                };
            }
        }

        /// <summary>
        /// Creates a prompt for the AI based on portfolio data
        /// </summary>
        private string CreatePrompt(PortfolioValueResponseDto portfolioData)
        {
            return $@"
You are a professional financial advisor. Based on the following portfolio data as of {portfolioData.ReferenceDate:yyyy-MM-dd},
please provide:

1. Exactly 3 specific investment recommendations
2. Exactly 2 market predictions for the next 6 months

Portfolio data:
- Total Portfolio Value: {portfolioData.TotalValue}
- Number of investments: {portfolioData.InvestmentValues.Count}
- Investment breakdown:
{FormatInvestmentData(portfolioData.InvestmentValues)}

Format your response in JSON with the following structure:
{{
  ""recommendations"": [""recommendation1"", ""recommendation2"", ""recommendation3""],
  ""predictions"": [""prediction1"", ""prediction2""]
}}

Your recommendations should be specific, actionable and based on the portfolio composition.
Your predictions should be balanced and realistic.
";
        }

        /// <summary>
        /// Formats investment data for inclusion in the prompt
        /// </summary>
        private string FormatInvestmentData(List<InvestmentResponseDto> investments)
        {
            var sb = new StringBuilder();

            // Grup yatırımları türlerine göre
            var investmentsByType = new Dictionary<string, decimal>();
            var totalInvestmentCount = investments.Count;

            foreach (var investment in investments)
            {
                if (investmentsByType.ContainsKey(investment.InvestmentType))
                {
                    investmentsByType[investment.InvestmentType] += investment.Value;
                }
                else
                {
                    investmentsByType[investment.InvestmentType] = investment.Value;
                }
            }

            // Özet bilgileri formatla
            foreach (var type in investmentsByType.Keys)
            {
                var value = investmentsByType[type];
                var percentage = 100 * value / investments.Sum(i => i.Value);
                sb.AppendLine($"  * {type}: {value:N2} ({percentage:N2}% of portfolio)");
            }

            // En büyük 10 yatırımı listele
            sb.AppendLine("\nTop 10 largest investments:");
            var top10 = investments.OrderByDescending(i => i.Value).Take(10);
            foreach (var investment in top10)
            {
                sb.AppendLine(
                    $"  * {investment.InvestmentCode} ({investment.InvestmentType}): {investment.Value:N2} ({investment.PercentageOfPortfolio}% of portfolio)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Calls the OpenAI API with the specified prompt
        /// </summary>
        private async Task<string> CallOpenAiAsync(string prompt, CancellationToken cancellationToken)
        {
            // Set up the request
            var request = new
            {
                model = _modelName,
                messages = new[]
                {
                    new { role = "system", content = "You are a professional financial advisor." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 500
            };

            var requestJson = JsonSerializer.Serialize(request);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // Set the API key
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // Send the request
            var response = await _httpClient.PostAsync(_apiUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Read the response
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            // Extract the content from the response
            using var doc = JsonDocument.Parse(responseBody);
            var choices = doc.RootElement.GetProperty("choices");
            var firstChoice = choices[0];
            var message = firstChoice.GetProperty("message");
            var messageContent = message.GetProperty("content").GetString();

            return messageContent ?? string.Empty;
        }

        /// <summary>
        /// Parses the OpenAI response into an InvestmentAdviceDto
        /// </summary>
        private InvestmentAdviceDto ParseOpenAiResponse(string openAiResponse)
        {
            try
            {
                // Attempt to parse the JSON response
                using var doc = JsonDocument.Parse(openAiResponse);
                var root = doc.RootElement;

                var result = new InvestmentAdviceDto
                {
                    Recommendations = new List<string>(),
                    Predictions = new List<string>()
                };

                if (root.TryGetProperty("recommendations", out var recommendations))
                {
                    foreach (var recommendation in recommendations.EnumerateArray())
                    {
                        result.Recommendations.Add(recommendation.GetString() ?? string.Empty);
                    }
                }

                if (root.TryGetProperty("predictions", out var predictions))
                {
                    foreach (var prediction in predictions.EnumerateArray())
                    {
                        result.Predictions.Add(prediction.GetString() ?? string.Empty);
                    }
                }

                // Ensure we have exactly 3 recommendations and 2 predictions
                while (result.Recommendations.Count < 3)
                {
                    result.Recommendations.Add("Consider diversifying your investment portfolio.");
                }

                while (result.Predictions.Count < 2)
                {
                    result.Predictions.Add("Market conditions may change based on economic factors.");
                }

                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing OpenAI response: {Response}", openAiResponse);

                // If parsing fails, return default recommendations
                return new InvestmentAdviceDto
                {
                    Recommendations = new List<string>
                    {
                        "Consider diversifying your investment portfolio.",
                        "Review your asset allocation regularly.",
                        "Ensure you have sufficient liquidity for unexpected expenses."
                    },
                    Predictions = new List<string>
                    {
                        "Market volatility may continue in the coming months.",
                        "Economic indicators suggest cautious optimism for growth sectors."
                    }
                };
            }
        }
    }
}