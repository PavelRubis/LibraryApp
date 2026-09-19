# Frontend — Домашняя библиотека

Фронтенд на Vue 3, TypeScript, Vite и Vuetify. Типы и низкоуровневые API-клиенты в `src/api/generated` создаются из Swagger backend-приложения. Файлы в этой папке вручную не редактируются.

## Запуск

Backend по умолчанию должен быть доступен на `http://localhost:5080`.

```powershell
npm install
npm run dev
```

Vite откроет приложение на `http://localhost:5173` и проксирует `/api` в backend.

Для другого адреса API создайте `.env.local`:

```text
VITE_API_BASE_URL=https://localhost:7080
```

## Обновление клиента API

Запустите backend со Swagger на порту 5080, затем:

```powershell
npm run generate:api
```

Прикладные обёртки над сгенерированным клиентом находятся в `src/services`.

## Проверка production-сборки

```powershell
npm run build
```
