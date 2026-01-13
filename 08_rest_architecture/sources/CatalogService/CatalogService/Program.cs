using CatalogService.Data;
using CatalogService.Repositories;
using CatalogService.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Initialize and register data stores as Singletons (simulate DB tables)
var categoryStore = new CategoryDataStore();
var itemStore = new ItemDataStore();

if (builder.Environment.IsDevelopment())
{
    await DataSeeder.InitializeAsync(categoryStore, itemStore);
}

builder.Services.AddSingleton(categoryStore);
builder.Services.AddSingleton(itemStore);

// Register repositories as Scoped (data access layer)
builder.Services.AddScoped<ICategoryRepository, InMemoryCategoryRepository>();
builder.Services.AddScoped<IItemRepository, InMemoryItemRepository>();

// Register services as Scoped (business logic layer)
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IItemService, ItemService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();