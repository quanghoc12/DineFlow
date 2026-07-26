FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DineFlow.sln ./
COPY src/DineFlow.BusinessObjects/DineFlow.BusinessObjects.csproj src/DineFlow.BusinessObjects/
COPY src/DineFlow.DataAccessObjects/DineFlow.DataAccessObjects.csproj src/DineFlow.DataAccessObjects/
COPY src/DineFlow.Repositories/DineFlow.Repositories.csproj src/DineFlow.Repositories/
COPY src/DineFlow.Services/DineFlow.Services.csproj src/DineFlow.Services/
COPY src/DineFlow.Api/DineFlow.Api.csproj src/DineFlow.Api/
RUN dotnet restore src/DineFlow.Api/DineFlow.Api.csproj

COPY src/ src/
RUN dotnet publish src/DineFlow.Api/DineFlow.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DineFlow.Api.dll"]
