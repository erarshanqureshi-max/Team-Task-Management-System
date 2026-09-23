# TaskFlow - Team Task Management System

A clean, robust, and full-featured **Team Task Management System** built with **ASP.NET Core Web API (.NET 8)**, **Entity Framework Core**, **Microsoft SQL Server**, **JWT Bearer Authentication**, and **React.js (Vite + Bootstrap 5)**.

The project follows a clean **Controller $\rightarrow$ Service $\rightarrow$ EF Core $\rightarrow$ SQL Server** architecture designed specifically for readability, high testability, and confident technical interview presentation.

---

## 🌐 Live Application Demo
- **Live Demo URL**: **[https://astronomy-saturn-allocated-messages.trycloudflare.com](https://astronomy-saturn-allocated-messages.trycloudflare.com)**
- **API Status**: Online (ASP.NET Core Web API .NET 8 + React Vite Frontend + MS SQL Server)
- **Interactive Credentials**:
  - **Admin**: `admin@example.com` | `Password123!`
  - **Manager**: `manager@example.com` | `Password123!`
  - **Member**: `user@example.com` | `Password123!`

---

## Table of Contents
1. [Project Overview](#project-overview)
2. [Key Features](#key-features)
3. [Technology Stack](#technology-stack)
4. [Architecture & Design Principles](#architecture--design-principles)
5. [Folder Structure](#folder-structure)
6. [Role Permissions Matrix](#role-permissions-matrix)
7. [Database Setup & Migrations](#database-setup--migrations)
8. [Running the Application](#running-the-application)
   - [Running Backend](#running-backend-aspnet-core-8)
   - [Running Frontend](#running-frontend-react--vite)
9. [Swagger & API Documentation](#swagger--api-documentation)
10. [Sample Development Credentials](#sample-development-credentials)
11. [Important API Endpoints](#important-api-endpoints)
12. [Running Automated Tests](#running-automated-tests-xunit)
13. [CI/CD with GitHub Actions](#cicd-with-github-actions)
14. [Postman Collection](#postman-collection)
15. [Docker Setup](#docker-setup)

---

## Project Overview

TaskFlow is designed to empower teams to organize work, delegate responsibilities, and monitor task progression with strict role-based access control.

The system enforces:
- **Admin**: Full oversight over users, teams, task assignments, and organization metrics.
- **Manager**: Authority restricted to their managed team, creating tasks, assigning work to team members, and viewing team-level velocity.
- **User**: Scoped access restricted to personal assigned tasks, updating task statuses, and collaborating via comments.

---

## Key Features

- **Authentication & Security**:
  - Secure JWT authentication containing `userId`, `email`, and `role` claims.
  - Salted password hashing with BCrypt.
  - Role-based authorization enforced at both controller and service business layers.
- **Task Management**:
  - Full CRUD operations with priority levels (`Low`, `Medium`, `High`, `Urgent`) and status tracking (`To Do`, `In Progress`, `Done`).
  - Filtering by status, priority, and due date.
  - Keyword search across task titles and descriptions.
  - Strict isolation: Managers cannot assign tasks outside their team; Users cannot view or modify unassigned tasks.
- **Interactive Comments Thread**:
  - Real-time discussion on individual tasks with author timestamps and role details.
- **In-App Notifications**:
  - Automated triggers when tasks are assigned or statuses are updated.
  - Unread counters, mark as read, and bulk mark-all-read.
- **Role-Tailored Dashboards**:
  - **Admin**: Organization-wide statistics (Total Users, Teams, Task Status Breakdown, Overdue and High Priority counts).
  - **Manager**: Team-specific metrics and workload breakdown.
  - **User**: Personal assigned tasks, progress metrics, and upcoming deadlines.
- **Global Error Handling**:
  - Standardized JSON responses (`{ success: false, message: "..." }`) with appropriate HTTP status codes (200, 201, 400, 401, 403, 404, 500) without exposing stack traces.

---

## Technology Stack

### Backend
- **Framework**: ASP.NET Core Web API (.NET 8)
- **ORM**: Entity Framework Core 8.0
- **Database**: Microsoft SQL Server / LocalDB
- **Authentication**: JWT Bearer Tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Password Security**: `BCrypt.Net-Next`
- **API Documentation**: Swagger / OpenAPI with Bearer Authorization (`Swashbuckle.AspNetCore`)
- **Testing**: xUnit, Moq, EF Core InMemory Provider

### Frontend
- **Framework**: React.js (v18)
- **Build Tool**: Vite 5
- **Routing**: React Router v6
- **HTTP Client**: Centralized Axios with request/response interceptors
- **UI Framework**: Bootstrap 5 + Bootstrap Icons

### Containerization
- Multi-stage Dockerfiles for Backend and Frontend
- Docker Compose orchestrating SQL Server, API, and React Client

---

## Architecture & Design Principles

```
API Request
    │
    ▼
ExceptionMiddleware        --> Catches unhandled exceptions, returns clean JSON
    │
    ▼
Controllers                --> Route mapping, DTO validation, HTTP response codes
    │
    ▼
Services                   --> Business rules, role authorization, notifications
    │
    ▼
Entity Framework Core      --> ApplicationDbContext, Relational Mappings, Migrations
    │
    ▼
Microsoft SQL Server       --> Relational storage with foreign key constraints & indexes
```

### Why this architecture?
- **Clarity over Complexity**: Directly follows the Controller $\rightarrow$ Service $\rightarrow$ EF Core flow. Avoids over-engineering like unnecessary Repository/Unit-of-Work wrappers over EF Core (which already implements Repository and Unit-of-Work patterns).
- **Security-First**: Role authorization is validated on every endpoint and verified in the business service layer.
- **Interview-Friendly**: Easy to navigate and explain each layer's distinct responsibility.

---

## Folder Structure

```
d:/Assignment Project/
├── Backend/
│   ├── TaskManagement.API/
│   │   ├── Controllers/             # REST endpoints (Auth, Tasks, Teams, Users, Comments, Notifications, Dashboard)
│   │   ├── Services/                # Business logic & role validation rules
│   │   ├── Interfaces/              # Dependency injection service abstractions
│   │   ├── Data/                    # ApplicationDbContext, DbInitializer (Seed Data)
│   │   ├── Models/                  # Domain Entities (User, Team, TeamMember, TaskItem, Comment, Notification)
│   │   ├── DTOs/                    # Request/Response Data Transfer Objects
│   │   ├── Middleware/              # Global Exception Handling Middleware
│   │   ├── Helpers/                 # JwtHelper & Claims Principal utilities
│   │   ├── Migrations/              # EF Core database schema migrations
│   │   ├── appsettings.json         # Configuration & Connection strings
│   │   ├── Program.cs               # DI setup, CORS, Auth, Swagger Bearer configuration
│   │   └── Dockerfile               # Multi-stage ASP.NET Core build
│   └── TaskManagement.Tests/
│       ├── Helpers/                 # TestDbContextFactory (InMemory DB)
│       ├── Services/                # AuthServiceTests, TaskServiceTests, AuthorizationTests
│       └── TaskManagement.Tests.csproj
├── Frontend/
│   ├── src/
│   │   ├── api/                     # Centralized Axios client and feature API services
│   │   ├── context/                 # AuthContext (JWT management, role helpers)
│   │   ├── components/              # Layout, Navbar, Sidebar, Badges, Toast, ProtectedRoute
│   │   ├── pages/                   # Login, Register, Dashboard, Tasks, Teams, Users, Notifications
│   │   ├── App.jsx                  # Route definitions and role gates
│   │   ├── index.css                # Admin styles & layout offsets
│   │   └── main.jsx                 # React root entry point
│   ├── package.json
│   ├── vite.config.js
│   ├── nginx.conf                   # Nginx SPA fallback configuration
│   └── Dockerfile                   # Multi-stage Frontend build
├── docker-compose.yml               # Multi-container orchestration
└── README.md
```

---

## Role Permissions Matrix

| Feature / Action | Admin | Manager | User |
| :--- | :---: | :---: | :---: |
| **Login & Register** | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| **View Dashboard** | All Org Stats | Team Stats | Own Task Stats |
| **View Tasks** | All Tasks | Team Tasks | Assigned Tasks Only |
| **Create Tasks** | Any Team & User | Own Team Members | :x: |
| **Update Task Details** | Any Task | Own Team Tasks | :x: |
| **Delete Tasks** | Any Task | Own Team Tasks | :x: |
| **Update Task Status** | Any Task | Team Tasks | Assigned Tasks Only |
| **Assign / Reassign Task** | Any User | Own Team Members | :x: |
| **Add / View Comments** | :white_check_mark: | Team Tasks | Assigned / Team Tasks |
| **In-App Notifications** | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| **Manage Teams (CRUD)** | :white_check_mark: | :x: | :x: |
| **Manage Team Members** | :white_check_mark: | Own Team | :x: |
| **View User Directory** | :white_check_mark: | :white_check_mark: | :x: |

---

## Database Setup & Migrations

### Connection String Setup
In `Backend/TaskManagement.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
}
```

> If using **SQL Server Express**, change `Server=localhost` to `Server=.\\SQLEXPRESS`.  
> If using **SQL LocalDB**, change to `Server=(localdb)\\MSSQLLocalDB`.

### Automatic Seeding
The application automatically runs `context.Database.MigrateAsync()` and seeds sample users, teams, tasks, comments, and notifications on startup.

### Manual EF Core CLI Commands
Run from `Backend/TaskManagement.API/`:
```powershell
# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply migrations to database
dotnet ef database update

# Rollback to specific migration
dotnet ef database update <PreviousMigrationName>
```

---

## Running the Application

### Running Backend (ASP.NET Core 8)
1. Open PowerShell or Terminal.
2. Navigate to `Backend/TaskManagement.API`:
   ```powershell
   cd "Backend/TaskManagement.API"
   dotnet run --urls http://localhost:5000
   ```
3. The API will start on: **http://localhost:5000**
4. Swagger UI will be accessible at: **http://localhost:5000/swagger**

### Running Frontend (React + Vite)
1. Open a new PowerShell or Terminal window.
2. Navigate to `Frontend`:
   ```powershell
   cd "Frontend"
   npm install
   npm run dev
   ```
3. Open browser at: **http://localhost:5173**

---

## Swagger & API Documentation

Swagger is configured with **JWT Bearer Authentication**.

1. Navigate to: **http://localhost:5000/swagger**
2. Execute `POST /api/auth/login` using any sample credential.
3. Copy the returned `token` from the response.
4. Click the green **Authorize** button at the top right of the Swagger UI.
5. In the input box, enter your token (or `Bearer <token>`) and click **Authorize**.
6. All protected endpoints can now be executed directly from Swagger.

---

## Sample Development Credentials

All seeded sample accounts use the password: `Password123!`

| Role | Email | Password | Pre-assigned Role Scope |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@example.com` | `Password123!` | System administrator, full organization access |
| **Manager** | `manager@example.com` | `Password123!` | Manages "Engineering Team" and "Product Team" |
| **User** | `user@example.com` | `Password123!` | Member of "Engineering Team", assigned developer tasks |
| **User (Extra)** | `charlie@example.com` | `Password123!` | Member of "Product Team", assigned design tasks |

---

## Important API Endpoints

### Authentication
- `POST /api/auth/register` - Create account (`Name`, `Email`, `Password`, `Role`)
- `POST /api/auth/login` - Authenticate and retrieve JWT token

### Tasks
- `GET /api/tasks` - List tasks (Filtered by caller role + Query params: `status`, `priority`, `deadline`, `search`)
- `GET /api/tasks/{id}` - Get task details
- `POST /api/tasks` - Create task (*Admin*, *Manager*)
- `PUT /api/tasks/{id}` - Update task details (*Admin*, *Manager*)
- `DELETE /api/tasks/{id}` - Delete task (*Admin*, *Manager*)
- `PATCH /api/tasks/{id}/status` - Update status (`To Do`, `In Progress`, `Done`)
- `PATCH /api/tasks/{id}/assign` - Assign task to user (*Admin*, *Manager*)

### Comments
- `GET /api/tasks/{taskId}/comments` - List comments for task
- `POST /api/tasks/{taskId}/comments` - Add a comment

### In-App Notifications
- `GET /api/notifications` - Get notifications for authenticated user
- `PATCH /api/notifications/{id}/read` - Mark single notification as read
- `PATCH /api/notifications/read-all` - Mark all notifications as read

### Dashboard
- `GET /api/dashboard/admin` - Overall organization analytics (*Admin*)
- `GET /api/dashboard/manager` - Team analytics (*Admin*, *Manager*)
- `GET /api/dashboard/user` - Personal task analytics (*Authenticated User*)

### Teams & Members
- `GET /api/teams` - List accessible teams
- `GET /api/teams/{id}` - Get team details
- `POST /api/teams` - Create team (*Admin*)
- `PUT /api/teams/{id}` - Update team (*Admin*)
- `DELETE /api/teams/{id}` - Delete team (*Admin*)
- `GET /api/teams/{id}/members` - List team members
- `POST /api/teams/{id}/members` - Add user to team (*Admin*, *Manager*)
- `DELETE /api/teams/{id}/members/{userId}` - Remove user from team (*Admin*, *Manager*)

### Users
- `GET /api/users` - List organization users (*Admin*, *Manager*)
- `GET /api/users/{id}` - Get user summary

---

## Running Automated Tests (xUnit)

Run the xUnit test suite from the repository root or backend folder:
```powershell
cd Backend
dotnet test
```

### Covered Test Cases:
- **`AuthServiceTests`**:
  - `RegisterAsync_WithValidData_CreatesUserAndReturnsToken`
  - `RegisterAsync_WithExistingEmail_ThrowsBadRequestException`
  - `LoginAsync_WithValidCredentials_ReturnsAuthResponseWithJwt`
  - `LoginAsync_WithInvalidPassword_ThrowsBadRequestException`
  - `LoginAsync_WithNonExistentEmail_ThrowsBadRequestException`
- **`TaskServiceTests`**:
  - `CreateTaskAsync_AsManagerForOwnTeam_Succeeds`
  - `CreateTaskAsync_AsManagerAssigningOutsideTeam_ThrowsBadRequestException`
  - `CreateTaskAsync_AsUser_ThrowsForbiddenException`
  - `UpdateStatusAsync_AsAssignedUser_SucceedsAndDispatchesNotification`
  - `UpdateStatusAsync_AsUnassignedUser_ThrowsForbiddenException`
  - `GetTaskByIdAsync_AsUnassignedUser_ThrowsForbiddenException`
  - `AssignTaskAsync_AsManagerToTeamMember_Succeeds`
  - `AssignTaskAsync_AsManagerToOutsider_ThrowsBadRequestException`
- **`AuthorizationTests`**:
  - `GenerateToken_ContainsRequiredUserIdEmailAndRoleClaims` (Admin, Manager, User)
  - `GetUserIdAndRole_FromClaimsPrincipal_ExtractsCorrectly`

---

## CI/CD with GitHub Actions

The repository includes a ready-to-run GitHub Actions workflow located at [`.github/workflows/ci.yml`](.github/workflows/ci.yml).
- Automatically builds the ASP.NET Core solution.
- Executes all 17 xUnit tests on every pull request and push to `main`/`master`.
- Installs and verifies the React production build (`npm run build`).

---

## Postman Collection

A complete, pre-configured Postman Collection is included in the project root:
- File: [`TaskFlow.postman_collection.json`](TaskFlow.postman_collection.json)
- Includes automated token capture: Logging in as Admin will automatically save the Bearer token to the collection variable `{{token}}` for seamless testing of all endpoints.

---

## Docker Setup

To build and run the entire system (SQL Server, ASP.NET Core API, and React Frontend) using Docker:

```powershell
# Build and run containers in background
docker compose up -d --build

# View logs
docker compose logs -f

# Stop containers
docker compose down
```

### Container Endpoints:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000/swagger
- **SQL Server**: localhost:1433
