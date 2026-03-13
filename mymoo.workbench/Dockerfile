
FROM inedo.mymooo.com/docker/dotnet-aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_HTTP_PORTS 80

FROM inedo.mymooo.com/docker/dotnet-sdk:8.0 AS build
WORKDIR /src
ENV NUGET_XMLDOC_MODE none
COPY ["src/com.mymooo.workbench/", "com.mymooo.workbench/"]
RUN dotnet nuget add source https://inedo.mymooo.com/nuget/mymooo/v3/index.json -n mymooo -u mymooo -p mymooo.com --store-password-in-clear-text
RUN dotnet nuget remove source nuget.org
RUN dotnet restore "com.mymooo.workbench/com.mymooo.workbench.sln"
COPY . .
WORKDIR "/src/com.mymooo.workbench/com.mymooo.workbench"
RUN dotnet build "com.mymooo.workbench.csproj" -c Release  -o /app/build

FROM build AS publish
RUN dotnet publish "com.mymooo.workbench.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
#RUN dotnet publish --framework netcoreapp3.1 --configuration Release -o /app/publish
COPY --from=publish /app/publish .
COPY ./client/workbench/dist /app/wwwroot/html5
ENTRYPOINT ["dotnet", "com.mymooo.workbench.dll"]
