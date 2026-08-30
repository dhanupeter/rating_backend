FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Rating.API/Rating.API.csproj", "Rating.API/"]
RUN dotnet restore "Rating.API/Rating.API.csproj"

COPY . .
WORKDIR "/src/Rating.API"

RUN dotnet build "Rating.API.csproj" -c Release -o /app/build
RUN dotnet publish "Rating.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Rating.API.dll"]
