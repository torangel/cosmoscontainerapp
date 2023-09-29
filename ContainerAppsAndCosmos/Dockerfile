FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ContainerAppsAndCosmos/ContainerAppsAndCosmos.csproj", "ContainerAppsAndCosmos/"]
RUN dotnet restore "ContainerAppsAndCosmos/ContainerAppsAndCosmos.csproj"
COPY . .
WORKDIR "/src/ContainerAppsAndCosmos"
RUN dotnet build "ContainerAppsAndCosmos.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContainerAppsAndCosmos.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContainerAppsAndCosmos.dll"]
