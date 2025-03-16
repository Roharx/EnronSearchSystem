# Use .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything from the context
COPY ./infrastructure/MessageQueue ./MessageQueue

# Move into the MessageQueue folder
WORKDIR /app/MessageQueue

# Restore dependencies
RUN dotnet restore

# Build and publish
RUN dotnet publish -c Release -o /out

# Use a runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Start the MessageQueue service
CMD ["dotnet", "MessageQueue.dll"]
