# Use .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything from the context (to ensure dependencies exist)
COPY ./src ./src
COPY *.sln .

# Restore dependencies
WORKDIR /app/src/CleanerService
RUN dotnet restore

# Build and publish
RUN dotnet publish -c Release -o /out

# Use runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
CMD ["dotnet", "CleanerService.dll"]
