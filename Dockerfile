FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY FaerieTables/*.sln ./FaerieTables/
COPY FaerieTables/FaerieTables.Api/*.csproj ./FaerieTables/FaerieTables.Api/
COPY FaerieTables/FaerieTables.Core/*.csproj ./FaerieTables/FaerieTables.Core/
COPY FaerieTables/FaerieTables.Web/*.csproj ./FaerieTables/FaerieTables.Web/
COPY FaerieTables/FaerieTables.Api.Tests/*.csproj ./FaerieTables/FaerieTables.Api.Tests/
RUN dotnet restore FaerieTables/FaerieTables.sln
COPY FaerieTables/. ./FaerieTables/
RUN dotnet publish FaerieTables/FaerieTables.Api/FaerieTables.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "FaerieTables.Api.dll"]
