# Multi-stage build for Resume Template Hub API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["EaseMyBooking.Api/EaseMyBooking.Api.csproj", "EaseMyBooking.Api/"]
RUN dotnet restore "EaseMyBooking.Api/EaseMyBooking.Api.csproj"

# Copy source and build
COPY . .
RUN dotnet build "EaseMyBooking.Api/EaseMyBooking.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "EaseMyBooking.Api/EaseMyBooking.Api.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:10000/api/health || exit 1

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000
ENTRYPOINT ["dotnet", "EaseMyBooking.Api.dll"]
