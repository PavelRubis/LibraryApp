# LibraryApp

Приложение для ведения домашней библитеки. Приложение использует .NET 10, Dapper, MediatR, FluentMigrator, Microsoft SQL Server, NGINX, Vie3, Vuetify.

## Быстрый запуск через Docker Compose

Для запуска нужны Git, Docker Engine и Docker Compose. Из чистого окружения приложение разворачивается тремя командами:

```bash
git clone https://github.com/PavelRubis/LibraryApp.git

cd LibraryApp

docker compose up
```

При первом запуске Compose соберёт frontend и backend, дождётся готовности SQL Server, создаст базу `LibraryApp` и применит миграции.

После запуска доступны:

- frontend: <http://localhost:8080>;
- SQL Server: `localhost:1433`.

Backend не публикует порт на хосте и доступен только внутри Docker-сети. Frontend проксирует запросы `/api` в backend через Nginx.

Стандартные значения можно переопределить переменными окружения до запуска Compose:

| Переменная | Значение по умолчанию | Назначение |
| --- | --- | --- |
| `FRONTEND_PORT` | `8080` | Порт frontend на хосте |
| `SQL_PORT` | `1433` | Порт SQL Server на хосте |
| `MSSQL_SA_PASSWORD` | `LibraryApp_Strong_Passw0rd!` | Локальный пароль пользователя `sa` |

Логи backend записываются в именованный Docker volume `backend-logs`, а данные SQL Server — в `sql-data`. Их содержимое сохраняется после обычной остановки (`docker compose down`)

Просмотреть файлы логов можно внутри backend-контейнера:

```bash
docker compose exec backend ls -la /app/logs
docker compose exec backend sh -c 'tail -f /app/logs/logs*.log'
```
