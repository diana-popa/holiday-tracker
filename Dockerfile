FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY HolidayTracker.csproj .
RUN dotnet restore HolidayTracker.csproj
COPY . .
RUN dotnet publish HolidayTracker.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "HolidayTracker.dll"]