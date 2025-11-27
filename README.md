# Incident Management System

Система генерации и обработки событий с созданием инцидентов.

## Запуск

### EventProcessor (должен запускаться первым)
```bash
cd EventProcessor
dotnet run
```
URL: http://localhost:5001
Swagger: http://localhost:5001/swagger

### EventGenerator
```bash
cd EventGenerator
dotnet run
```
URL: http://localhost:5000
Swagger: http://localhost:5000/swagger

## Описание

Два сервиса на ASP.NET Core 6:
- **EventGenerator** - генерирует события автоматически (каждые 0-2 сек) и по REST API
- **EventProcessor** - обрабатывает события по 3 шаблонам и сохраняет инциденты в SQLite

### Шаблоны:
1. Event.Type=1 → Incident.Type=1
2. Event.Type=2 + Event.Type=1 (≤20сек) → Incident.Type=2
3. Event.Type=3 + создание Incident.Type=2 (≤60сек) → Incident.Type=3