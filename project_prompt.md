# Project: Amherst GraphQL API — Geo Service

## Goal

Build a .NET 9 / Hot Chocolate v15 GraphQL API that exposes geographic shape data from a
Postgres/PostGIS table. The solution uses a strict hexagonal (ports & adapters) architecture.

---

## Technology Stack

| Concern | Technology | Version |
|---------|-----------|---------|
| Runtime | .NET | 9.0 |
| Language | C# | latest (`LangVersion=latest`) |
| Web framework | ASP.NET Core | 9.0 |
| GraphQL server | Hot Chocolate | 15.1.x |
| GraphQL filtering/sorting | HotChocolate.Data | 15.1.x |
| EF/HC integration | HotChocolate.Data.EntityFramework | 15.1.x |
| ORM | Entity Framework Core (via Npgsql) | 9.0.x |
| Database driver | Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.x |
| Config binding | Microsoft.Extensions.Options + ConfigurationExtensions + Configuration.Binder | 9.0.x |

---

## Solution Structure

Four projects under `src/`, with a `Directory.Build.props` at the solution root that sets
`net9.0`, `Nullable=enable`, `ImplicitUsings=enable`, and `LangVersion=latest` globally so
individual csproj files stay minimal.

```
amherst_graphql/
  Directory.Build.props
  Amherst.GraphQL.sln
  src/
    Amherst.GraphQL.Domain/
    Amherst.GraphQL.Application/
    Amherst.GraphQL.Infrastructure/
    Amherst.GraphQL.Api/
```

Dependency direction is strictly one-way: **Domain ← Application ← Infrastructure ← Api**.
No project may reference a layer above it.

---

## Architecture Rules

- **Domain entities are clean POCOs** — no EF attributes, no BSON attributes, no HC
  annotations. All mapping lives in Infrastructure.
- **Port interfaces live in Application** — Infrastructure implements them; Api consumes them.
- **Infrastructure must not reference Hot Chocolate packages** — HC-specific integrations
  belong only in Api.
- **EF Core is read-only** — `UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` set
  globally on the context factory; repositories also call `.AsNoTracking()`.
- **`ApplyConfiguration` is explicit** — call `modelBuilder.ApplyConfiguration(new XyzConfig())`
  directly in `OnModelCreating`, not `ApplyConfigurationsFromAssembly`, to avoid cross-context
  bleed if a second `DbContext` is added later.

---

## Database Schema

```sql
CREATE TABLE geo_shapes (
    geo_src        text NULL,
    geo_type_code  text NULL,
    geo_type_name  text NULL,
    geo_value      text NULL,
    geo_name       text NULL,
    wkt_polygon    text NULL,
    spatial_index  int8 NULL,
    geo_polygon    public.geography NULL  -- PostGIS geography column, GIST-indexed
);
```

---

## Domain Layer (`Amherst.GraphQL.Domain`)

One entity: **`Geo`** (namespace `Amherst.GraphQL.Domain.Entities`).

Maps the seven non-spatial columns from `geo_shapes`. The `geo_polygon` PostGIS geography
column is intentionally **excluded** from the domain entity entirely — it is only accessed
via raw SQL.

Composite primary key columns `geo_type_code` and `geo_value` are non-nullable `string`
(initialized to `string.Empty`) because the DDL treats them as NOT NULL in practice. All
other columns are nullable.

No NetTopologySuite dependency anywhere in the solution.

---

## Application Layer (`Amherst.GraphQL.Application`)

One port interface: **`IGeoRepository`** (namespace `Amherst.GraphQL.Application.Ports`).

Exposes two methods, both returning `IQueryable<Geo>` to allow Hot Chocolate's
`[UseFiltering]` and `[UseSorting]` middleware to compose additional LINQ operators at
the resolver level:

- **`Query(string geoTypeCode, string geoValue)`** — exact-match lookup by the composite key
- **`QueryContaining(double latitude, double longitude)`** — spatial point-in-polygon search

---

## Infrastructure Layer (`Amherst.GraphQL.Infrastructure`)

### Settings

**`PostgresSettings`** (namespace `Amherst.GraphQL.Infrastructure.Postgres`) — strongly-typed
config POCO with `SectionName = "Postgres"` constant and a `ConnectionString` property.
Used with `configuration.GetSection(...).Get<PostgresSettings>()`.

### DbContext

**`GeoDbContext`** — single `DbSet<Geo>` property named `Geos`. Primary constructor takes
`DbContextOptions<GeoDbContext>`. `OnModelCreating` calls `ApplyConfiguration` with one
configuration class.

### EF Configuration

**`GeoConfiguration`** — `IEntityTypeConfiguration<Geo>`. Maps to table `geo_shapes`.
Composite key on `(GeoTypeCode, GeoValue)`. Every property gets an explicit
`HasColumnName(...)` mapping to its snake_case column name. The `geo_polygon` column is
not mapped — a comment explains that spatial queries use `FromSql` instead.

### Repository

**`GeoRepository`** — implements `IGeoRepository` via primary constructor injection of
`GeoDbContext`.

- `Query` uses standard LINQ `Where` to filter by both key columns.
- `QueryContaining` uses **`FromSql(FormattableString)`** (the `$"..."` interpolated overload,
  not `FromSqlRaw`) with a raw PostGIS query. EF Core automatically parameterizes the
  interpolated values as `$1`/`$2`, preventing SQL injection.
  - The SELECT list in the raw SQL must explicitly name all seven mapped columns.
  - Spatial predicate: `ST_Covers(geo_polygon, ST_SetSRID(ST_MakePoint({longitude}, {latitude}), 4326)::geography)`
  - The `::geography` cast on the point expression is required to match the column's type.
  - **`ST_MakePoint` takes X (longitude) first, then Y (latitude)** — the parameter order
    in the method signature is `(latitude, longitude)` but the SQL call reverses them.

### Dependency Injection

**`DependencyInjection.cs`** — static extension method `AddInfrastructure(IConfiguration)`.

1. Reads connection string via `PostgresSettings`.
2. Registers `AddPooledDbContextFactory<GeoDbContext>` with `UseNpgsql` and
   `UseQueryTrackingBehavior(NoTracking)`.
3. Registers a scoped `GeoDbContext` resolved from `IDbContextFactory<GeoDbContext>` (needed
   for direct injection into repositories).
4. Registers `IGeoRepository` → `GeoRepository` as scoped.

---

## Api Layer (`Amherst.GraphQL.Api`)

### Query Resolvers

**`GeoQueries`** (namespace `Amherst.GraphQL.Api.Queries`) — static class decorated with
`[QueryType]`. Two static resolver methods, both injecting `IGeoRepository` via `[Service]`:

- **`GetGeos(string geoTypeCode, string geoValue, ...)`** — decorated with `[UseSorting]`.
  Delegates to `repository.Query(...)`.
- **`GetGeosContaining(double latitude, double longitude, ...)`** — decorated with
  `[UseFiltering]` and `[UseSorting]`. Delegates to `repository.QueryContaining(...)`.

### Program.cs

Top-level statements. Calls `AddInfrastructure`, then configures HC with:

```
.AddGraphQLServer()
.AddQueryType()
.AddTypeExtension(typeof(GeoQueries))
.AddFiltering()
.AddSorting()
.RegisterDbContextFactory<GeoDbContext>()
```

Maps the `/graphql` endpoint via `app.MapGraphQL()`.

### Configuration Files

`appsettings.json` — standard logging defaults plus an empty `"Postgres": { "ConnectionString": "" }` section.

`appsettings.Development.json` — same logging block, plus a populated Postgres connection string
placeholder (`Host`, `Port`, `Database`, `SearchPath`, `SSL Mode`, `Username`, `Password`).

---

## Key Gotchas

| # | Issue | Resolution |
|---|-------|-----------|
| 1 | `ST_MakePoint(x, y)` takes longitude first | Method signature is `(latitude, longitude)` but SQL call is `ST_MakePoint({longitude}, {latitude})` |
| 2 | `FromSqlRaw` vs `FromSql` | Use `FromSql($"...")` — the interpolated overload; EF parameterizes values safely |
| 3 | `FromSql` SELECT list must cover all mapped columns | Explicitly list all 7 columns; omitting any mapped column causes a runtime error |
| 4 | `geography` vs `geometry` type mismatch | Apply `::geography` cast to the constructed point so `ST_Covers` arguments are type-compatible |
| 5 | HC v15 `AddTypes()` with zero args | Ambiguous overload — always use `.AddQueryType()` + `.AddTypeExtension(typeof(...))` explicitly |
| 6 | `[QueryType]` is a runtime attribute | Works with `AddTypeExtension()`; it is not source-gen only |
| 7 | `geo_polygon` column | Never map it in EF — doing so requires NetTopologySuite and complicates the model |
| 8 | Infrastructure must not reference HC | Any HC-specific wiring (e.g. `RegisterDbContextFactory`) belongs in Api's `Program.cs` |

---

## Verification

```bash
dotnet build Amherst.GraphQL.sln   # expect: 0 errors, 0 warnings

dotnet run --project src/Amherst.GraphQL.Api
```

Open `/graphql` (HC Nitro UI) and run:

```graphql
# Exact-match lookup
{ geos(geoTypeCode: "ZIP", geoValue: "78701") { geoName wktPolygon } }

# Point-in-polygon spatial query (Austin, TX)
{ geosContaining(latitude: 30.267, longitude: -97.743) { geoTypeCode geoValue geoName } }
```
