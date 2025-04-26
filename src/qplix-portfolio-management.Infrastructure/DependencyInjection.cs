using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using qplix_portfolio_management.Application.Abstractions.Interfaces;
using qplix_portfolio_management.Infrastructure.Abstractions;
using qplix_portfolio_management.Infrastructure.Services;


namespace qplix_portfolio_management.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterExternalServices(this IServiceCollection services)
        {
            services.AddScoped<IAiAdvisorService, OpenAIService>();
            return services;
        }
    }
}