using SprocRunner;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SprocRunnerMiddlewareServiceCollectionExtensions
    {
        public static IServiceCollection AddSprocRunner(this IServiceCollection services, Action<SprocRunnerOptions> setupAction = null)
        {
            var options = new SprocRunnerOptions();
            setupAction?.Invoke(options);

            services.AddSingleton(options);

            return services;
        }
    }
}
