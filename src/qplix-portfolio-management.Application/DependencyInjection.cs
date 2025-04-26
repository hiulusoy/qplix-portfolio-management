using Microsoft.Extensions.DependencyInjection;
using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Application.Services;
using qplix_portfolio_management.Application.Services.Cities;
using qplix_portfolio_management.Application.Services.Investors;
using qplix_portfolio_management.Application.Services.Portfolios;

namespace qplix_portfolio_management.Application;

public static class DependencyInjection
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<IPortfolioCalculationService, PortfolioCalculationService>();
        services.AddScoped<IInvestorService, InvestorService>();
        return services;
    }
}