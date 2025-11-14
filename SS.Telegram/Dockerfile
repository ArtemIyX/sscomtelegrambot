FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SS.Telegram/SS.Telegram.csproj", "SS.Telegram/"]
COPY ["SS.Data/SS.Data.csproj", "SS.Data/"]
COPY ["SS.Parser/SS.Parser.csproj", "SS.Parser/"]
RUN dotnet restore "SS.Telegram/SS.Telegram.csproj"
COPY . .
WORKDIR "/src/SS.Telegram"
RUN dotnet build "./SS.Telegram.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SS.Telegram.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SS.Telegram.dll"]
