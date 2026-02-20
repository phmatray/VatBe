FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy manifests for restore
COPY global.json Directory.Packages.props Directory.Build.props ./
COPY src/VatBe/VatBe.csproj src/VatBe/
COPY web/VatBe.Web/VatBe.Web.csproj web/VatBe.Web/

RUN dotnet restore web/VatBe.Web/VatBe.Web.csproj

# Copy source
COPY src/ src/
COPY web/ web/

RUN dotnet publish web/VatBe.Web/VatBe.Web.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "VatBe.Web.dll"]
