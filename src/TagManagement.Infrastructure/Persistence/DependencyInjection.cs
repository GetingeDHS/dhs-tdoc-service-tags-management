using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagManagement.Core.Interfaces;
using TagManagement.Infrastructure.Persistence;
using TagManagement.Infrastructure.Persistence.Repositories;

namespace TagManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext - configured to use existing TDOC database schema
            services.AddDbContext<TagManagementDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add repositories - using TDOC-aware implementation
            services.AddScoped<ITagRepository, TDocTagRepository>();

            return services;
        }
    }
}
