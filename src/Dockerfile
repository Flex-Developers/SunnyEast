FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем только csproj файлы
COPY ["Client/Client/Client.csproj", "Client/Client/"]
COPY ["Client/Client.Infrastructure/Client.Infrastructure.csproj", "Client/Client.Infrastructure/"]
COPY ["Application.Contract/Application.Contract.csproj", "Application.Contract/"]

# Restore для всего solution
RUN dotnet restore "Client/Client/Client.csproj"

# Копируем код ПОСЛЕ restore
COPY . .

# Build и publish БЕЗ --no-restore (пусть еще раз проверит)
WORKDIR "/src/Client/Client"
RUN dotnet publish "Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80