# LibraryApp

Приложение для ведения домашней библитеки. Приложение использует .NET 10, Dapper, MediatR, FluentMigrator и Microsoft SQL Server.

## Запуск SQL Server

```powershell
docker run -d --name libraryapp-sql `
  -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=LibraryApp_Strong_Passw0rd!" `
  -p 1433:1433 `
  -v libraryapp-sql-data:/var/opt/mssql `
  --restart unless-stopped `
  mcr.microsoft.com/mssql/server:2022-latest
```

После запуска контейнера создайте базу данных:

```powershell
docker exec libraryapp-sql /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P "LibraryApp_Strong_Passw0rd!" -C `
  -Q "IF DB_ID('LibraryApp') IS NULL CREATE DATABASE LibraryApp"
```

Пароль выше предназначен только для локальной разработки.

## Конфигурация

Строка подключения задаётся через `ConnectionStrings__DefaultConnection` либо user secrets:

```powershell
dotnet user-secrets set --project WebAPI "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=LibraryApp;User Id=sa;Password=LibraryApp_Strong_Passw0rd!;TrustServerCertificate=True"
```

Автоматическое применение миграций управляется параметром `Database:RunMigrationsOnStartup`.

## Миграции вручную

```powershell
dotnet tool restore
dotnet build Infrastructure/Infrastructure.csproj
dotnet fm migrate -p SqlServer -c "Server=localhost,1433;Database=LibraryApp;User Id=sa;Password=LibraryApp_Strong_Passw0rd!;TrustServerCertificate=True" -a Infrastructure/bin/Debug/net10.0/Infrastructure.dll
```

## Запуск

```powershell
dotnet run --project WebAPI
```

Swagger UI доступен по адресу, указанному в консоли, по пути `/swagger`.

## Тесты

```powershell
dotnet test
```

Интеграционные тесты используют Testcontainers и требуют запущенный Docker Engine.

