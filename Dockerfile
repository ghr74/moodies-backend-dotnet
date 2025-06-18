FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y \
    curl \
    wget
USER $APP_UID
WORKDIR /app
EXPOSE 4317
EXPOSE 4318

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["moodies-backend.csproj", "./"]
RUN dotnet restore "moodies-backend.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./moodies-backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./moodies-backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:4317;http://0.0.0.0:4318

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 CMD curl --fail http://localhost:4318/healthc || exit

ENTRYPOINT ["dotnet", "moodies-backend.dll"]
