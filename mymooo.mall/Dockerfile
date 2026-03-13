#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["server/com.mymooo.mall/", "com.mymooo.mall/"]
RUN dotnet nuget add source https://nuget.mymooo.com/nuget -n mymooo
RUN dotnet restore "com.mymooo.mall/com.mymooo.mall.sln"
COPY . .
WORKDIR "/src/com.mymooo.mall/com.mymooo.mall"
RUN dotnet build "com.mymooo.mall.csproj" -c Release  -o /app/build

FROM build AS publish
RUN dotnet publish "com.mymooo.mall.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "com.mymooo.mall.dll"]