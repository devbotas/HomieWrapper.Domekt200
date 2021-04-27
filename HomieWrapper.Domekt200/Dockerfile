#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-amd64 AS build
WORKDIR /src
COPY ["homie-wrapper-domekt200/homie-wrapper-domekt200.csproj", "homie-wrapper-domekt200/"]
RUN dotnet restore "homie-wrapper-domekt200/homie-wrapper-domekt200.csproj"
COPY . .
WORKDIR "/src/homie-wrapper-domekt200"
RUN dotnet build "homie-wrapper-domekt200.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "homie-wrapper-domekt200.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "homie-wrapper-domekt200.dll"]