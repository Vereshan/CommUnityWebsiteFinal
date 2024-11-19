# Use the official .NET image from Docker Hub
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CommUnityWeb/CommUnityWeb.csproj", "CommUnityWeb/"]
RUN dotnet restore "CommUnityWeb/CommUnityWeb.csproj"
COPY . .
WORKDIR "/src/CommUnityWeb"
RUN dotnet publish "CommUnityWeb.csproj" -c Release -o /app/publish

# Copy the build output to the base image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommUnityWeb.dll"]
