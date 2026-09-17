# HERMES — Help Ticketing System

> *What are tickets and their responses if not messages at their core?*

Hermes is a web-based help-desk application built as a group project. The name comes from the Greek messenger god — a fitting patron for a system whose entire purpose is routing messages between the people who need help and the people who can provide it.

---

## Project Background

The brief was to develop a help-system application from scratch following **Scrum methodology** — daily stand-ups, short sprint cycles, and a Trello kanban board to track progress.

One early challenge was aligning the team around a single tech stack: each member brought a different level of familiarity with .NET. After discussing past exposure to various languages and frameworks, the group chose **ASP.NET Core with Blazor Server**. The appeal was practical — Blazor Server keeps the frontend and backend in a single project with no separate API layer to maintain. The UI is rendered on the server and kept in sync with the browser over a real-time SignalR connection, which removed the overhead of building and consuming REST endpoints given the scope of work and the timeframe available (8 business days).

The team spent one day prototyping rough ideas in **Figma** — login and registration screens, a first take on the dashboard layout, and the general visual direction. Several of those early concepts carried through into the final implementation. It was also during this prototyping phase that the team brainstormed names for the app, landing on **HERMES**.

---

## Architecture & Tech Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | ASP.NET Core 10 (Blazor Server — Interactive Server rendering) |
| **Language** | C# 13 |
| **Authentication** | ASP.NET Core Identity (cookie-based, role support) |
| **Database** | SQLite via Entity Framework Core 10 (code-first migrations) |
| **Styling** | Custom CSS with CSS variables and scoped component styles |
| **Source control** | Git + GitHub (branch-per-feature, pull-request workflow) |




---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

Verify the SDK is installed:

```bash
dotnet --version   # should print 10.x.xxx
```

### Clone and run

```bash
git clone https://github.com/Specialisterne2026HelpSystemG2/hermes.git
cd hermes/Hermes/Hermes
dotnet run
```

The app will:
1. Apply any pending EF Core migrations automatically.
2. Seed the admin user, default categories, and demo tickets (on a fresh database).
3. Start listening on `http://localhost:5180` (or the port shown in the console).

### Default admin login

| Field | Value |
|-------|-------|
| Email | `admin@hermes.local` |
| Password | `Admin123!` |

### Resetting the local database

If seed data in `SeedData.cs` has changed (check the PR description), delete the local SQLite file and restart:

```bash
rm -f HermesContext.db HermesContext.db-shm HermesContext.db-wal
dotnet run
```

**Windows (PowerShell):**

```powershell
Remove-Item -Force HermesContext.db, HermesContext.db-shm, HermesContext.db-wal -ErrorAction SilentlyContinue
dotnet run
```

### EF Core tools (for creating migrations)

```bash
dotnet tool install --global dotnet-ef
```

---

## Team Workflow

- WIP

---

## License

This project was developed as a training exercise and is not currently published under an open-source license.
