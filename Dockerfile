FROM microsoft/aspnetcore:1.1 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS http://*:${PORT}

FROM microsoft/aspnetcore-build:1.1 AS build
WORKDIR /src
COPY *.sln ./
COPY rpkp/rpkp.csproj rpkp/
RUN dotnet restore
COPY . .
WORKDIR /src/rpkp
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
CMD ["dotnet", "rpkp.dll"]
