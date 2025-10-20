# Task Management API

A comprehensive RESTful API built with ASP.NET Core for managing tasks, users, and performance reviews in an organization. The system supports role-based access control with distinct functionalities for Managers and Employees.

## 🚀 Key Features

- **User Authentication & Authorization**
  - JWT-based authentication
  - Role-based access control (Manager/Employee)
  - Secure password hashing using SHA-256

- **Task Management**
  - Create, read, update, and delete tasks
  - Task status tracking (Pending, InProgress, Done)
  - Task assignment to employees
  - Due date management

- **Task Updates**
  - Employees can add progress updates to their tasks
  - Support for attachment URLs
  - Chronological update history

- **Task Reviews**
  - Managers can review completed tasks
  - Rating system (1-5 stars)
  - Written feedback and comments

- **Access Control**
  - Managers have full system access
  - Employees can only view and modify their assigned tasks
  - Hierarchical permission system

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **ORM**: Entity Framework Core
- **API Documentation**: Swagger/OpenAPI
- **Architecture Pattern**: Repository Pattern with Service Layer
- **Language**: C# (.NET 6/7/8)

## ⚙️ Installation and Setup

### Prerequisites

- .NET SDK 6.0 or higher
- SQL Server (LocalDB, Express, or Full version)
- Visual Studio 2022 or VS Code (recommended)
- Postman or similar API testing tool (optional)

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/task-management-api.git
cd task-management-api
```

### Step 2: Configure Database Connection

Update the `appsettings.json` file with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TaskManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyMinimum32CharactersLong!",
    "Issuer": "TaskManagementAPI",
    "Audience": "TaskManagementAPIUsers",
    "ExpiryInHours": "1"
  }
}
```

### Step 3: Apply Database Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If you don't have EF Core tools installed:

```bash
dotnet tool install --global dotnet-ef
```

### Step 4: Run the Application

```bash
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`
- Swagger UI: `https://localhost:7xxx/swagger`

## 📚 API Endpoints

### Authentication

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| POST | `/api/auth/register` | Register new user | Public |
| POST | `/api/auth/login` | Login and get JWT token | Public |

### Users

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| GET | `/api/users` | Get all users | Manager |
| GET | `/api/users/{id}` | Get user by ID | Authenticated |

### Tasks

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| POST | `/api/tasks` | Create new task | Authenticated |
| GET | `/api/tasks` | Get all tasks | Authenticated |
| GET | `/api/tasks/{id}` | Get task by ID | Authenticated |
| PUT | `/api/tasks/{id}` | Update task | Manager |
| DELETE | `/api/tasks/{id}` | Delete task | Manager |
| PATCH | `/api/tasks/{id}/status` | Update task status | Task Owner |
| POST | `/api/tasks/{id}/updates` | Add task update | Employee (Task Owner) |
| GET | `/api/tasks/{id}/updates` | Get task updates | Authenticated |
| POST | `/api/tasks/{id}/reviews` | Add task review | Manager |
| GET | `/api/tasks/{id}/reviews` | Get task reviews | Authenticated |

### Reviews

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| PUT | `/api/reviews/{id}` | Update review | Manager |
| DELETE | `/api/reviews/{id}` | Delete review | Manager |

## 💡 Usage Examples

### 1. Register a New User

```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123",
  "role": "Employee"
}
```

### 2. Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePass123"
}
```

Response includes JWT token to be used in subsequent requests.

### 3. Create a Task (Manager)

```http
POST /api/tasks
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "title": "Complete Project Documentation",
  "description": "Write comprehensive documentation for the API",
  "assignedTo": 5,
  "dueDate": "2025-11-01T00:00:00"
}
```

### 4. Update Task Status (Employee)

```http
PATCH /api/tasks/1/status
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "status": "InProgress"
}
```

### 5. Add Task Update

```http
POST /api/tasks/1/updates
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "update_text": "Completed 50% of the documentation",
  "attachmentURL": "https://example.com/progress-report.pdf"
}
```

## 🔒 Security Notes

- Passwords are hashed using SHA-256 before storage
- JWT tokens are required for all protected endpoints
- Role-based authorization prevents unauthorized access
- CORS is configured (update for production use)
- HTTPS is enforced in production

---

# 📖 Detailed File Explanations

## Controllers

### **controller_1.cs - AuthController & UsersController**

**AuthController:**
- Handles user registration and authentication
- **Register endpoint**: Accepts user details (full name, email, password, role), validates input using ModelState, and delegates to `AuthService` for user creation and JWT generation
- **Login endpoint**: Validates credentials and returns JWT token if successful
- Both endpoints return standardized `ApiResponse` objects with success status, messages, and data/errors
- No authorization required (public endpoints)

**UsersController:**
- Manages user-related operations
- **GetAllUsers**: Manager-only endpoint that retrieves all users in the system
- **GetUserById**: Returns specific user details with role-based access control - employees can only view their own profile, managers can view anyone
- Uses claims-based authentication to extract user ID and role from JWT token
- Returns 403 Forbidden if an employee tries to access another user's profile

### **controller_2.cs - TasksController**

Comprehensive task management controller with multiple responsibilities:

**Task CRUD Operations:**
- **CreateTask**: Allows managers to assign tasks to anyone, employees can only create tasks for themselves. Validates assignment and returns 403 if employee tries to assign to others
- **GetAllTasks**: Returns filtered results - managers see all tasks, employees see only their assigned tasks
- **GetTaskById**: Retrieves specific task with permission checks
- **UpdateTask**: Manager-only endpoint for full task modification (title, description, assignment, due date, status)
- **DeleteTask**: Manager-only endpoint for task removal

**Task Status Management:**
- **UpdateTaskStatus**: Allows task owners (employees) to update status of their own tasks (Pending → InProgress → Done)
- Validates status values and ownership before updating

**Task Updates:**
- **AddTaskUpdate**: Employee-only endpoint for adding progress updates to their tasks
- **GetTaskUpdates**: Returns chronological list of updates for a task

**Task Reviews:**
- **AddTaskReview**: Manager-only endpoint for reviewing completed tasks
- **GetTaskReviews**: Returns all reviews for a specific task
- Reviews can only be added to tasks with "Done" status

All endpoints use `ClaimTypes` to extract authenticated user information and enforce role-based access control.

### **controller_3.cs - ReviewsController**

Dedicated controller for managing task reviews (Manager-only access):

- **UpdateReview**: Allows managers to modify existing review ratings and comments
- **DeleteReview**: Enables managers to remove reviews
- All endpoints validate review existence and return appropriate error responses
- Separate from TasksController for better organization and single responsibility

## Services

### **services_1.cs - AuthService & UserService**

**AuthService:**
Core authentication logic including:

- **RegisterAsync**: 
  - Validates role (must be "Employee" or "Manager")
  - Checks for duplicate email addresses
  - Hashes passwords using SHA-256
  - Creates user in database
  - Generates JWT token immediately
  - Returns login response with token

- **LoginAsync**:
  - Retrieves user by email
  - Verifies password hash
  - Generates new JWT token on successful authentication

- **Password Hashing**:
  - Uses SHA-256 for password hashing (Note: In production, consider using bcrypt or Argon2 for better security)
  - Compares hashed values for verification

- **JWT Token Generation**:
  - Creates tokens with user ID, email, name, and role claims
  - Configures issuer and audience from configuration
  - Uses HMAC-SHA256 signature algorithm
  - Token expiry is commented out (should be uncommented for production)

**UserService:**
Business logic for user management:

- **GetAllUsersAsync**: Retrieves all users and maps to DTOs (excludes sensitive data like passwords)
- **GetUserByIdAsync**: Fetches specific user, returns error if not found
- Both methods return standardized `ApiResponse` with success indicators

### **services_2.cs - TaskService**

Comprehensive task management business logic:

**CreateTaskAsync**:
- Validates that assigned user exists
- Sets initial status to "Pending"
- Records creator ID
- Returns complete task DTO with creator and assignee names

**GetAllTasksAsync**:
- Implements role-based filtering
- Managers retrieve all tasks
- Employees only get their assigned tasks
- Returns list of task DTOs

**GetTaskByIdAsync**:
- Retrieves specific task
- Enforces permission check - employees can only view their assigned tasks
- Returns detailed task information

**UpdateTaskAsync**:
- Manager-only operation (enforced in controller)
- Validates assigned user exists
- Validates status value against allowed options
- Updates all task properties including reassignment

**DeleteTaskAsync**:
- Removes task from database
- Returns boolean success indicator

**UpdateTaskStatusAsync**:
- Validates status value (Pending, InProgress, Done)
- Verifies user owns the task
- Employees can only update their own tasks
- Uses specialized repository method for status updates

**Helper Methods**:
- `IsValidStatus`: Validates status strings
- `MapToTaskDto`: Converts entity to DTO with navigation property names (Creator, AssignedUser)

### **services_3.cs - TaskUpdateService & TaskReviewService**

**TaskUpdateService:**
Manages task progress updates:

- **CreateTaskUpdateAsync**:
  - Validates task exists
  - Ensures user is assigned to the task
  - Creates update with text and optional attachment URL
  - Returns DTO with updater's name

- **GetTaskUpdatesAsync**:
  - Retrieves all updates for a task
  - Orders by date (newest first)
  - Returns list with user information

**TaskReviewService:**
Manages task reviews by managers:

- **CreateTaskReviewAsync**:
  - Validates task exists
  - **Critical check**: Only allows reviews for tasks with "Done" status
  - Creates review with rating (1-5) and comments
  - Records reviewer ID

- **GetTaskReviewsAsync**:
  - Fetches all reviews for a task
  - Includes reviewer information
  - Ordered by date

- **UpdateTaskReviewAsync**:
  - Allows modification of existing reviews
  - Updates rating and comments
  - Maintains original reviewer and date

- **DeleteTaskReviewAsync**:
  - Removes review from database
  - Returns success status

Both services include mapping methods to convert entities to DTOs with populated navigation properties.

## Repositories

### **Repositories.cs - Data Access Layer**

**UserRepository:**
- **GetByIdAsync**: Direct lookup by primary key
- **GetByEmailAsync**: Finds user by email (used for login)
- **GetAllAsync**: Returns all users
- **CreateAsync**: Adds new user and saves to database
- **EmailExistsAsync**: Checks for duplicate emails during registration

**TaskRepository:**
- **GetByIdAsync**: Retrieves task with Creator and AssignedUser navigation properties loaded
- **GetAllAsync**: Returns all tasks with related user data
- **GetByUserIdAsync**: Filters tasks by assigned user (for employee view)
- **CreateAsync**: Adds task and returns complete entity with navigations
- **UpdateAsync**: Modifies existing task
- **DeleteAsync**: Removes task, returns false if not found
- **UpdateStatusAsync**: Specialized method for status-only updates (more efficient)

All task methods use `.Include()` to eagerly load navigation properties for better performance.

**TaskUpdateRepository:**
- **GetByIdAsync**: Retrieves update with User navigation
- **GetByTaskIdAsync**: Gets all updates for a task, ordered by date (descending)
- **CreateAsync**: Adds new update with user information

**TaskReviewRepository:**
- **GetByIdAsync**: Fetches review with Reviewer navigation
- **GetByTaskIdAsync**: Gets all reviews for a task, ordered by date
- **CreateAsync**: Adds new review
- **UpdateAsync**: Modifies existing review
- **DeleteAsync**: Removes review

All repositories follow the repository pattern with async operations and proper entity tracking.

## Models & DTOs

### **models.cs - Database Entities**

**User Entity:**
- Primary key: UserID
- Properties: FullName, Email, Password (hashed), Role, CreatedAt
- Navigation properties for all relationships (CreatedTasks, AssignedTasks, TaskUpdates, TaskReviews)
- Email has unique index constraint
- Role constrained to 20 characters (Employee/Manager)

**Task Entity:**
- Primary key: TaskID
- Properties: Title, Description, Status, CreatedAt, DueDate
- Foreign keys: CreatedBy, AssignedTo
- Default status: "Pending"
- Navigation properties: Creator, AssignedUser, TaskUpdates, TaskReviews
- Supports nullable DueDate

**TaskUpdate Entity:**
- Primary key: UpdateID
- Links to Task and User (UpdatedBy)
- Properties: Update_text, AttachmentURL, UpdateDate
- Stores progress notes and optional attachments
- Auto-set UpdateDate on creation

**TaskReview Entity:**
- Primary key: ReviewID
- Links to Task and User (ReviewedBy)
- Properties: Comments, Rating (1-5), ReviewDate
- Rating validation using Range attribute
- Used for performance evaluation

### **dto.cs - Data Transfer Objects**

**Authentication DTOs:**
- **RegisterDto**: Input for user registration with validation attributes
- **LoginDto**: Simple email/password input
- **LoginResponseDto**: Returns user info + JWT token (no password)

**User DTOs:**
- **UserDto**: Public user information without sensitive data

**Task DTOs:**
- **CreateTaskDto**: Input for new tasks (no status, set automatically)
- **UpdateTaskDto**: Full task modification including status
- **UpdateTaskStatusDto**: Status-only update
- **TaskDto**: Complete task information with creator and assignee names

**Task Update DTOs:**
- **CreateTaskUpdateDto**: Input for progress updates
- **TaskUpdateDto**: Complete update info with user names

**Task Review DTOs:**
- **CreateTaskReviewDto**: Input for reviews
- **UpdateTaskReviewDto**: Modification input
- **TaskReviewDto**: Complete review with reviewer name

**ApiResponse<T>:**
- Generic wrapper for all API responses
- Properties: Success, Message, Data, Errors
- Provides consistent response structure across all endpoints
- Supports validation error lists

## Configuration Files

### **data.cs - ApplicationDbContext**

Entity Framework Core database context configuration:

**DbSet Properties:**
- Users, Tasks, TaskUpdates, TaskReviews

**OnModelCreating Configurations:**

**User Configuration:**
- Primary key setup
- Unique index on Email
- Default value for CreatedAt using SQL Server function

**Task Configuration:**
- Default status value ("Pending")
- Default CreatedAt timestamp
- **Cascade delete** for Creator relationship (if user deleted, their created tasks are deleted)
- **No action** for AssignedUser relationship (prevents cascade delete conflicts)

**TaskUpdate Configuration:**
- Default UpdateDate timestamp
- Cascade delete with Task (updates deleted when task is deleted)
- No action for User to prevent conflicts

**TaskReview Configuration:**
- Default ReviewDate timestamp
- Cascade delete with Task
- No action for Reviewer

### **Program.cs - Application Startup**

Complete application configuration:

**Service Registration:**
- Controllers and API explorer
- Swagger with JWT authentication support
- SQL Server DbContext with connection string
- JWT authentication configuration with validation parameters
- Repository pattern implementations (scoped lifetime)
- Service layer implementations (scoped lifetime)
- CORS policy allowing all origins (update for production)

**JWT Configuration:**
- Reads settings from appsettings.json
- Configures Bearer authentication scheme
- Sets up token validation parameters (issuer, audience, signing key)
- Enforces token lifetime validation with zero clock skew

**Swagger Configuration:**
- Adds Bearer token input to Swagger UI
- Enables API testing with authentication
- Security scheme configuration for authorization header

**Middleware Pipeline:**
- Swagger UI (development only)
- HTTPS redirection
- CORS middleware
- Authentication middleware (must come before authorization)
- Authorization middleware
- Controller routing

**Note:** The commented JWT token at the end appears to be a test token (should be removed in production)
