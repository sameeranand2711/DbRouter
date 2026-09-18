using DbRouter.Core.Abstractions.Resolvers;
using DbRouter.SampleApi.Configuration;
using DbRouter.SampleApi.Endpoints;
using DbRouter.SampleApi.Extensions;
using DbRouter.SampleApi.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSampleDataAccess(builder.Configuration);

WebApplication app = builder.Build();

_ = app.Services.GetRequiredService<IDbRouter<DatabaseKey>>();

if (app.Configuration.GetValue<bool>("Sample:InitializeDatabases"))
{
    await app.Services.InitializeSampleDataAsync();
}

app.UseMiddleware<CustomerDatabaseSelectionMiddleware>();
app.MapDatabaseEndpoints();
app.MapExplicitResolutionEndpoints();
app.MapScopedResolutionEndpoints();
app.MapEfCoreResolutionEndpoints();

app.Run();
