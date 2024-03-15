FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

COPY ["src/Minitwit.Web/Minitwit.Web.csproj", "src/Minitwit.Web/"]
COPY ["src/Minitwit.Core/Minitwit.Core.csproj", "src/Minitwit.Core/"]
COPY ["src/Minitwit.Infrastructure/Minitwit.Infrastructure.csproj", "src/Minitwit.Infrastructure/"]
RUN dotnet restore "src/Minitwit.Web/Minitwit.Web.csproj"

COPY . .

RUN dotnet build "src/Minitwit.Web/Minitwit.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Minitwit.Web/Minitwit.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production

# Use dockerize in the ENTRYPOINT to wait for the database service
ENTRYPOINT ["dotnet", "Minitwit.Web.dll"]
