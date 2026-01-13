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
        c.IncludeXmlComments(xmlPath);
});

// Initialize and register the in-memory data store as Singleton (simulates external DB)
var dataStore = new InMemoryDataStore();
if (builder.Environment.IsDevelopment())
{
    await DataSeeder.InitializeAsync(dataStore);
}
builder.Services.AddSingleton(dataStore);

// Register repositories as Scoped (abstraction over data access)
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