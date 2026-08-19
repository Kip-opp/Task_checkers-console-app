# Frontend

The browser client for Task & Games Hub is a React 19 and Vite application.

Use the repository-level [README](../README.md) for the complete .NET-first setup, database migration, API configuration, and application run instructions.

Frontend-only commands from the repository root:

```powershell
npm --prefix hub-frontend ci
npm --prefix hub-frontend run dev
npm --prefix hub-frontend run lint
npm --prefix hub-frontend run build
```

Set `VITE_API_BASE_URL` when the API is not running at the default development URL:

```powershell
$env:VITE_API_BASE_URL = "http://localhost:5086/api"
```
