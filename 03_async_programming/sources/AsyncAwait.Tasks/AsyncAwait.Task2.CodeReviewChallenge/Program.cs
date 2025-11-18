using AsyncAwait.Task2.CodeReviewChallenge.Extensions;
using AsyncAwait.Task2.CodeReviewChallenge.Models.Support;
using AsyncAwait.Task2.CodeReviewChallenge.Services;
using CloudServices;
using CloudServices.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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

// Add MVC (with endpoint routing disabled, as in your original code)
builder.Services.AddMvc(options => options.EnableEndpointRouting = false);

var app = builder.Build();

// Middleware configuration
// Note: The 'if (false && env.IsDevelopment())' block is always false, so only UseExceptionHandler is used.
app.UseExceptionHandler("/Home/Error");

app.UseStatistic();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

// UseMvc is still supported for legacy scenarios, but consider migrating to endpoint routing in the future.
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});

await app.RunAsync();