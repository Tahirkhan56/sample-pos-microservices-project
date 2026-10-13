using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.BuildingBlocks.Web;
using Pos.BuildingBlocks.Web.Endpoints;
using Pos.Payments.Application;
using Pos.Payments.Infrastructure;
using Pos.Payments.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services
    .AddPaymentsApplication()
    .AddPaymentsInfrastructure(builder.Configuration)
    .AddEndpoints(typeof(Program).Assembly);

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
app.MapEndpoints();

await app.Services.InitializeDatabaseAsync<PaymentsDbContext>();
await app.RunAsync();
