FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Minitwit.Web/Minitwit.Web.csproj", "/Minitwit.Web/"]
COPY ["src/Minitwit.Core/Minitwit.Core.csproj", "/Minitwit.Core/"]
COPY ["src/Minitwit.Infrastructure/Minitwit.Infrastructure.csproj", "/Minitwit.Infrastructure/"]
RUN dotnet restore "src/Minitwit.Web/Minitwit.Web.csproj"

COPY . .

WORKDIR "/src/Minitwit.Web"
RUN dotnet build "Minitwit.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Minitwit.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish appHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Minitwit.Web.dll"]