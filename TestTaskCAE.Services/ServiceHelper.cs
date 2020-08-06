using Microsoft.Extensions.DependencyInjection;
using TestTaskCAE.BaseClasses;
using TestTaskCAE.DAL;

namespace TestTaskCAE.Services
{
    public static class ServiceHelper
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderDataService, OrderDataService>();
            services.AddDalServices();
        }
    }
}
