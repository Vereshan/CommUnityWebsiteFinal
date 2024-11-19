# Use the official .NET image from Microsoft as the base image for the runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["CommUnityWeb/CommUnityWeb.csproj", "CommUnityWeb/"]
RUN dotnet restore "CommUnityWeb/CommUnityWeb.csproj"

# Copy the rest of the application code
COPY . .

# Build the application
WORKDIR "/src/CommUnityWeb"
RUN dotnet publish "CommUnityWeb.csproj" -c Release -o /app/publish

# Define the final image that will run the application
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommUnityWeb.dll"]
