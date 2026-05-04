# Task Manager: Take-Home Submission

A full-stack task management app built with .NET 9 Web API and React (TypeScript).

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 9.x ([download](https://dotnet.microsoft.com/download/dotnet/9.0)) |
| Node.js | 18.x or 20 LTS ([download](https://nodejs.org)) |
| Git | any recent version ([download](https://git-scm.com)) |

**Windows quick-install** (run in PowerShell, no admin required for winget):

```powershell
winget install Microsoft.DotNet.SDK.9
winget install OpenJS.NodeJS.LTS
winget install --id Git.Git -e --source winget
```

> Restart your PowerShell window after each install so the updated `PATH` takes effect before verifying.

Verify:

```powershell
dotnet --version   # 9.x.x
node --version     # v18.x or v20.x
npm --version
git --version
```

**Windows: allow npm scripts to run** (one-time, if `npm run dev` is blocked):

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## Quick Start

### 1. Clone

```powershell
git clone https://github.com/HasanGulenc/ToDoTracker.git
cd ToDoTracker
```

### 2. Configure the backend JWT secret

The backend requires a `Jwt:Secret` (≥ 32 characters) that is **not** committed to the repo. Create the file:

```bash
# backend/appsettings.Development.json
```

```json
{
  "Jwt": {
    "Secret": "your-secret-key-at-least-32-characters-long"
  }
}
```

> In production, set the environment variable `Jwt__Secret` instead.

### 3. Run the backend

```powershell
cd backend
dotnet restore
dotnet run --urls http://localhost:5000
```

> **NuGet restore fails?** Fresh Windows machines sometimes ship without the default NuGet feed configured. Fix it once:
> ```powershell
> dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
> ```
> Then re-run `dotnet restore`.

On first run, EF Core migrations apply automatically and create `tasks.db` (SQLite). The API is ready when you see:

```
Now listening on: http://localhost:5000
```

### 4. Run the frontend

Open a second terminal:

```powershell
cd frontend
npm install
```

Create the local env file:

```
# frontend/.env.local
VITE_API_URL=http://localhost:5000
```

```powershell
npm run dev
```

Open **http://localhost:5173** in your browser.

---

## Running the tests

### Backend (xUnit, 26 integration tests)

```bash
dotnet test ToDoTracker.sln
```

Tests use a real SQLite file per test class (not in-memory) and spin up a full `WebApplicationFactory<Program>`.

### Frontend (Vitest, 7 unit tests)

```bash
cd frontend
npm run test:run
```

Tests cover: login success/failure, register, task form create/error state.

---

## API Overview

All task endpoints require a `Bearer` token from `/api/auth/login` or `/api/auth/register`.

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/register` | Register → returns JWT |
| POST | `/api/auth/login` | Login → returns JWT |
| GET | `/api/tasks` | List tasks (filter, sort, paginate) |
| POST | `/api/tasks` | Create task |
| GET | `/api/tasks/{id}` | Get task by ID |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Delete task |
| GET | `/api/tasks/due-today` | Tasks due today (UTC date) |

Query parameters for `GET /api/tasks`:

| Param | Values |
|-------|--------|
| `status` | `Todo`, `InProgress`, `Done` |
| `priority` | `Low`, `Medium`, `High` |
| `sortBy` | `dueDate`, `priority`, `createdAt` |
| `sortDir` | `asc`, `desc` |
| `page` | integer ≥ 1 (default: 1) |
| `pageSize` | 1-100 (default: 20) |

---

## Architecture

### Backend

**Single .NET 9 project** with feature folders (`Auth/`, `Tasks/`, `Data/`, `Common/`). No Repository pattern, no Clean Architecture layers. At this scale, EF Core's `DbContext` is already a unit-of-work abstraction and adding more layers would be over-engineering.

Controllers call `DbContext` directly via three layers:

```
Controller → DbContext → SQLite
```

- **Auth**: JWT bearer tokens, 7-day expiry. All task queries filter by `userId` from the JWT `NameIdentifier` claim so cross-user data leaks are impossible.
- **Dates**: `DateOnly` for due dates (no timezone fragility). `DateTime` fields stored as UTC.
- **Errors**: Global `ExceptionMiddleware` maps `NotFoundException` → 404, `ConflictException` → 409.
- **Validation**: Data Annotations on all DTOs. Returns RFC 7807 problem details on 400.
- **CORS**: Locked to a single origin via `Cors:AllowedOrigin` config, not allow-all.

### Frontend

**React 18 + TypeScript + Vite 4.** No state management library. React context is sufficient for auth at this scale.

- JWT stored in **`sessionStorage`** (not `localStorage`), scoped to the browser tab.
- API client is hand-typed to match backend DTOs exactly (`src/api/types.ts`).
- `VITE_API_URL` read from `.env.local`, never hardcoded.
- Create/edit form stays open and shows the server error message on failure, so users can correct and retry.

### Testing rationale

Backend tests use a real SQLite file (not EF in-memory) because in-memory databases mask real schema constraints and migration bugs. Frontend tests mock the API layer and assert on rendered output and user interactions.
