using System;
using System.Threading.Tasks;
using Elastic.Data;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Elastic.Configuration
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddTransient<ClientProvider>()
                .AddSerialization();
        }

        public static IServiceCollection AddSerialization(this IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            return services;
        }
    }
}
