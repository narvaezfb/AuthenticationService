# Use the official .NET SDK image for .NET 7 as the base image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the .NET project files and restore dependencies
COPY ./*.csproj ./
RUN dotnet restore

# Copy the application source code
COPY . ./

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Use the runtime image for arm64 architecture
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Set the runtime environment variables if needed
#ENV ASPNETCORE_URLS=http://+:80

# Expose the port your app listens on
EXPOSE 80

# Command to run the application
ENTRYPOINT ["dotnet", "Authentication Service.dll"]
