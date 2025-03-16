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

# Copy built application
COPY --from=build /out .

# Ensure maildir exists in container
RUN mkdir -p /app/maildir && chmod -R 777 /app/maildir

# Start the service with an entrypoint to ensure maildir exists at runtime
ENTRYPOINT ["/bin/sh", "-c", "mkdir -p /app/maildir && chmod -R 777 /app/maildir && exec dotnet CleanerService.dll"]
