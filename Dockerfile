FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Minitwit.Web/Minitwit.Web.csproj", "src/Minitwit.Web/"]
COPY ["src/Minitwit.Core/Minitwit.Core.csproj", "src/Minitwit.Core/"]
COPY ["src/Minitwit.Infrastructure/Minitwit.Infrastructure.csproj", "src/Minitwit.Infrastructure/"]
RUN dotnet restore "src/Minitwit.Web/Minitwit.Web.csproj"

COPY . .

WORKDIR "/src"
RUN dotnet build "src/Minitwit.Web/Minitwit.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Minitwit.Web/Minitwit.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install wget and dockerize
RUN apt-get update \
    && apt-get install -y wget \
    && wget https://github.com/jwilder/dockerize/releases/download/v0.6.1/dockerize-linux-amd64-v0.6.1.tar.gz \
    && tar -C /usr/local/bin -xzvf dockerize-linux-amd64-v0.6.1.tar.gz \
    && rm dockerize-linux-amd64-v0.6.1.tar.gz

# Use dockerize in the ENTRYPOINT to wait for the database service
ENTRYPOINT ["dockerize", "-wait", "tcp://database:3306", "-timeout", "30s", "dotnet", "Minitwit.Web.dll"]
