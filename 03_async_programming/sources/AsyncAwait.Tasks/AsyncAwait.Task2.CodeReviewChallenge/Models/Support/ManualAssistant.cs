using CloudServices.Interfaces;

namespace AsyncAwait.Task2.CodeReviewChallenge.Models.Support;

public class ManualAssistant(ISupportService supportService) : IAssistant
{
    private readonly ISupportService _supportService = supportService ?? throw new ArgumentNullException(nameof(supportService));

    public async Task<string> RequestAssistanceAsync(string requestInfo)
    {
        try
        {
            // Await registration so we know it's completed before getting info.
            await _supportService.RegisterSupportRequestAsync(requestInfo);

            // Then query support info.
            return await _supportService.GetSupportInfoAsync(requestInfo);
        }
        catch (HttpRequestException ex)
        {
            // Return a completed string task — no need to wrap in Task.Run.
            return $"Failed to register assistance request. Please try later. {ex.Message}";
        }
    }
}