# Stage 1 — restore
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

COPY Amherst.GraphQL.sln Directory.Build.props ./
COPY src/Amherst.GraphQL.Api/Amherst.GraphQL.Api.csproj src/Amherst.GraphQL.Api/
COPY src/Amherst.GraphQL.Application/Amherst.GraphQL.Application.csproj src/Amherst.GraphQL.Application/
COPY src/Amherst.GraphQL.Domain/Amherst.GraphQL.Domain.csproj src/Amherst.GraphQL.Domain/
COPY src/Amherst.GraphQL.Infrastructure/Amherst.GraphQL.Infrastructure.csproj src/Amherst.GraphQL.Infrastructure/

RUN dotnet restore

# Stage 2 — build/publish
FROM restore AS publish
COPY src/ src/
RUN dotnet publish src/Amherst.GraphQL.Api/Amherst.GraphQL.Api.csproj -c Release -o /app --no-restore

# Stage 3 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Amherst.GraphQL.Api.dll"]
