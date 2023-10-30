FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Simbir.GO/Simbir.GO.csproj", "Simbir.GO/"]
RUN dotnet restore "Simbir.GO/Simbir.GO.csproj"
COPY . .
WORKDIR "/src/Simbir.GO"
RUN dotnet build "Simbir.GO.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Simbir.GO.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Simbir.GO.dll"]
