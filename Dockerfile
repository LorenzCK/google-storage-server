# ---
# First stage (build)
# ---
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy solution as distinct layer
COPY GoogleStorageServer/GoogleStorageServer.sln .
COPY GoogleStorageServer/GoogleStorageServer.csproj .
RUN dotnet restore

# Copy everything else and build
COPY GoogleStorageServer/. ./
RUN dotnet publish -c Release -o out

# ---
# Second stage (execution)
# ---
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
LABEL maintainer="Lorenz Cuno Klopfenstein <lck@klopfenstein.net>"
ENV TZ=UTC

WORKDIR /app
COPY --from=build /app/out ./

# Fix console logging
ENV Logging__Console__FormatterName=

# Run on localhost:8779
ENV ASPNETCORE_URLS http://+:8779
EXPOSE 8779

# Drop privileges
USER 1000

ENTRYPOINT ["dotnet", "GoogleStorageServer.dll"]
