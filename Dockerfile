FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY BookQuotes.csproj ./
RUN dotnet restore --nologo

COPY . ./
RUN dotnet publish BookQuotes.csproj -c Release -o /app/publish --no-restore

FROM nginxinc/nginx-unprivileged:1.29-alpine AS runtime
WORKDIR /usr/share/nginx/html

COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot ./

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/ || exit 1

