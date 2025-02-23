# Faerie Tables TODO Checklist

A comprehensive, step-by-step list to ensure incremental progress and thorough testing.

---

## Chunk A: Core Solution Setup

- [x] **Create a new .NET solution** named `FaerieTables`
  - [x] Use `dotnet new sln`
- [x] **Create FaerieTables.Api** (ASP.NET Core Web API)
  - [x] Add a minimal “Hello World” endpoint at `/api/hello`
- [x] **Create FaerieTables.Web** (Blazor Server or WASM + ASP.NET Hosted)
  - [x] Have a default `Index` page showing `"Hello Faerie Tables"`
- [x] **(Optional) Create FaerieTables.Core** for domain logic (shared library)
  - [x] Reference it from both `FaerieTables.Api` and `FaerieTables.Web`
- [x] **Add .gitignore** for .NET, Docker, etc.

**Validation Check:**
- [x] Confirm solution builds successfully
- [x] Confirm “Hello World” endpoint is reachable
- [x] Confirm Blazor page displays

---

## Chunk B: Data Modeling & Persistence

- [x] **Install EF Core** packages in `FaerieTables.Api`:
  - [x] `Microsoft.EntityFrameworkCore`
  - [x] `Microsoft.EntityFrameworkCore.Sqlite`
  - [x] `Microsoft.EntityFrameworkCore.Design`
- [x] **Create `RandomTableContext`** (`DbContext`) in `FaerieTables.Api`
  - [x] Register it in `Program.cs` with the SQLite connection string
- [x] **Generate Migrations**:
  - [x] `dotnet ef migrations add InitialCreate`
  - [x] `dotnet ef database update`
- [x] **Verify** the SQLite database file is created

**Validation Check:**
- [x] Confirm no errors during migration
- [x] Confirm a functioning DBContext

---

## Chunk C: Table Management API

- [x] **Define Entities** (if not done in a separate library):
  - [x] `Table`, `TableColumn`, `TableRow`, `RowValue`, `Tag`, `TableTag`, `Session`, `Roll`, `RollResult`
- [ ] **Create DTO classes** for table management (e.g., `TableDto`)
- [ ] **Implement TableController** with:
  - [ ] `GET /tables` (searchable)
  - [ ] `GET /tables/{id}`
  - [ ] `POST /tables`
  - [ ] `PUT /tables/{id}`
  - [ ] `DELETE /tables/{id}`
- [ ] **Add basic validation** (e.g., require Title, validate `DiceRange`)
- [ ] **Unit Tests** (service-level or EF repository tests)
- [ ] **Integration Tests** (in-memory SQLite or test DB):
  - [ ] Validate CRUD operations
  - [ ] Confirm 404s for missing resources
  - [ ] Confirm 400s for validation errors

**Validation Check:**
- [ ] All TableController endpoints function as intended
- [ ] All tests pass

---

## Chunk D: Rolling Logic

- [ ] **Create RollingService** (or equivalent):
  - [ ] Fetch table data
  - [ ] Roll logic (row-based vs. column-based)
  - [ ] Override merges
- [ ] **Implement RollController**:
  - [ ] `POST /rolls`:
    - [ ] Accepts table ID, session ID, mode, overrides
    - [ ] Calls RollingService, persists Roll & RollResult
  - [ ] `POST /rolls/manual` (optional approach for manual-only rolls)
- [ ] **Unit Tests** for RollingService:
  - [ ] Random selection correctness
  - [ ] Override logic
- [ ] **Integration Tests** for RollController

**Validation Check:**
- [ ] Rolling endpoints produce correct random results
- [ ] Overrides are properly applied

---

## Chunk E: Session Logging

- [ ] **Create SessionController**:
  - [ ] `POST /sessions` to create a session
  - [ ] `GET /sessions/{sessionId}` to retrieve session details
  - [ ] `GET /sessions/{sessionId}/rolls` for all rolls in a session
  - [ ] `DELETE /sessions/{sessionId}/rolls` to clear logs
- [ ] **Link Rolls** to session ID
- [ ] **Markdown export** (e.g., `GET /sessions/{sessionId}/export`)
  - [ ] Format each roll as a bullet list item with table references
- [ ] **Integration Tests** for:
  - [ ] Session creation
  - [ ] Roll retrieval
  - [ ] Export to Markdown
  - [ ] Clearing logs

**Validation Check:**
- [ ] Session data is correctly persisted
- [ ] Markdown export is formatted properly
- [ ] Clearing logs works as intended

---

## Chunk F: Blazor Frontend

1. **Infrastructure Setup**
   - [ ] Ensure `HttpClient` or appropriate service is set up for API calls

2. **Table Management Pages**
   - [ ] `TablesList.razor`:
     - [ ] Fetch `/tables`, display list
     - [ ] Search box for fuzzy search
   - [ ] `TableEditor.razor`:
     - [ ] Display/edit table details (title, dice range, etc.)
     - [ ] Add/Edit/Delete columns and rows
     - [ ] Calls relevant API endpoints

3. **Rolling Page**
   - [ ] `RollPage.razor`:
     - [ ] Table selection dropdown or search
     - [ ] Mode selection (row vs. column)
     - [ ] Random roll -> display results
     - [ ] Manual override UI -> calls override endpoints

4. **Session Log UI**
   - [ ] Show current session’s rolls
   - [ ] Buttons to export as Markdown, clear logs

**Validation Check:**
- [ ] All pages make successful API calls
- [ ] Rolling results and logs display correctly
- [ ] Manual override updates are reflected in the log

---

## Chunk G: Docker Container & Deployment

- [ ] **Create a Dockerfile**:
  - [ ] Copy solution files
  - [ ] Restore dependencies
  - [ ] Publish in Release mode
  - [ ] Expose ports
  - [ ] Use an appropriate base image (e.g. `mcr.microsoft.com/dotnet/aspnet:7.0`)
- [ ] **Test Build & Run**:
  - [ ] `docker build -t faerie-tables .`
  - [ ] `docker run -p 8080:80 faerie-tables`
- [ ] **Check** that the SQLite DB is accessible inside the container
  - [ ] Migrations run at startup (if configured)

**Validation Check:**
- [ ] App is reachable at `http://localhost:8080`
- [ ] Verify the Blazor frontend and API are functional in the container

---

## Chunk H: Final Integration & Polishing

- [ ] **Add OpenAPI/Swagger**:
  - [ ] Use Swashbuckle or similar
  - [ ] Annotate controllers with [ApiController], routes, and doc comments
- [ ] **Run All Tests** (unit, integration, UI if applicable)
  - [ ] Confirm passing
- [ ] **Perform Final Cleanup**:
  - [ ] Remove debug logs
  - [ ] Ensure consistent naming conventions
  - [ ] Validate error handling is in place
  - [ ] Confirm code style
- [ ] **Docker Production Readiness**:
  - [ ] Validate environment variable overrides (if needed)
  - [ ] Possibly add CI/CD steps

**Validation Check:**
- [ ] Everything is documented (API doc, readme, or wiki)
- [ ] Docker image is final and can be deployed
- [ ] The solution is stable and meets requirements

---

## Final Notes

- Check off each item as you complete it.  
- Keep your tests current with new features.  
- Don’t forget to update the documentation (in-line code comments, README, or wiki) as you refine the solution.

