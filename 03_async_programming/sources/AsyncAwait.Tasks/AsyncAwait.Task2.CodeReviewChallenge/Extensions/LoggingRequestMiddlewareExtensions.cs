using AsyncAwait.Task2.CodeReviewChallenge.Middleware;

namespace AsyncAwait.Task2.CodeReviewChallenge.Extensions;

public static class LoggingRequestMiddlewareExtensions
{
    public static IApplicationBuilder UseStatistic(this IApplicationBuilder builder) => builder.UseMiddleware<StatisticMiddleware>();
}