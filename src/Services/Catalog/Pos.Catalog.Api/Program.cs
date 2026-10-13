using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.BuildingBlocks.Web;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.Catalog.Application;
using Pos.Catalog.Infrastructure;
using Pos.Catalog.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services
    .AddCatalogApplication()
    .AddCatalogInfrastructure(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly);

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.Services.InitializeDatabaseAsync<CatalogDbContext>(CatalogSeeder.SeedAsync);
await app.RunAsync();
