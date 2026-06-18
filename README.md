# Task Management API

A full-featured **RESTful API** for managing projects, tasks, comments, and notifications, built with **ASP.NET Core** and **Entity Framework Core**. The system supports real-time notifications via **SignalR**, role-based authorization, and pagination across all list endpoints.

---

## Key Features

- **Authentication & Authorization** — JWT-based authentication with role-based access control (Admin, TeamLeader, User).
- **Project Management** — Full CRUD operations for projects with ownership-based access, search, and project-level statistics (completion percentage, overdue tasks, status breakdown).
- **Task Management** — Create, assign, update, filter, and track tasks with support for status workflows (Pending → InProgress → Completed / OnHold / Cancelled), priority levels, and due dates.
- **Comments System** — Threaded comments on tasks with authorization logic ensuring only relevant users (assignee, project creator, or admin) can participate.
- **Real-Time Notifications** — Dual-channel notification system: persisted in the database for history and pushed instantly via SignalR for live updates.
- **Pagination** — Generic, reusable pagination across all list endpoints using a custom `IQueryable<T>` extension method.
- **Database Seeding** — Automatic admin user creation on first run.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core (Code-First) |
| Database | SQL Server |
| Authentication | JWT (JSON Web Tokens) |
| Real-Time | SignalR |
| Architecture | Layered Services + Controller-based routing |

---

## Architecture Overview

```
Client (HTTP/WebSocket)
        │
        ▼
┌─────────────────────────┐
│    JWT Middleware        │  ← Validates token, extracts claims
└────────────┬────────────┘
             ▼
┌─────────────────────────┐
│    BaseController       │  ← GetCurrentUserId(), ValidateModel()
└────────────┬────────────┘
             ▼
┌─────────────────────────┐
│    Controllers          │  ← Business logic + authorization
│  (Auth, Project, Task,  │
│   Comment, Notification,│
│   User)                 │
└─────┬──────────┬────────┘
      │          │
      ▼          ▼
┌──────────┐  ┌──────────────────────────┐
│ DbContext│  │ INotificationService     │
│ (EF Core)│  │   └─► ITaskHubService    │
└──────────┘  │        └─► SignalR Hub   │
              └──────────────────────────┘
```

---

## Database Schema

```
User (1) ──────── (M) Project
  │                      │
  │ (assigned/created)   │ (contains)
  │                      │
  ├──── (M) TaskItem ────┘
  │           │
  │           └──── (M) Comment
  │
  └──── (M) Notification
```

**Key relationships:**
- A `User` has two separate relationships with `TaskItem`: as **creator** (`TasksCreated`) and as **assignee** (`TasksAssigned`).
- Deleting a user **restricts** deletion if they created tasks, but **sets null** on tasks assigned to them.
- Deleting a project **cascades** to all its tasks and their comments.
- `Email` and `Username` have unique indexes for fast lookups.
- Composite index on `(UserId, IsRead)` in Notifications for efficient unread-count queries.

---

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/Register` | Register a new user |
| POST | `/api/Auth/Login` | Login and receive JWT token |

### Projects
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Project` | Create a new project |
| GET | `/api/Project` | Get all projects (admin: all, user: own) with search |
| GET | `/api/Project/my-projects` | Get current user's projects |
| GET | `/api/Project/{id}` | Get project by ID |
| GET | `/api/Project/{id}/tasks` | Get project tasks (filterable by status/priority) |
| GET | `/api/Project/{id}/statistics` | Get project statistics |
| PUT | `/api/Project/{id}` | Update a project |
| DELETE | `/api/Project/{id}` | Delete a project (only if no tasks exist) |

### Tasks
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Task` | Create a new task |
| GET | `/api/Task` | Get my assigned tasks (filter by status, priority, search, overdue) |
| GET | `/api/Task/{id}` | Get task by ID |
| GET | `/api/Task/{id}/comments` | Get comments for a task |
| GET | `/api/Task/{id}/comments/count` | Get comment count for a task |
| GET | `/api/Task/statistics` | Get personal task statistics |
| PUT | `/api/Task/{id}` | Update a task |
| PATCH | `/api/Task/{id}/status` | Change task status |
| DELETE | `/api/Task/{id}` | Delete a task |

### Comments
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Comment` | Add a comment to a task |
| GET | `/api/Comment` | Get my comments |
| GET | `/api/Comment/{id}` | Get comment by ID |
| PUT | `/api/Comment/{id}` | Update a comment (owner only) |
| DELETE | `/api/Comment/{id}` | Delete a comment |

### Notifications
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Notification` | Get my notifications (filterable by read status) |
| GET | `/api/Notification/{id}` | Get notification by ID |
| GET | `/api/Notification/unread-count` | Get unread notification count |
| PUT | `/api/Notification/{id}/mark-as-read` | Mark a notification as read |
| PUT | `/api/Notification/mark-all-read` | Mark all notifications as read |
| DELETE | `/api/Notification/{id}` | Delete a notification |
| DELETE | `/api/Notification/delete-all` | Delete all my notifications |

### Users
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/User` | Get all users (Admin/TeamLeader only) |
| GET | `/api/User/Profile` | Get my profile |
| GET | `/api/User/Profile/{id}` | Get user by ID (Admin only) |
| PUT | `/api/User/Profile` | Update my profile |
| PUT | `/api/User/Profile/{id}` | Update user by admin |
| PUT | `/api/User/ChangePassword` | Change my password |
| DELETE | `/api/User/{id}` | Delete user (Admin/TeamLeader only) |

---

## Authorization Model

The system uses a **Role + Ownership** authorization pattern:

| Role | Permissions |
|------|------------|
| **Admin** | Full access to all resources |
| **TeamLeader** | View all users, delete users |
| **User** | CRUD on own resources only |

For tasks and comments, access is granted to the **creator**, the **assigned user**, or the **project owner** — ensuring relevant stakeholders can always interact with their work items.

---

## Real-Time Notifications (SignalR)

The system uses **SignalR** with a group-based messaging pattern:

- **`user_{userId}`** — Each user joins their personal group on connection. Used for delivering notifications (task assigned, status changed, comment added).
- **`task_{taskId}`** — Users join a task-specific room when viewing a task. Used for broadcasting new comments and status updates to all viewers.

### Notification Triggers

| Event | Who Gets Notified |
|-------|-------------------|
| Task assigned | Assigned user (if not the creator) |
| Task completed | Project owner |
| Status changed | Task creator (if changed by someone else) |
| Comment added | Assigned user + Project owner (deduplicated) |

---

## Pagination

All list endpoints support pagination via query parameters:

```
GET /api/Task?PageNumber=1&PageSize=10&status=0&priority=3&search=fix
```

Response format:
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 47,
  "totalPages": 5
}
```

Implemented as a generic `IQueryable<T>` extension method, making it reusable across all entities without code duplication.

---

## Getting Started

### Prerequisites
- .NET 8 SDK (or your target version)
- SQL Server (LocalDB or full instance)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/Task-Management-API.git
   cd Task-Management-API
   ```

2. **Update the connection string** in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=TaskManagementDB;Trusted_Connection=true;TrustServerCertificate=true;"
   }
   ```

3. **Apply migrations and run**
   ```bash
   dotnet ef database update
   dotnet run
   ```

4. **Access the API** at `https://localhost:5001/swagger` (Swagger UI)

An **admin account** is automatically seeded on first run:
- Email: `admin@example.com`
- Password: `123456`

---

## Project Structure

```
Task-Management-Project/
├── Controllers/
│   ├── Base/
│   │   └── BaseController.cs        # Shared controller logic
│   ├── AuthController.cs            # Register & Login
│   ├── ProjectController.cs         # Project CRUD + statistics
│   ├── TaskController.cs            # Task CRUD + status + filtering
│   ├── CommentController.cs         # Comment CRUD
│   ├── NotificationController.cs    # Notification management
│   └── UserController.cs            # User profiles & admin operations
├── Models/
│   ├── User.cs, Project.cs, TaskItem.cs, Comment.cs, Notification.cs
│   ├── Common/
│   │   ├── PagedResult.cs           # Generic pagination response
│   │   └── PaginationParams.cs      # Pagination query parameters
│   └── Context.cs                   # EF Core DbContext + Fluent API config
├── Services/
│   ├── INotificationService.cs      # Notification business logic interface
│   ├── NotificationServicecs.cs     # Notification implementation
│   ├── ITaskHubService.cs           # SignalR delivery interface
│   └── TaskHubService.cs            # SignalR delivery implementation
├── Hubs/
│   └── TaskHub.cs                   # SignalR hub with group management
├── DTOs/                            # Request/Response data transfer objects
├── Helpers/
│   ├── JWTService.cs                # Token generation
│   └── PasswordHasher.cs            # Password hashing utility
├── Extensions/
│   └── PaginationExtensions.cs      # Generic IQueryable pagination
├── Exceptions/                      # Custom exception classes
├── Enum/                            # TaskStatus, TaskPriority, NotificationType
└── Data/
    └── DBSeeder.cs                  # Initial admin seeding
```

---

## Design Decisions

| Decision | Reasoning |
|----------|-----------|
| **Direct DbContext** instead of Repository Pattern | Project scope doesn't warrant the extra abstraction layer; EF Core's DbContext already acts as a Unit of Work + Repository |
| **BaseController inheritance** | Eliminates repeated user-extraction and validation code across all controllers (DRY) |
| **Separate INotificationService and ITaskHubService** | Single Responsibility — business logic (what to notify) is decoupled from delivery mechanism (how to push via SignalR) |
| **Generic pagination as extension method** | Cleaner API (`query.ToPagedResultAsync()`) than a separate service; reusable across all entities |
| **SignalR Groups** over individual connections | Scalable approach — users can have multiple connections (tabs/devices) and still receive all notifications |
| **Ownership + Role authorization** | Flexible model where admins have full access while regular users can only interact with resources they own or are assigned to |

---

## Future Improvements

- [ ] Add file/attachment support for tasks
- [ ] Implement Team management (models exist, relationships commented out for future use)
- [ ] Add email notifications alongside real-time push
- [ ] Implement task deadline reminders (method exists in `INotificationService`, needs a background job scheduler)
- [ ] Add refresh token support for JWT
- [ ] Write unit and integration tests
- [ ] Add rate limiting and request throttling

---

## License

This project is open source and available under the [MIT License](LICENSE).
