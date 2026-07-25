# Task Management REST API

### Overview
This is a simple RESTful API for managing projects and their associated tasks. It allows users to create projects, add tasks to them, and perform various operations like filtering, sorting, and pagination on tasks across all projects or within a specific project.

### Tech Stack
- **Framework**: .NET 9 (ASP.NET Core Web API)
- **Database**: MySQL / MariaDB
- **ORM**: Entity Framework Core (Pomelo.EntityFrameworkCore.MySql)
- **API Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing

### Setup Instructions

#### Prerequisites
- .NET 9 SDK
- MySQL or MariaDB running locally

#### 1. Install `dotnet-ef` tool (if missing)
```bash
dotnet tool install --global dotnet-ef
```

#### 2. Configure Connection String
Open `TaskMangementAPI/appsettings.json` or `TaskMangementAPI/appsettings.Development.json` and update the `DefaultConnection` string with your MySQL credentials:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementDb;User=root;Password=your_password;"
}
```

#### 3. Run Migrations
Apply the database migrations to create the schema:
```bash
cd TaskMangementAPI
dotnet ef database update
```

#### 4. Run the Application
```bash
dotnet run
```
The API will be available at `https://localhost:5001` (or the port shown in your terminal).
You can access the Swagger UI at `/swagger` (e.g., `https://localhost:5001/swagger`).

### API Documentation

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/projects` | Create a new project |
| GET | `/api/projects` | List all projects (paginated) |
| GET | `/api/projects/{id}` | Get a single project details |
| PUT | `/api/projects/{id}` | Update a project |
| DELETE | `/api/projects/{id}` | Delete a project (cascade deletes tasks) |
| POST | `/api/projects/{id}/tasks` | Create a task under a specific project |
| GET | `/api/projects/{id}/tasks` | List tasks for a project (paginated, filterable, sortable) |
| GET | `/api/tasks/{id}` | Get a single task |
| PUT | `/api/tasks/{id}` | Update a task |
| DELETE | `/api/tasks/{id}` | Delete a task |
| GET | `/api/tasks` | List all tasks across all projects (paginated, filterable, sortable, searchable) |

#### Example Requests/Responses

**Create Project**
- **POST** `/api/projects`
- **Request Body:**
```json
{
  "name": "New Project",
  "description": "Project Description"
}
```
- **Response (201 Created):**
```json
{
  "id": 1,
  "name": "New Project",
  "description": "Project Description",
  "taskCount": 0,
  "createdAt": "2023-10-27T10:00:00Z",
  "updatedAt": "2023-10-27T10:00:00Z"
}
```

**Create Task under Project**
- **POST** `/api/projects/1/tasks`
- **Request Body:**
```json
{
  "title": "Initial Task",
  "description": "Task details",
  "status": "Todo",
  "priority": "Medium",
  "dueDate": "2023-12-31T23:59:59Z"
}
```
- **Response (201 Created):**
```json
{
  "id": 1,
  "projectId": 1,
  "projectName": "New Project",
  "title": "Initial Task",
  "description": "Task details",
  "status": "Todo",
  "priority": "Medium",
  "dueDate": "2023-12-31T23:59:59Z",
  "createdAt": "2023-10-27T10:05:00Z",
  "updatedAt": "2023-10-27T10:05:00Z"
}
```

**List Tasks with Filters**
- **GET** `/api/tasks?status=Todo&priority=High&q=initial&page=1&limit=10`
- **Response (200 OK):**
```json
{
  "items": [ ... ],
  "totalCount": 1,
  "page": 1,
  "limit": 10,
  "totalPages": 1
}
```

### Schema Rationale
- **Project/Task Relationship**: A One-to-Many relationship where a project can have multiple tasks. Cascade delete is enabled to ensure tasks are cleaned up when a project is removed.
- **Enums as Strings**: `Status` and `Priority` are stored as strings in the database for better readability and to avoid issues if the enum order changes.
- **Indexes**:
  - `Project.Name`: Unique index to prevent duplicate project names.
  - `Task.ProjectId`: Index to optimize joins and filtering by project.

### Running Tests
Execute all tests (Unit and Integration) using:
```bash
dotnet test
```
- **Unit Tests**: Cover business logic in isolation (e.g., due date validation, duplicate name checks, status transition logging) using an InMemory database.
- **Integration Tests**: Verify full API flows (Lifecycle, Filtering, Search/Pagination) using a real HTTP client and an InMemory database to simulate the environment.

### Known Limitations / Assumptions
- **Search**: The search functionality (`?q=`) is case-insensitive (using `.ToLower()`).
- **Timezone**: All dates are stored and returned in UTC.
- **Security**: This is a simplified version for demonstration purposes; it does not include authentication or authorization.
- **Soft Delete**: Not implemented; all deletes are permanent.
