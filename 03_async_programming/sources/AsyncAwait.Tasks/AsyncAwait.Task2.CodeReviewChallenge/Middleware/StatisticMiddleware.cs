using AsyncAwait.Task2.CodeReviewChallenge.Headers;
using CloudServices.Interfaces;

namespace AsyncAwait.Task2.CodeReviewChallenge.Middleware;

public class StatisticMiddleware(RequestDelegate next, IStatisticService statisticService)
{
    private readonly IStatisticService _statisticService = statisticService ?? throw new ArgumentNullException(nameof(statisticService));
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path;

        // Await registration and retrieval of the visit count. Avoid blocking calls and Thread.Sleep.
        // If you don't want to delay the response by waiting for registration, consider using a proper background queue.
        await _statisticService.RegisterVisitAsync(path);

        var visits = await _statisticService.GetVisitsCountAsync(path);

        context.Response.Headers.Append(CustomHttpHeaders.TotalPageVisits, visits.ToString());

        await _next(context);
    }
}