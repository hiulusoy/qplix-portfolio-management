using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using qplix_portfolio_management.Infrastructure.Abstractions;

namespace qplix_portfolio_management.Infrastructure.Services
{
    /// <summary>
    /// Configuration options for Bundesbank API client
    /// </summary>
    public class BundesbankApiOptions
    {
        public const string SectionName = "Bundesbank";

        /// <summary>
        /// Base URL for the Bundesbank API
        /// </summary>
        public string ApiUrl { get; set; } = "https://api.statistiken.bundesbank.de/rest";

        /// <summary>
        /// Default Dataflow ID (typically 'BBEX3' for exchange rates)
        /// </summary>
        public string DataflowId { get; set; } = "BBEX3";
    }

    /// <summary>
    /// Implementation of Bundesbank API Client
    /// </summary>
    public class BundesbankCurrencyService : IBundesbankCurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly BundesbankApiOptions _options;

        public BundesbankCurrencyService(
            HttpClient httpClient,
            IOptions<BundesbankApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task<string> GetAllDataAsync(
            string flowRef = null,
            string format = "json",
            string lang = "en")
        {
            flowRef ??= _options.DataflowId;

            var response = await _httpClient.GetAsync(
                $"{_options.ApiUrl}/data/{flowRef}?format={format}&lang={lang}"
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <inheritdoc/>
        public async Task<string> GetDataAsync(
            string key,
            string flowRef = null,
            string format = "json",
            string lang = "en")
        {
            flowRef ??= _options.DataflowId;

            var response = await _httpClient.GetAsync(
                $"{_options.ApiUrl}/data/{flowRef}/{key}?format={format}&lang={lang}"
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <inheritdoc/>
        public async Task<string> GetDataForDateRangeAsync(
            string key,
            string startPeriod,
            string endPeriod,
            string flowRef = null,
            string format = "json",
            string lang = "en")
        {
            flowRef ??= _options.DataflowId;

            var response = await _httpClient.GetAsync(
                $"{_options.ApiUrl}/data/{flowRef}/{key}?format={format}&lang={lang}&startPeriod={startPeriod}&endPeriod={endPeriod}"
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <inheritdoc/>
        public async Task<string> GetDataflowAsync(
            string resourceID = null,
            string format = "struct_xml",
            string lang = "en",
            string references = "all")
        {
            resourceID ??= _options.DataflowId;

            var response = await _httpClient.GetAsync(
                $"{_options.ApiUrl}/metadata/dataflow/BBK/{resourceID}?format={format}&lang={lang}&references={references}"
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}