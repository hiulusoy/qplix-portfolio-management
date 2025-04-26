using System.Threading.Tasks;

namespace qplix_portfolio_management.Infrastructure.Abstractions
{
    /// <summary>
    /// Interface for Bundesbank API Client
    /// </summary>
    public interface IBundesbankCurrencyService
    {
        /// <summary>
        /// Retrieves all available exchange rate data for a specific dataflow
        /// </summary>
        Task<string> GetAllDataAsync(
            string flowRef = null, 
            string format = "json", 
            string lang = "en");

        /// <summary>
        /// Retrieves specific exchange rate data for a given currency key
        /// </summary>
        Task<string> GetDataAsync(
            string key, 
            string flowRef = null, 
            string format = "json", 
            string lang = "en");

        /// <summary>
        /// Retrieves exchange rate data for a specific currency within a date range
        /// </summary>
        Task<string> GetDataForDateRangeAsync(
            string key, 
            string startPeriod, 
            string endPeriod, 
            string flowRef = null, 
            string format = "json", 
            string lang = "en");

        /// <summary>
        /// Retrieves metadata for a specific dataflow
        /// </summary>
        Task<string> GetDataflowAsync(
            string resourceID = null, 
            string format = "struct_xml", 
            string lang = "en", 
            string references = "all");
    }
}