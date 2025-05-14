FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["device-drop-backend/device-drop-backend.csproj", "device-drop-backend/"]
RUN dotnet restore "device-drop-backend/device-drop-backend.csproj"

COPY . .
WORKDIR "/src/device-drop-backend"
RUN dotnet build "device-drop-backend.csproj" -c Release -o /app/build
RUN dotnet publish "device-drop-backend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "device-drop-backend.dll"]