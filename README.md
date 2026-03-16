# Real-Time Financial Monitor

A full-stack MVP for monitoring financial transactions in real time.  
**Backend:** .NET 8 · SignalR · SQLite · Redis  
**Frontend:** React 19 · TypeScript · Redux Toolkit · Vite

---

## Architecture Overview

```
┌─────────────┐   HTTP POST    ┌──────────────────┐   SignalR   ┌──────────────┐
│  /add page  │ ─────────────► │  .NET 8 Backend  │ ──────────► │ /monitor page│
│ (Simulator) │                │  (port 8080)     │             │ (Dashboard)  │
└─────────────┘                └────────┬─────────┘             └──────────────┘
                                        │
                              ┌─────────▼─────────┐
                              │  SQLite + Redis    │
                              │  (cache / pub-sub) │
                              └───────────────────┘
```

---

## Running the System

### Option 1 — Docker Compose (recommended)

**Prerequisites:** Docker Desktop running.

```bash
docker-compose up --build
```

| Service  | URL                          |
|----------|------------------------------|
| Frontend | http://localhost:3001        |
| Backend  | http://localhost:8080        |
| Redis    | localhost:6379               |

To stop:

```bash
docker-compose down
```

---

### Option 2 — Local Development (without Docker)

**Prerequisites:** .NET 8 SDK, Node.js 18+

> Redis is **not required** locally. `appsettings.Development.json` already sets `"Provider": "InMemory"`, so the backend uses in-memory cache and SignalR by default.

#### 1. Start the Backend

```bash
dotnet run
```

Backend listens on `http://localhost:5210`.

#### 2. Start the Frontend

```bash
cd client
npm install
npm run dev
```

Frontend runs on `http://localhost:5173`.

---

## Using the App

| Route      | Purpose                                                     |
|------------|-------------------------------------------------------------|
| `/add`     | Generate mock transactions and POST them to the backend     |
| `/monitor` | Live dashboard — new transactions appear in real time       |

- Transactions are color-coded by status (`Completed` / `Failed` / `Pending`).
- Use the filter controls on `/monitor` to show only specific statuses.

---

## Distributed Architecture Note

When deployed to multiple pods, SignalR clients connected to different pods won't receive each other's broadcasts by default.  
This is solved by configuring SignalR's **Redis backplane** (`AddStackExchangeRedis`), which routes all hub messages through a central Redis pub/sub channel so every pod receives every broadcast regardless of which pod accepted the original HTTP POST.

Set `"Provider": "Redis"` in `appsettings.json` and ensure the connection string points to a shared Redis instance (or Redis Cluster / Azure Cache for Redis in production).

