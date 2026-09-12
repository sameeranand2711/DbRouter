using DbRouter.SampleApi.Endpoints;
using DbRouter.SampleApi.Extensions;
using DbRouter.SampleApi.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSampleDataAccess(builder.Configuration);

WebApplication app = builder.Build();

app.UseMiddleware<CustomerDatabaseSelectionMiddleware>();
app.MapDatabaseEndpoints();
app.MapExplicitResolutionEndpoints();
app.MapScopedResolutionEndpoints();
app.MapEfCoreResolutionEndpoints();

app.Run();
