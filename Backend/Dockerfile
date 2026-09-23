FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["TaskManagement.API/TaskManagement.API.csproj", "TaskManagement.API/"]
RUN dotnet restore "TaskManagement.API/TaskManagement.API.csproj"

# Copy source and build
COPY . .
WORKDIR "/src/TaskManagement.API"
RUN dotnet publish "TaskManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
