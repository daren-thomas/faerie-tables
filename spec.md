# Faerie-Tables: Random Table Manager Specification

This document outlines the requirements and design for a random table management tool (faerie-tables) intended for role-playing games. The tool supports managing random tables (including dice values and multi-column layouts), executing random or manual rolls, and maintaining a session log. The application is built using Blazor for the UI, an ASP.NET Core Web API for backend operations, Entity Framework Core with SQLite for data persistence, and is packaged as a Docker image for easy deployment.

## 1. Functional Requirements

### 1.1. Table Management

- **Import and Creation:**
    - Ability to create new random tables manually.
    - Import tables via an interchange format (YAML/JSON) that includes metadata (title, tags, source, license, description), dice range, column definitions, and rows.
    - Upload CSV files (via standard file upload) for import; conversion to the interchange format can be handled by an importer module.
- **Editing:**
    - Users can append or modify columns and rows.
    - Rows may include single values or ranges (e.g., "7-11"). Dice ranges must follow the “XdY” format (supporting shorthand like “d20”).
- **Categorization:**
    - Tables are categorized using a system of tags and table names.
- **Export:**
    - Tables can be exported in the interchange format.
    - Optionally, export as a markdown table (including extra data needed for integration with dice-roller plugins in Obsidian).

### 1.2. Rolling

- **Main Use Case:**
    - Look up a table via a fuzzy search (searching by title, description, and tags).
    - Roll on the table using two modes:
        - **Row Roll:** A single roll that randomly selects an entire row.
        - **Column Roll:** A separate roll for each column.
    - **Manual Overrides:**
        - Users can override a roll by specifying custom values for columns.

### 1.3. API-Driven Design

- **RESTful Endpoints** are provided for:
    - **Table Management:** CRUD operations for tables.
    - **Roll Execution:** Endpoints for executing rolls (random and manual overrides).
- **Authentication/Authorization:**
    - Not detailed in this spec, but should be integrated if multi-user support is required.

## 2. Architecture Overview

### 2.1. Technology Stack

- **Frontend:** Blazor (for interactive UI).
- **Backend:** ASP.NET Core Web API.
- **Data Persistence:** Entity Framework Core with SQLite.
- **Containerization:** Docker (application packaged as a Docker image).
- **Optional Libraries:**
    - AutoMapper for DTO mapping.
    - FluentValidation for input validation.

### 2.2. Deployment

- **Docker:**
    - A Dockerfile is provided to build the API/Blazor application.
    - Optionally, a Docker Compose file can be used for multi-service orchestration.
- **Environment Configuration:**
    - SQLite connection strings and other settings are handled via `appsettings.json` and environment variables.

## 3. Data Model & Database Schema

### 3.1. Entities and Relationships

#### Table Entity

- **Id (GUID):** Unique identifier.
- **Title, Source, License, Description, DiceRange (string).**
- **Relationships:**
    - One-to-many with **TableColumn**.
    - One-to-many with **TableRow**.
    - Many-to-many with **Tag** (via **TableTag**).

#### TableColumn Entity

- **Id, TableId, Name, Type (string).**

#### TableRow Entity

- **Id, TableId.**
- **Relationship:** One-to-many with **RowValue**.

#### RowValue Entity

- **Id, RowId, ColumnId, Value (string).**

#### Tag Entity

- **Id, Name (string).**

#### TableTag (Join Table)

- **TableId, TagId.**

### 3.2. Entity Framework Configuration

- **DbContext:**
    - Define `RandomTableContext` with DbSet properties for each entity.
    - Configure relationships and many-to-many join tables via Fluent API in `OnModelCreating`.
- **Migrations:**
    - Use `dotnet ef migrations add InitialCreate` and `dotnet ef database update` to manage schema.

## 4. API Endpoint Specifications

### 4.1. Table Management Endpoints

- **GET /tables**
    
    - **Query:** Optional fuzzy search (`search`), pagination (`page`, `limit`).
    - **Response:** List of tables (including metadata, dice_range, etc.).
- **GET /tables/{id}**
    
    - **Response:** Detailed table information with columns, rows (and row values), and tags.
    - **Errors:** Return 404 if not found.
- **POST /tables**
    
    - **Request:** JSON matching the interchange format with metadata and table structure.
    - **Validation:** Validate dice_range format and required fields.
    - **Response:** Created table with GUID.
    - **Errors:** 400 for validation errors.
- **PUT /tables/{id}**
    
    - **Request:** Updated table details.
    - **Response:** Updated table object.
    - **Errors:** 404 if not found, 400 for validation errors.
- **DELETE /tables/{id}**
    
    - **Response:** Confirmation message.
    - **Errors:** 404 if not found.

### 4.2. Roll Execution Endpoint

- **POST /rolls**

    - **Request:** `{ "table_id": "GUID", "mode": "row|column", "overrides": [ { "column": "ColumnName", "value": "CustomValue" } ] }`
    - **Process:**
        - Validate table existence.
        - In row mode, select one random row; in column mode, roll for each column.
        - Apply any manual overrides.
    - **Response:** Dictionary of column names to rolled values.
    - **Errors:** 400 for missing/invalid parameters, 404 if table not found.


## 5. Error Handling & Validation

- **Data Validation:**
    - Use Data Annotations and FluentValidation to ensure required fields are provided.
    - Validate dice_range against a regex (e.g., `^(\d*d\d+)$`).
- **API Responses:**
    - Use standard HTTP status codes (200, 201, 400, 404, 500).
    - Return structured error messages containing an error code and descriptive message.
- **Transactions:**
    - Wrap multi-step operations (like creating a roll and its results) in a transaction for atomicity.

## 6. Testing Plan

### 6.1. Unit Tests

- **Business Logic:**
    - Test random roll generation for both row and column modes.
    - Validate manual override logic.
- **Data Validation:**
    - Test input validation for creating/updating tables, ensuring invalid dice_range formats are rejected.

### 6.2. Integration Tests

- **API Endpoints:**
    - Test each endpoint (GET, POST, PUT, DELETE) using an in-memory SQLite database.
    - Verify that relationships (e.g., table to tags) are correctly persisted.

### 6.3. UI Tests (Blazor)

- **Component Testing:**
    - Test Blazor components for search functionality, table display, rolling buttons, and log updates.
- **End-to-End Tests:**
    - Simulate user workflows: search for a table, execute rolls (both random and manual), and verify the log is updated accordingly.

### 6.4. Docker & Deployment

- **Container Tests:**
    - Ensure the Docker image builds correctly.
    - Validate that the container starts and connects to the SQLite database.
    - Test environment variable overrides for production configuration.

## 7. Additional Considerations

- **Documentation:**
    - Include API documentation (Swagger/OpenAPI recommended) for ease of testing and integration.
- **Logging & Monitoring:**
    - Implement application logging to record errors and usage statistics.
- **Future Enhancements:**
    - Consider adding authentication if multiple users are supported.
    - Explore more advanced search (e.g., full-text search on table content) if required.