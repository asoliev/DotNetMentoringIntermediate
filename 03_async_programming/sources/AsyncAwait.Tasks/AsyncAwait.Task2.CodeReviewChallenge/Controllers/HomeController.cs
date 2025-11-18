using System.Diagnostics;
using AsyncAwait.Task2.CodeReviewChallenge.Models;
using AsyncAwait.Task2.CodeReviewChallenge.Models.Support;
using AsyncAwait.Task2.CodeReviewChallenge.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsyncAwait.Task2.CodeReviewChallenge.Controllers;

public class HomeController(IAssistant assistant, IPrivacyDataService privacyDataService) : Controller
{
    private readonly IAssistant _assistant = assistant ?? throw new ArgumentNullException(nameof(assistant));

    private readonly IPrivacyDataService _privacyDataService = privacyDataService ?? throw new ArgumentNullException(nameof(privacyDataService));

    public ActionResult Index() => View();

    public ActionResult Privacy()
    {
        ViewBag.Message = _privacyDataService.GetPrivacyDataAsync().Result;
        return View();
    }

    public async Task<IActionResult> Help()
    {
        ViewBag.RequestInfo = await _assistant.RequestAssistanceAsync("guest").ConfigureAwait(false);
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}