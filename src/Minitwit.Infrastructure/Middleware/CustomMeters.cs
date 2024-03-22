using Prometheus;

namespace Minitwit.Infrastructure.Middleware;

public class CustomMeters
{
    private static readonly Counter ApiRequestsCounter = Metrics.CreateCounter(
        "minitwit_api_requests_total",
        "Number of API requests"
    );

    public static void IncrementApiRequestsCounter()
    {
        ApiRequestsCounter.Inc();
    }
}