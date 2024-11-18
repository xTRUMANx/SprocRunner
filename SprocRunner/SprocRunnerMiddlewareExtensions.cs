using SprocRunner;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class SprocRunnerMiddlewareExtensions
    {
        public static IApplicationBuilder UseSprocRunner(this IApplicationBuilder app, Action<SprocRunnerOptions> setupAction = null)
        {
            var options = app.ApplicationServices.GetService(typeof(SprocRunnerOptions)) as SprocRunnerOptions;

            if (options.ConnectionString is null)
            {
                throw new ArgumentNullException(nameof(options.ConnectionString), "ConnectionString can not be null!");
            }

            return app.UseMiddleware<SprocRunnerMiddleware>(options);
        }
    }
}
