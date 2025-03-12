# Use .NET SDK to build and apply migrations
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY ./infrastructure/Database ./Database
COPY *.sln .

# Navigate to Database project
WORKDIR /app/Database

# Install EF Core CLI
RUN dotnet tool install --global dotnet-ef

# Ensure the global tools path is in the environment
ENV PATH="${PATH}:/root/.dotnet/tools"

# Restore dependencies
RUN dotnet restore

# Apply migrations
CMD ["dotnet", "ef", "database", "update"]
