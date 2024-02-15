# Use the official Microsoft .NET Core runtime base image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# Set the working directory
WORKDIR /app

# Copy the csproj and restore dependencies
COPY ["src/Minitwit.Web/Minitwit.Web.csproj", "Minitwit.Web/"]
RUN dotnet restore "Minitwit.Web/Minitwit.Web.csproj"

# Copy everything else and build the project
COPY . .
WORKDIR "/app/Minitwit.Web"
RUN dotnet build "Minitwit.Web.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "Minitwit.Web.csproj" -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Minitwit.Web.dll"]