# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# restore
COPY ["RecipeBudgetService.csproj", "."]
RUN dotnet restore "RecipeBudgetService.csproj"

# build
COPY . .
RUN dotnet build "RecipeBudgetService.csproj" -c Release -o /app/build


# Stage 2: Publish Stage
FROM build AS publish
RUN dotnet publish "RecipeBudgetService.csproj" -c Release -o /app/publish

# Stage 3: Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
ENV ASPNETCORE_HTTP_PORTS=5001
EXPOSE 5001
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RecipeBudgetService.dll"]