using Microsoft.Extensions.Diagnostics.HealthChecks;

public class HealthCheckApiWithArgs : IHealthCheck
    {
        private readonly string _url;

        public HealthCheckApiWithArgs(string url)
        {
            _url = url;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(_url);

            HttpResponseMessage response = await client.GetAsync("");

            return response.StatusCode == HttpStatusCode.OK ?
                await Task.FromResult(new HealthCheckResult(
                      status: HealthStatus.Healthy,
                      description: "The API is healthy :)"
                       )) :
                await Task.FromResult(new HealthCheckResult(
                      status: context.Registration.FailureStatus,
                      description: $"API does not work because {response.ReasonPhrase} :("));
        }
    }
