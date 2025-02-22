Below is a structured plan that starts at a high level, then breaks the project down into progressively smaller steps. Finally, you’ll see a series of prompts (in separate code blocks) that you could feed into a code-generation LLM to implement the project in a test-driven, iterative manner. The plan ensures incremental progress, early testing, and no orphaned code.

---

## **1. High-Level Blueprint**

1. **Initialize the Project:**
    
    - Create a solution with two projects:
        - **FaerieTables.Web** (Blazor UI)
        - **FaerieTables.Api** (ASP.NET Core Web API, EF Core)
    - Establish a shared **FaerieTables.Core** library if needed for domain models and service logic.
2. **Data Modeling & Database Setup:**
    
    - Define entities:
        - Table, TableColumn, TableRow, RowValue, Tag, TableTag, Session, Roll, RollResult
    - Configure Entity Framework Core with SQLite.
    - Create migrations and initialize the database.
3. **API Endpoints:**
    
    - **Table Management:** CRUD for tables and related columns/rows/tags.
    - **Rolling:** Two modes (row vs. column) plus manual override (log merges).
    - **Session Logging:** Retrieve, export, and clear logs.
4. **Blazor Frontend:**
    
    - **Table Management UI:** Create/import/edit tables.
    - **Roll UI:** Fuzzy search -> roll -> log results.
    - **Session Log UI:** Display, export, and clear session logs.
5. **Testing:**
    
    - **Unit Tests:** Validate logic for rolling, merging logs, data validations.
    - **Integration Tests:** API endpoints with an in-memory DB.
    - **UI Tests:** Basic Blazor component tests.
6. **Containerization (Docker):**
    
    - Create a Dockerfile.
    - Test container build and runtime.
7. **Documentation & Final Checks:**
    
    - Add OpenAPI/Swagger for the API.
    - Review logs, error handling, and finalize the solution.

---

## **2. Break the Blueprint into Iterative Chunks**

1. **Chunk A: Core Solution Setup**
    
    - Initialize solution, projects, .gitignore, basic folders.
    - Install initial NuGet packages (ASP.NET Core, EF Core, etc.).
    - Set up a minimal “Hello World” Web API and Blazor front page.
2. **Chunk B: Data Modeling & Persistence**
    
    - Create EF Core entities for Tables, Columns, Rows, RowValues, etc.
    - Configure relationships in `DbContext`.
    - Migrate schema to SQLite, confirm database connectivity.
3. **Chunk C: Table Management API**
    
    - Implement endpoints for creating, reading, updating, and deleting tables.
    - Include basic validation (title required, etc.).
    - Write unit tests for the data layer and integration tests for the endpoints.
4. **Chunk D: Rolling Logic**
    
    - Implement random roll logic (row-based vs. column-based).
    - Add a manual override mechanic (log merging).
    - Write tests (unit + integration) to confirm correct rolling results.
5. **Chunk E: Session Logging**
    
    - Create session entity and endpoints to start/end sessions.
    - Implement the roll log, exporting to Markdown, clearing logs, etc.
    - Write integration tests for session logs.
6. **Chunk F: Blazor Frontend**
    
    - Fuzzy search for tables and basic table-management screens.
    - Rolling UI with session log display.
    - Add triggers for manual overrides, log updates.
7. **Chunk G: Docker Container & Deployment**
    
    - Create a Dockerfile that builds the solution.
    - Verify container runs with EF Core migrations and the Blazor UI accessible.
8. **Chunk H: Final Integration & Polishing**
    
    - Ensure all endpoints and UI work seamlessly.
    - Add OpenAPI/Swagger documentation.
    - Final test pass and code cleanup.

---

## **3. Break Each Chunk Into Even Smaller Steps**

Below, each chunk is broken down into its constituent steps for clarity.

### **Chunk A: Core Solution Setup**

1. **Create a new solution** using the `dotnet new sln` command.
2. **Create the FaerieTables.Api** project (ASP.NET Core Web API).
3. **Create the FaerieTables.Web** project (Blazor Server or WebAssembly, as preferred).
4. **Optionally create FaerieTables.Core** for shared domain logic.
5. **Reference projects** in the solution.
6. **Set up .gitignore** (for .NET, optional Docker, etc.).
7. **Run a “Hello World”** endpoint in the API to confirm everything starts.

### **Chunk B: Data Modeling & Persistence**

1. **Install EF Core** packages (Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, etc.).
2. **Define entity classes** (Table, TableColumn, etc.) in FaerieTables.Core or the Api project’s Models folder.
3. **Create DbContext** (e.g., `RandomTableContext`) and configure relationships in `OnModelCreating`.
4. **Add migration** with `dotnet ef migrations add InitialCreate`.
5. **Update the database** with `dotnet ef database update`.
6. **Verify** the SQLite file is created and the schema matches your model.

### **Chunk C: Table Management API**

1. **Define DTO classes** (e.g., `TableDto`) for create/update.
2. **Implement TableController** with endpoints:
    - `GET /tables`
    - `GET /tables/{id}`
    - `POST /tables`
    - `PUT /tables/{id}`
    - `DELETE /tables/{id}`
3. **Add basic validation** with data annotations or FluentValidation.
4. **Write Unit Tests** for the service layer (if you have a separate service).
5. **Write Integration Tests** calling the actual endpoints with a test DB.

### **Chunk D: Rolling Logic**

1. **Create a RollingService** (or a set of methods) that:
    - Retrieves the table and relevant columns/rows.
    - Randomly selects a row (row-based).
    - Randomly selects values for each column (column-based).
2. **Integrate overrides**:
    - If an override is specified, that value replaces the random result for a given column.
3. **Add endpoints** in a `RollController`:
    - `POST /rolls` for random roll execution.
    - `POST /rolls/manual` for manual override logging.
4. **Write tests** for random distribution correctness, override merges, etc.

### **Chunk E: Session Logging**

1. **Create Session, Roll, and RollResult** entities if not already done.
2. **Add a `SessionController`** with endpoints to start a session, get logs, clear logs, export logs, etc.
3. **Link `Roll` entries** to a session ID in the `POST /rolls` endpoint or via a parameter.
4. **Implement Markdown export** with a small method that formats logs.
5. **Write tests** ensuring logs are created, exported, and cleared as expected.

### **Chunk F: Blazor Frontend**

1. **Set up minimal routing and pages** (e.g., `Index.razor`, `Tables.razor`).
2. **Add a Table list component** with fuzzy search (fetch from `GET /tables`).
3. **Create a detail/edit component** for tables, hooking into the TableController endpoints.
4. **Implement a Rolling page**:
    - Let users pick a table from a search or list.
    - Show roll results.
    - Store results in a session log displayed at the bottom of the page.
5. **Implement manual override** UI for columns.
6. **Implement log export** and clearing.

### **Chunk G: Docker Container & Deployment**

1. **Write a Dockerfile** that:
    - Copies the solution.
    - Restores and builds in release mode.
    - Publishes and sets the entry point to the .NET assembly.
2. **Build the image** and run a container.
3. **Test** that the containerized app can connect to SQLite and serve the Blazor UI.

### **Chunk H: Final Integration & Polishing**

1. **Add OpenAPI/Swagger** for your API (Swashbuckle, etc.).
2. **Ensure** unit and integration tests pass.
3. **Review** logs, error handling, final code style cleanup.
4. **Tag** the final version and produce an artifact or published Docker image.

---

## **4. Review for Step Size and Testing**

- Each chunk is granular enough to allow incremental development and testing.
- Each step within a chunk is self-contained (no big leaps of logic).
- Testing is introduced early and grows with functionality.

---

## **5. Series of Prompts for a Code-Generation LLM**

Below is a sample set of prompts you could use to guide a code-generation LLM (e.g., GitHub Copilot Chat, ChatGPT with a plugin, etc.). Each prompt is designed to implement and test a specific step. You would feed the LLM the _entire conversation context_ plus each prompt in sequence.

> **Note:** You will likely refine the prompts further based on your specific environment and naming conventions. These serve as starting points illustrating structure, incremental progress, and the inclusion of testing.

---

### **Prompt 1: Create the Core Solution**

```text
You are a code-generation assistant. Please create a new .NET solution named "FaerieTables." 
Inside that solution, create two projects:

1. A Blazor Server (or WASM with ASP.NET Core Hosted) project named "FaerieTables.Web."
2. An ASP.NET Core Web API project named "FaerieTables.Api."

Initialize them so that:
- The API project has a simple "Hello World" endpoint at "/api/hello".
- The Blazor project has a default Index page that displays "Hello Faerie Tables".

Make sure they reference each other as needed if using a shared library. Also create a .gitignore for .NET and any standard files we don't want checked in (e.g., bin, obj).
```

---

### **Prompt 2: Set Up EF Core with SQLite**

```text
Add Entity Framework Core to the FaerieTables.Api project. 
- Install Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, and Microsoft.EntityFrameworkCore.Design via NuGet.
- Create a DbContext class named RandomTableContext with no entities yet. 
- Configure it to use SQLite with a connection string in appsettings.json. 
- Add the required code in Program.cs to ensure the DbContext is registered.
- Create a test migration named "InitialCreate" and apply it so that it successfully generates a new SQLite database file.

Include brief instructions on how to run migrations, e.g., "dotnet ef migrations add" and "dotnet ef database update."
```

---

### **Prompt 3: Define Entities and Relationships**

```text
Now, define the following entities in the FaerieTables.Api project (or a FaerieTables.Core library if you prefer):

- Table (Id GUID, Title string, Source string, License string, Description string, DiceRange string)
- TableColumn (Id GUID, TableId GUID, Name string, Type string)
- TableRow (Id GUID, TableId GUID)
- RowValue (Id GUID, RowId GUID, ColumnId GUID, Value string)
- Tag (Id GUID, Name string)
- TableTag (TableId GUID, TagId GUID) // many-to-many
- Session (SessionId GUID, UserId string, Name string, Description string)
- Roll (RollId GUID, SessionId GUID, TableId GUID, TableTitle string, Mode string, Timestamp DateTime)
- RollResult (Id GUID, RollId GUID, TableColumnId GUID, Value string)

Use EF Core fluent API in OnModelCreating to set up relationships:
- One Table has many TableColumns, many TableRows, and many-to-many Tags.
- One TableRow has many RowValues.
- One Session can have many Rolls.
- One Roll can have many RollResults.

Finally, create a migration named "AddCoreEntities" and apply it, verifying the database is updated. Provide a code snippet with the entity definitions and the context configuration.
```

---

### **Prompt 4: Table Management Endpoints**

```text
Implement the TableController with endpoints for:
- GET /tables (returns all tables or filtered by optional 'search' query param)
- GET /tables/{id}
- POST /tables
- PUT /tables/{id}
- DELETE /tables/{id}

Use a TableDto for creating/updating. Include minimal validation, such as requiring Title. 
Store and retrieve data through EF Core. 
Write basic unit tests for any service methods that handle table logic (if you have a separate service). 
Then write integration tests for the controller using the in-memory SQLite approach.

Provide the final TableController code, TableDto, any validation attributes, and the test files.
```

---

### **Prompt 5: Implement Rolling Logic**

```text
1. Create a service class (e.g., RollingService) that:
   - Given a TableId and a mode ("row" or "column"), fetches the table and performs a random selection.
   - Row mode: pick one row at random.
   - Column mode: for each column, pick a random row’s value for that column.
2. Add a 'POST /rolls' endpoint in a RollController that:
   - Accepts a JSON body with { "table_id": "...", "session_id": "...", "mode": "row or column", "overrides": [ { "column": "...", "value": "..." } ] }.
   - Calls the RollingService, applies overrides, and persists a Roll and the associated RollResult records.
3. Return the roll results as JSON.
4. Write unit tests for RollingService to confirm random distribution is correct, override logic is correct, etc. Then write integration tests for the RollController.

Provide the code for RollingService, the RollController, test cases, and any updates to the DbContext.
```

---

### **Prompt 6: Session and Logging**

```text
1. Implement a SessionController that:
   - Allows creating a session (POST /sessions) with a name/description.
   - Retrieves session info by ID.
   - Retrieves all Rolls associated with a session (GET /sessions/{sessionId}/rolls).
2. In the RollController, ensure that each roll is associated with the session. 
3. Implement an endpoint to export a session’s rolls as Markdown, e.g., GET /sessions/{sessionId}/export. 
4. Implement an endpoint to clear a session’s roll log, e.g., DELETE /sessions/{sessionId}/rolls.
5. Write integration tests to confirm that:
   - Rolls are associated with a session.
   - The Markdown export includes the correct table references and values.
   - Clearing the session’s rolls works properly.

Return the final code for SessionController, updates to RollController, plus the tests.
```

---

### **Prompt 7: Blazor Frontend – Basic UI**

```text
Add the following pages/components to the Blazor app:

1. TablesList.razor
   - Fetches tables from GET /tables
   - Displays them, including a search box for fuzzy searching by title/description
   - Allows navigating to edit/view a specific table

2. TableEditor.razor
   - Displays the table details (title, dice range, etc.) and columns/rows
   - Allows adding/editing rows and columns
   - Calls the appropriate POST/PUT/DELETE endpoints

3. RollPage.razor
   - Lets the user select a table (via a dropdown or search component) 
   - Offers two modes: row or column
   - On roll, calls the POST /rolls endpoint
   - Displays the results
   - Displays a session log at the bottom (if we have a session loaded) and a button to override rolls 
   - Possibly a manual override button or inline override UI

4. Session management components for viewing logs, exporting logs to Markdown, clearing logs, etc.

Include minimal styling and route definitions. Provide the code for these components or a stub for each so that we can build upon them. Show how you're making HTTP calls from Blazor to the API.
```

---

### **Prompt 8: Dockerization**

```text
1. Create a Dockerfile in the solution root that:
   - Copies the .sln and .csproj files
   - Restores dependencies
   - Publishes in Release mode
   - Exposes the necessary port
   - Uses 'aspnet:7.0' or appropriate base image

2. Update the .csproj or solution so that the publish build includes both the Web and Api components. 
3. Confirm that the SQLite database is created or updated at startup. If needed, call "dotnet ef database update" in the Docker build or entrypoint.

Finally, show how to build and run the container with:
   docker build -t faerie-tables .
   docker run -p 8080:80 faerie-tables

Provide the Dockerfile, any changes in Program.cs for DB migrations, and testing instructions.
```

---

### **Prompt 9: Final Polishing and Integration**

```text
We are wrapping up. Please:

1. Add Swagger/OpenAPI support to the FaerieTables.Api project using Swashbuckle or similar. 
2. Ensure all controllers are annotated with appropriate route and Swagger doc attributes. 
3. Verify all integration tests pass and that the Blazor UI can consume these endpoints properly. 
4. Perform any final code cleanup: remove unnecessary logs, ensure consistent naming and style, etc.

Provide the updated Program.cs, controller annotations, and a final summary of changes. Confirm that the final Docker image runs as expected, with a note on how to access the Swagger UI inside the container (e.g., http://localhost:8080/swagger).
```

---

## **Conclusion**

By following these progressively refined steps—and feeding each prompt in turn to a code-generation tool—you’ll be able to implement _Faerie-Tables_ incrementally with robust testing at each stage. Each prompt is self-contained, ensuring no “orphaned code” remains unintegrated. You can adjust prompt wording or complexity as needed, but the overall approach remains the same: **small, test-driven increments that build on previous steps, culminating in a fully functional and containerized solution.**