FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DotnetRagApi.Api/DotnetRagApi.Api.csproj DotnetRagApi.Api/
COPY DotnetRagApi.Application/DotnetRagApi.Application.csproj DotnetRagApi.Application/
COPY DotnetRagApi.Domain/DotnetRagApi.Domain.csproj DotnetRagApi.Domain/
COPY DotnetRagApi.Infrastructure/DotnetRagApi.Infrastructure.csproj DotnetRagApi.Infrastructure/
RUN dotnet restore DotnetRagApi.Api/DotnetRagApi.Api.csproj

COPY . .
RUN dotnet publish DotnetRagApi.Api/DotnetRagApi.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DisableHttpsRedirection=true
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotnetRagApi.Api.dll"]
