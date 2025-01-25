# Use the .NET 9.0 SDK to build the project
# Use azure linux image for minimal footprint
# https://github.com/dotnet/dotnet-docker/blob/main/documentation/azurelinux.md
FROM mcr.microsoft.com/dotnet/sdk:9.0-azurelinux3.0 AS build-env
WORKDIR /app

# Copy the entire project and build the release artifact
COPY . ./
RUN dotnet publish -c Release -o out

# Use the ASP.NET Core runtime image to run the published project
FROM mcr.microsoft.com/dotnet/aspnet:9.0-azurelinux3.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port that your application listens on
EXPOSE 5000

# Set the entry point for your application
ENTRYPOINT ["dotnet", "Web.dll"]