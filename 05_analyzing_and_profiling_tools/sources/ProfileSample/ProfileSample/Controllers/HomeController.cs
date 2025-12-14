using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileSample.DAL;
using ProfileSample.Models;

namespace ProfileSample.Controllers;

public class HomeController(ProfileSampleContext context, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await context.ImgSources
            .Take(20)
            .Select(x => new ImageModel
            {
                Name = x.Name,
                Data = x.Data
            })
            .ToListAsync();

        return View(model);
    }

    public async Task<IActionResult> Convert()
    {
        var imgPath = Path.Combine(environment.WebRootPath, "Content", "Img");
        var files = Directory.GetFiles(imgPath, "*.jpg");
        List<ImgSource> entities = [];
        foreach (var file in files)
        {
            byte[] buff = await System.IO.File.ReadAllBytesAsync(file);

            var entity = new ImgSource
            {
                Name = Path.GetFileName(file),
                Data = buff,
            };
            entities.Add(entity);
        }
        context.ImgSources.AddRange(entities);
        await context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    public IActionResult Contact()
    {
        ViewBag.Message = "Your contact page.";
        return View();
    }

    public IActionResult About()
    {
        ViewBag.Message = "Your application description page.";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}