#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RedisLite/RedisLite.csproj", "RedisLite/"]
COPY ["RESP/RESP.csproj", "RESP/"]
RUN dotnet restore "./RedisLite/RedisLite.csproj"
COPY . .
WORKDIR "/src/RedisLite"
RUN dotnet build "./RedisLite.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RedisLite.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#8080 je port koji exposuje .net 8.0 i kada se pokrene kontejner on je expose-ovan, proveriti
#kako da se exposuje drugi pored njega

FROM base AS final
WORKDIR /app
EXPOSE 6379
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RedisLite.dll"]