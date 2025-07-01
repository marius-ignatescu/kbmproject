# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY KBMHttpService/ ./KBMHttpService/
COPY KBMGrpcService/Protos/ ./KBMGrpcService/Protos/
WORKDIR /src/KBMHttpService
RUN dotnet restore "KBMHttpService.csproj"
RUN dotnet build "KBMHttpService.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "KBMHttpService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KBMHttpService.dll"]