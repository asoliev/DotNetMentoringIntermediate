using CloudServices.Interfaces;

namespace AsyncAwait.Task2.CodeReviewChallenge.Models.Support;

public class ManualAssistant(ISupportService supportService) : IAssistant
{
    private readonly ISupportService _supportService = supportService ?? throw new ArgumentNullException(nameof(supportService));

    public async Task<string> RequestAssistanceAsync(string requestInfo)
    {
        try
        {
            var t = _supportService.RegisterSupportRequestAsync(requestInfo);

            Console.WriteLine(t.Status); // this is for debugging purposes
            Thread.Sleep(5000); // this is just to be sure that the request is registered

            return await _supportService.GetSupportInfoAsync(requestInfo)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return await Task.Run(async () =>
                await Task.FromResult($"Failed to register assistance request. Please try later. {ex.Message}"));
        }
    }
}