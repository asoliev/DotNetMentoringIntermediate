using AsyncAwait.Task2.CodeReviewChallenge.Extensions;
using AsyncAwait.Task2.CodeReviewChallenge.Models.Support;
using AsyncAwait.Task2.CodeReviewChallenge.Services;
using CloudServices;
using CloudServices.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region builder services
// Service registration
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddSingleton<IStatisticService, CloudStatisticService>();
builder.Services.AddSingleton<ISupportService, CloudSupportService>();
builder.Services.AddSingleton<IPrivacyDataService, PrivacyDataService>();
builder.Services.AddScoped<IAssistant, ManualAssistant>();

// Use controllers + views and Razor Pages (endpoint routing enabled by default)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
#endregion

var app = builder.Build();

#region WebApplication configuration
// Middleware configuration
// Note: The 'if (false && env.IsDevelopment())' block is always false, so only UseExceptionHandler is used.
app.UseExceptionHandler("/Home/Error");

app.UseStatistic();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
#endregion

await app.RunAsync();