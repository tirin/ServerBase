#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ServerBase/ServerBase.csproj", "ServerBase/"]
RUN dotnet restore "ServerBase/ServerBase.csproj"
COPY . .
WORKDIR "/src/ServerBase"
RUN dotnet build "ServerBase.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServerBase.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServerBase.dll"]