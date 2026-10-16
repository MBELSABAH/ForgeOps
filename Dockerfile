FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/ForgeOps.Web/ForgeOps.Web.csproj src/ForgeOps.Web/
RUN dotnet restore src/ForgeOps.Web/ForgeOps.Web.csproj
COPY src/ForgeOps.Web/ src/ForgeOps.Web/
RUN dotnet publish src/ForgeOps.Web/ForgeOps.Web.csproj -c Release --no-restore -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /out .
RUN mkdir -p /app/App_Data && chown -R app:app /app/App_Data
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "ForgeOps.Web.dll"]
