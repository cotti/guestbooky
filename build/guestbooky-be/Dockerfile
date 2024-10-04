FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app
COPY ../../src/Guestbooky/ .
COPY ../../tests/ .
RUN dotnet restore ./Guestbooky.API/

RUN dotnet build ./Guestbooky.API/Guestbooky.API.csproj -c Release -o /app/build

RUN dotnet publish ./Guestbooky.API/Guestbooky.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 5008

ENTRYPOINT ["dotnet", "Guestbooky.API.dll"]
