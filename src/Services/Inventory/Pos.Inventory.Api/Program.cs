using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.BuildingBlocks.Web;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.Inventory.Application;
using Pos.Inventory.Infrastructure;
using Pos.Inventory.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services
    .AddInventoryApplication()
    .AddInventoryInfrastructure(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly);

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.Services.InitializeDatabaseAsync<InventoryDbContext>();
await app.RunAsync();
