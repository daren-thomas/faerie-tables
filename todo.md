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
  - [x] `Table`, `TableColumn`, `TableRow`, `RowValue`, `Tag`, `TableTag`
- [x] **Create DTO classes** for table management (e.g., `TableDto`)
- [x] **Implement TableController** with:
  - [x] `GET /tables` (searchable)
  - [x] `GET /tables/{id}`
  - [x] `POST /tables`
  - [x] `PUT /tables/{id}`
  - [x] `DELETE /tables/{id}`
- [x] **Add basic validation** (e.g., require Title, validate `DiceRange`)
- [x] **Unit Tests** (service-level or EF repository tests)
- [x] **Integration Tests** (in-memory SQLite or test DB):
  - [x] Validate CRUD operations
  - [x] Confirm 404s for missing resources
  - [x] Confirm 400s for validation errors

**Validation Check:**
- [x] All TableController endpoints function as intended
- [x] All tests pass

---

## Chunk D: Rolling Logic

- [x] **Create RollingService** (or equivalent):
  - [x] Fetch table data
  - [x] Roll logic (row-based vs. column-based)
  - [x] Override merges
- [x] **Implement RollController**:
  - [x] `POST /rolls`:
    - [x] Accepts table ID, mode, overrides
    - [x] Calls RollingService and returns results
- [x] **Unit Tests** for RollingService:
  - [x] Random selection correctness
  - [x] Override logic
- [x] **Integration Tests** for RollController

**Validation Check:**
- [x] Rolling endpoints produce correct random results
- [x] Overrides are properly applied

---

## Chunk F: Blazor Frontend

1. **Infrastructure Setup**
   - [ ] Ensure `HttpClient` or appropriate service is set up for API calls

2. **Table Management Pages**
   - [x] `TablesList.razor`:
     - [x] Fetch `/tables`, display list
     - [x] Search box for fuzzy search
   - [x] `TableEditor.razor`:
     - [x] Display/edit table details (title, dice range, etc.)
     - [ ] Add/Edit/Delete columns and rows
     - [x] Calls relevant API endpoints

3. **Rolling Page**
   - [x] `RollPage.razor`:
     - [x] Table selection dropdown or search
     - [x] Mode selection (row vs. column)
     - [x] Random roll -> display results
     - [ ] Manual override UI -> calls override endpoints

Validation Check:**
 - [ ] All pages make successful API calls
 - [ ] Rolling results display correctly
 - [ ] Manual override updates are reflected in the results

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

