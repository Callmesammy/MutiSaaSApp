# CLAUDE.MD — Project Bible

## Project Overview
Multi Saas tenancy app

## Solution Structure
Domain Class Library          → Entities, Interfaces, Enums, BaseEntity
Application Class Library     → DTOs, Service Interfaces, Validators
Infrastructure Class Library  → EF Core, Repositories, JWT, Email
 API Class Library            → Controllers, Middleware, Docker, Program.cs
 docker-compose
XUnit

## Tech Stack
- .NET 10 Web API
- EF Core + SQL Server
- JWT Bearer Authentication
- BCrypt password hashing
- AutoMapper
- FluentValidation
- Swagger

## Coding Rules (ALWAYS follow these)
- async/await everywhere
- Repositories return null → Services throw custom exceptions
- All endpoints return ApiResponse<T>
- Global exception middleware handles all errors
- All entities inherit BaseEntity (Id, CreatedAt, UpdatedAt, IsDeleted)
- Soft delete only — never hard delete
- XML comments on all public methods

## Existing Patterns Already Built
- Auth: Register, Login, Refresh Token, Role-based JWT
- BaseEntity with soft delete
- ApiResponse<T> wrapper
- GlobalExceptionMiddleware
- Custom exceptions: NotFoundException, ConflictException

## How I Want Code Generated
- Always go layer by layer
- Show the full file path above every code block
- Do not skip error handling
- Match the exact patterns already in the project

Always place generated code in the correct project.
Use existing namespaces
