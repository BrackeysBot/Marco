FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Marco/Marco.csproj", "Marco/"]
RUN dotnet restore "Marco/Marco.csproj"
COPY . .
WORKDIR "/src/Marco"
RUN dotnet build "Marco.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Marco.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Marco.dll"]
