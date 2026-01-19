using Microsoft.EntityFrameworkCore;
using ProfileSample.DAL;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework
builder.Services.AddDbContext<ProfileSampleContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure MiniProfiler
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
})
.AddEntityFramework();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use MiniProfiler
app.UseMiniProfiler();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();