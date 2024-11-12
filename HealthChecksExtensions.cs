 public static class HealthChecksExtensions
    {
        public static void AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["HealthChecks:Url"];

            services.AddHealthChecks()
                .AddTypeActivatedCheck<HealthCheckApiWithArgs>(
                "Custom Health Check with Args",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { $"EndPoint Services: {url}" },
                args: new object[] { url }
                );
        }
    }
