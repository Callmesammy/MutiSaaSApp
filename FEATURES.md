# 🧩 TeamFlow API — Feature Specifications

> Detailed breakdown of every feature across all four versions.

---

## V1 — Core Features

---

### 1. Register Organization

**Endpoint:** `POST /api/organizations`

**What it does:**
- Creates a new `Organization` record
- Automatically creates a `User` account for the requester
- Assigns the user the `Admin` role within that org
- Returns a JWT token scoped to that organization

**Request:**
```json
{
  "organizationName": "Acme Corp",
  "adminEmail": "alice@acme.com",
  "adminPassword": "StrongPass123!"
}
```

**Rules:**
- Organization name must be unique
- Email must not already exist in the system
- Password must meet minimum strength requirements

---

### 2. Invite Users to Organization

**Endpoints:**
- `POST /api/organizations/{orgId}/invites` — Admin sends invite
- `POST /api/invites/accept` — Invitee accepts with token

**What it does:**
- Admin generates a signed invite token tied to an email address
- Token is stored with expiry (e.g. 48 hours)
- Invitee hits accept endpoint with token → account created + added to org as `Member`
- Background job (V3) simulates sending the email

**Rules:**
- Only `Admin` role can send invites
- Token is single-use
- Expired tokens return `410 Gone`

---

### 3. Role-Based Access Control

**Roles:** `Admin` · `Member`

| Permission | Admin | Member |
|------------|-------|--------|
| Invite users | ✅ | ❌ |
| Remove users | ✅ | ❌ |
| Create tasks | ✅ | ✅ |
| Edit any task | ✅ | ❌ |
| Edit own tasks | ✅ | ✅ |
| Delete tasks | ✅ | ❌ |
| View all tasks | ✅ | ✅ |

**Implementation:**
- Role stored in JWT claims on login
- ASP.NET authorization policies: `[Authorize(Policy = "AdminOnly")]`
- Middleware validates org membership on every request

---

### 4. Task Management (Per Organization)

**Endpoints:**
- `GET    /api/tasks` — List tasks (org-scoped)
- `GET    /api/tasks/{id}` — Get single task
- `POST   /api/tasks` — Create task
- `PUT    /api/tasks/{id}` — Update task
- `DELETE /api/tasks/{id}` — Delete task (Admin only)

**Task entity:**
```
Id, Title, Description, Status, Priority,
AssigneeId, OrganizationId, CreatedAt, UpdatedAt
```

**Status values:** `Todo` · `InProgress` · `Done`
**Priority values:** `Low` · `Medium` · `High`

---

### 5. Tenant Data Isolation

**What it does:**
- Every data access is automatically filtered by `OrganizationId`
- No endpoint can return data belonging to a different org
- Isolation enforced at the **repository layer**, not the controller

**Implementation:**
- `OrganizationId` extracted from JWT claims
- Base repository applies global query filter: `.Where(x => x.OrganizationId == currentOrgId)`
- Integration tests verify cross-tenant access returns `403` or empty set

---

### 6. JWT Authentication

**Endpoints:**
- `POST /api/auth/login` — Returns JWT
- `POST /api/auth/register` — Standalone user registration (optional for V1)

**Token payload:**
```
sub: userId
email: user@example.com
org_id: organizationId
role: Admin | Member
exp: unix timestamp
```

**Rules:**
- Tokens expire in 60 minutes (configurable)
- Secret key stored in environment config, never hardcoded
- All routes except `/auth/*` require a valid bearer token

---

### 7. Automated Tests (30+)

**Unit Tests — Application Layer**

| Test Group | Count |
|------------|-------|
| Register organization use case | 4 |
| Invite user use case | 5 |
| Accept invite use case | 4 |
| Create task use case | 5 |
| Update task use case | 4 |
| Delete task (role check) | 3 |
| Tenant isolation logic | 3 |

**Integration Tests — API Layer (WebApplicationFactory)**

| Test Group | Count |
|------------|-------|
| Auth endpoints | 4 |
| Organization endpoints | 3 |
| Task CRUD endpoints | 5 |
| Cross-tenant access denied | 3 |

**Total: 43 tests minimum**

---

## V2 — Performance Features

---

### 8. Redis Caching

**What it does:**
- Caches `GET /api/tasks` response per organization
- Cache key format: `tasks:org:{orgId}:page:{page}:filter:{hash}`
- Cache invalidated on any Create / Update / Delete in that org
- TTL: 5 minutes (configurable)

**Implementation:** `IDistributedCache` abstraction → Redis in prod, in-memory in tests

---

### 9. Pagination & Filtering

**Query parameters for `GET /api/tasks`:**
```
?page=1&pageSize=20
?status=InProgress
?priority=High
?assigneeId={userId}
?from=2024-01-01&to=2024-12-31
?sortBy=createdAt&order=desc
```

**Response shape:**
```json
{
  "data": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 87,
  "totalPages": 5
}
```

---

### 10. Database Indexing

| Table | Index |
|-------|-------|
| Tasks | `OrganizationId` |
| Tasks | `OrganizationId + Status` |
| Tasks | `AssigneeId` |
| OrgUsers | `OrganizationId + UserId` |
| InviteTokens | `Token` (unique) |

---

### 11. Endpoint Benchmark

- Tool: **BenchmarkDotNet**
- Target: `GET /api/tasks` — with and without Redis cache
- Report committed to `/docs/benchmarks/`
- Expected result: cache hit ≥ 10× faster than cold DB query

---

## V3 — Scalability Features

---

### 12. Background Job — Email Invite Simulation

**What it does:**
- When an invite is created, enqueue a background job
- Job logs: `"Sending invite email to {email} for org {orgName}"`
- Simulates real email send (no SMTP required)

**Implementation:** `IHostedService` queue or Hangfire (lightweight)

---

### 13. Domain Events — Task Created

**Flow:**
1. `CreateTaskUseCase` raises `TaskCreatedEvent`
2. `MediatR` dispatches to `TaskCreatedEventHandler`
3. Handler logs the event with structured context
4. (Extensible: swap handler for real message broker later)

**Event payload:**
```csharp
record TaskCreatedEvent(Guid TaskId, Guid OrgId, Guid CreatedByUserId, DateTime CreatedAt);
```

---

### 14. Rate Limiting

**Rules:**
- Global: 100 requests / minute per IP
- Auth endpoints: 10 requests / minute per IP (brute-force protection)
- Exceeded limit returns `429 Too Many Requests` with `Retry-After` header

**Implementation:** ASP.NET Core built-in `RateLimiter` (fixed window policy)

---

### 15. Structured Logging

**Tool:** Serilog

**Every log entry includes:**
```
RequestId, OrgId, UserId, Endpoint, StatusCode, DurationMs
```

**Log levels:**
- `Information` — request in/out, events fired
- `Warning` — invite token expired, unauthorized attempt
- `Error` — unhandled exceptions (with stack trace)

**Sinks:**
- `Console` — dev (human-readable)
- `File` — prod (`logs/teamflow-.json`, rolling daily)

---

## V4 — Production Polish Features

---

### 16. Health Check Endpoint

**Endpoint:** `GET /health`

**Checks:**
- PostgreSQL connectivity
- Redis connectivity
- Returns `200 OK` if all healthy, `503` if any fail

**Response:**
```json
{
  "status": "Healthy",
  "checks": {
    "postgres": "Healthy",
    "redis": "Healthy"
  },
  "duration": "42ms"
}
```

---

### 17. Docker Compose — Full Stack

**Services:**
```yaml
services:
  api:        # TeamFlow.API — built from Dockerfile
  postgres:   # postgres:16-alpine
  redis:      # redis:7-alpine
```

**Features:**
- Named volumes for Postgres data persistence
- Environment variables via `.env` file
- Health checks on DB and Redis before API starts
- Single command to run entire stack: `docker-compose up`

---

### 18. Environment Configuration

| Key | Dev | Prod |
|-----|-----|------|
| `ConnectionStrings__Postgres` | localhost:5432 | env var |
| `Redis__ConnectionString` | localhost:6379 | env var |
| `Jwt__Secret` | dev-secret (never committed) | env var |
| `Jwt__ExpiryMinutes` | 60 | 60 |
| `Logging__Level` | Debug | Warning |

**Rules:**
- No secrets in `appsettings.json` committed to git
- `.env.example` committed with placeholder values
- Prod reads from environment variables only

---

### 19. Global Error Handling Middleware

**What it does:**
- Catches all unhandled exceptions
- Returns consistent `ProblemDetails` (RFC 7807) shape
- Never leaks stack traces in production

**Error response shape:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource not found",
  "status": 404,
  "detail": "Task with id '...' was not found.",
  "traceId": "00-abc123..."
}
```

**Exception → Status code mapping:**
```
NotFoundException        → 404
ValidationException      → 422
UnauthorizedException    → 403
Any other exception      → 500 (detail hidden in prod)
```

---

### 20. CI/CD Pipeline — GitHub Actions

**Trigger:** Push or PR to `main`

**Steps:**
```
1. Checkout code
2. Setup .NET 8
3. Restore dependencies
4. Build solution
5. Run all tests (with test results artifact)
6. Build Docker image
7. (Optional) Push to Docker Hub or GHCR
```

**Badges:** Build status + test pass/fail shown in README
