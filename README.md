# HERMES — Help Ticketing System

> *What are tickets and their responses if not messages at their core?*

Hermes is a web-based help-desk application built as a group project. The name comes from the Greek messenger god — a fitting patron for a system whose entire purpose is routing messages between the people who need help and the people who can provide it.

---

## Project Background

The brief was to develop a help-system application from scratch within 8 business days, as a team of three.

One early challenge was aligning the team around a single tech stack: each member brought a different level of familiarity with .NET. After discussing past exposure to various languages and frameworks, the group chose **ASP.NET Core with Blazor Server**.

The appeal was practical — Blazor Server keeps the frontend and backend in a single C# project with no separate API layer to maintain. The server handles all the rendering and stays connected to the browser in real time, so there was no need to build a separate API — a good fit for the tight timeframe.

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

- **Scrum methodology** — daily stand-ups and short sprint cycles to keep the 8-day timeline on track.
- **Trello kanban board** for tracking tasks and priorities across the team.
- **Figma prototyping** — one day spent sketching login, registration, and dashboard screens; this is also when the team landed on the name *HERMES*.
- **Branch-per-feature + pull requests** on GitHub for code integration and review.
- The team had varying levels of experience with Git and version control; the branch-and-PR workflow was a learning process as much as a development practice.

---

## License

This project was developed as a training exercise during the **Specialisterne 2026** program and is not currently published under an open-source license.
