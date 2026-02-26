using Amherst.GraphQL.Api.Queries;
using Amherst.GraphQL.Infrastructure;
using Amherst.GraphQL.Infrastructure.Postgres;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddGraphQLServer()
    .AddSpatialTypes()
    .AddFiltering()
    .AddProjections()
    .AddSpatialFiltering()
    .AddSpatialProjections()
    .AddQueryType()
    .AddTypeExtension(typeof(GeoQueries))
    .AddSorting()
    .RegisterDbContextFactory<GeoDbContext>();

var app = builder.Build();

app.MapGraphQL();

app.Run();
