using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.BuildingBlocks.Web;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.Ordering.Application;
using Pos.Ordering.Infrastructure;
using Pos.Ordering.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services
    .AddOrderingApplication()
    .AddOrderingInfrastructure(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly);

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.Services.InitializeDatabaseAsync<OrderingDbContext>();
await app.RunAsync();
