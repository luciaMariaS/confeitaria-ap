    FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Confeitaria.Api/Confeitaria.Api.csproj Confeitaria.Api/
RUN dotnet restore Confeitaria.Api/Confeitaria.Api.csproj
COPY Confeitaria.Api/ Confeitaria.Api/
RUN dotnet publish Confeitaria.Api/Confeitaria.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Confeitaria.Api.dll"]