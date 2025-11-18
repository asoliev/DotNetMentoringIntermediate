using AsyncAwait.Task2.CodeReviewChallenge.Headers;
using CloudServices.Interfaces;

namespace AsyncAwait.Task2.CodeReviewChallenge.Middleware;

public class StatisticMiddleware(RequestDelegate next, IStatisticService statisticService)
{
    private readonly IStatisticService _statisticService = statisticService ?? throw new ArgumentNullException(nameof(statisticService));

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path;

        var staticRegTask = Task.Run(
            () => _statisticService.RegisterVisitAsync(path)
                .ConfigureAwait(false)
                .GetAwaiter().OnCompleted(UpdateHeaders));
        Console.WriteLine(staticRegTask.Status); // just for debugging purposes

        void UpdateHeaders()
        {
            context.Response.Headers.Append(
                CustomHttpHeaders.TotalPageVisits,
                _statisticService.GetVisitsCountAsync(path).GetAwaiter().GetResult().ToString());
        }

        Thread.Sleep(3000); // without this the statistic counter does not work
        await next(context);
    }
}