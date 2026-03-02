# 📋 TeamFlow API — Project Plan

> Multi-Tenant SaaS REST API built with .NET Clean Architecture, Docker, and automated testing.

---

## 🏗️ Architecture Overview

```
TeamFlow.API          → Controllers, Middleware, DI setup
TeamFlow.Application  → Use Cases, DTOs, Interfaces, Validators
TeamFlow.Domain       → Entities, Enums, Domain Events
TeamFlow.Infrastructure → EF Core, Repos, Redis, Jobs, Email
TeamFlow.Tests        → Unit + Integration tests
```

**Stack:** .NET 8 · PostgreSQL · Redis · Docker · xUnit · JWT

---

## 🗂️ Phases

---

### ✅ V1 — Core Foundation

**Goal:** Working multi-tenant API with auth, roles, and task management.

| # | Task | Notes |
|---|------|-------|
| 1 | Project scaffold | Clean arch folder structure, solution file |
| 2 | Domain entities | `Organization`, `User`, `OrgUser`, `TaskItem` |
| 3 | EF Core + PostgreSQL | DbContext, migrations, seeding |
| 4 | JWT Authentication | Register, Login, refresh token (optional) |
| 5 | Register Organization | Creates org + sets requesting user as Admin |
| 6 | Invite Users to Org | Token-based invite, accept endpoint |
| 7 | Role-based access | `Admin` / `Member` via policy + claims |
| 8 | Task CRUD | Create, Read, Update, Delete — scoped to org |
| 9 | Tenant isolation | All queries filtered by `OrganizationId` |
| 10 | 30+ automated tests | Unit (use cases) + Integration (API endpoints) |
| 11 | Dockerize | `Dockerfile` for API, basic `docker-compose` |

**Exit criteria:** All endpoints secured, tenant data isolated, tests passing, Docker runs locally.

---

### ⚡ V2 — Performance

**Goal:** Make the API fast and production-query-ready.

| # | Task | Notes |
|---|------|-------|
| 1 | Redis caching | Cache task list queries per org, invalidate on write |
| 2 | Pagination | `?page=1&pageSize=20` on all list endpoints |
| 3 | Filtering | Filter tasks by status, assignee, date range |
| 4 | DB indexing | Index `OrganizationId`, `UserId`, `Status` columns |
| 5 | Async everywhere | Audit all DB calls — no `.Result` or `.Wait()` |
| 6 | Benchmark endpoint | Use BenchmarkDotNet on `GET /tasks` — document results |

**Exit criteria:** Task list endpoint uses cache, pagination works, benchmark report committed.

---

### 🔁 V3 — Scalability

**Goal:** Decouple side effects and add observability.

| # | Task | Notes |
|---|------|-------|
| 1 | Background job — email | Hangfire or `IHostedService` to simulate invite emails |
| 2 | Domain events | `TaskCreatedEvent` published on task creation |
| 3 | Event handler | Log event / simulate downstream consumer |
| 4 | Rate limiting | Per-IP and per-user throttling via ASP.NET middleware |
| 5 | Structured logging | Serilog with `RequestId`, `OrgId`, `UserId` context |
| 6 | Log sinks | Console (dev) + File (prod-ready) |

**Exit criteria:** Invite triggers background job, task creation fires event, all logs are structured JSON.

---

### 🚀 V4 — Production Polish

**Goal:** Ready to demo, deploy, or hand off.

| # | Task | Notes |
|---|------|-------|
| 1 | Health check | `GET /health` — checks DB + Redis connectivity |
| 2 | Docker Compose | API + PostgreSQL + Redis in one `docker-compose.yml` |
| 3 | Environment configs | `appsettings.Development.json` / `.Production.json` + `.env` |
| 4 | Error handling middleware | Global handler → consistent `ProblemDetails` responses |
| 5 | CI/CD pipeline | GitHub Actions: build → test → Docker build on push |

**Exit criteria:** `docker-compose up` starts full stack, CI passes on every PR, no unhandled exceptions leak stack traces.

---

## 📅 Suggested Timeline

| Phase | Scope | Est. Time |
|-------|-------|-----------|
| V1 | Core API + tests + Docker | 1–2 weeks |
| V2 | Caching + performance | 3–5 days |
| V3 | Events + logging + rate limiting | 3–5 days |
| V4 | Polish + CI/CD | 2–3 days |

---

## 🔑 Key Decisions

- **Tenant isolation:** `OrganizationId` on every entity, enforced at the repository layer — not the controller.
- **Auth flow:** JWT issued on login, org membership validated per request via custom middleware/policy.
- **Test strategy:** Unit tests for Application layer (mocked repos), Integration tests for API routes using `WebApplicationFactory`.
- **No over-engineering:** No message broker (Kafka/RabbitMQ) in V3 — event handling is in-process to keep infra simple.
