# ASP.NET Web Forms Skills Search Training

A training example based on a real internal ASP.NET Web Forms app: a staff "skills directory"
search page where users filter a staff list by technical/psychometric skills, skill level, and
testing program. This version keeps the same UI/interaction pattern but fixes several
anti-patterns found in the original code-behind, and extracts the business logic into a
separately buildable, separately unit-tested class library.

## Solution layout

```
SkillsSearch.Core/            Plain C# library: models, search filtering, SQL data access
SkillsSearch.Core.Tests/      xUnit tests for SkillsSearch.Core (buildable/testable with `dotnet test`)
WebApp/                       Classic ASP.NET Web Forms front-end (System.Web, .NET Framework 4.7.2)
```

`SkillsSearch.Core` targets `netstandard2.0` specifically so it can be referenced both by the
.NET Framework 4.7.2 Web Forms project and by modern `dotnet test`-based tooling -- one library,
two very different runtimes.

## What changed vs. the original app, and why

The original code-behind worked, but had a few patterns worth calling out as training material:

1. **A real internal SQL Server hostname was hardcoded, and duplicated in every method.**
   Every `Load_*` method in the original page built its own `connectionString` literal. Fixed by
   moving it to a single `<connectionStrings>` entry in `Web.config` (see `WebApp/Web.config`),
   read once via `ConfigurationManager`.

2. **Search filtering built a `DataView.RowFilter` expression by string-concatenating raw user
   input.** The original `UpdateQuery()` method assembled something like
   `"[Gen_Info] like '%" + tb_Search.Text + "%'"` directly from the search textbox and selected
   checkboxes. `DataView.RowFilter` has its own expression language (comparisons, functions,
   sub-selects), so this is the same class of bug as building SQL by string concatenation --
   unescaped input can change what the filter *does*, not just what it matches. Fixed in
   `SearchFilterBuilder`: it filters an in-memory list with plain LINQ/string comparisons, so
   there is no expression string built from user input at all. See
   `SearchFilterBuilderTests.MaliciousLookingKeyword_IsTreatedAsLiteralText_NotInjected` for a
   test that demonstrates the difference.

3. **`System.Windows.Forms.MessageBox.Show(...)` was used for error handling in a web app.**
   `MessageBox` is a desktop UI API; calling it during an ASP.NET request has no visible effect
   for the actual client and blocks the worker thread waiting on a dialog nobody can see or
   dismiss. Fixed by surfacing errors on the page (`lbl_Error`) and letting exceptions propagate
   to normal ASP.NET error handling/logging instead of being silently swallowed by an empty
   `catch { MessageBox.Show(...); }`.

4. **Business logic lived entirely in code-behind, with no way to test it without IIS/SQL
   Server.** Extracted into `SkillsSearch.Core`: `SearchFilterBuilder` (pure filtering logic),
   `ISkillsRepository`/`SqlSkillsRepository` (data access, using parameterized `SqlCommand`
   objects instead of ad hoc string SQL), and `InMemorySkillsRepository` (a fake for local/dev use
   and UI-layer testing). The `Core.Tests` project exercises `SearchFilterBuilder` directly.

5. **Connections weren't reliably closed on error.** The original called `connection.Close()`
   only on the success path inside a `try`; an exception before that line left the connection
   open until finalization. `SqlSkillsRepository` wraps every `SqlConnection`/`SqlCommand`/
   `SqlDataReader` in a `using` declaration instead.

6. One skill category was renamed from an internal-jargon name to something generic
   (`TechSkillCategories.InternallyDeveloped`, was "\<Company\> Developed Software" in the
   original) -- purely a naming change, no behavior difference.

## Running the tests

```bash
cd SkillsSearch.Core.Tests
dotnet test
```

This is the part of the repo CI actually builds/tests (`.github/workflows/tests.yml`) -- it only
needs the cross-platform .NET SDK.

## Running the web app

`WebApp` is classic ASP.NET Web Forms (`System.Web`), which needs **Visual Studio + IIS Express
or full IIS** to build and run -- the `dotnet` CLI can't build `System.Web` projects. To run it
locally:

1. Open `AspNetWebFormsSearchTraining.sln` in Visual Studio (2019+ with the ASP.NET workload).
2. Update the `SkillsDb` connection string in `WebApp/Web.config` to point at a real SQL Server
   instance with `TechSkills`, `PsychometricSkills`, `TestingPrograms`, `TestingProgramInfo`, and
   `tmp_Staff_Skills` tables (see `SqlSkillsRepository` for the exact columns expected), or swap
   `SqlSkillsRepository` for `InMemorySkillsRepository` with some sample data while you experiment.
3. Set `WebApp` as the startup project and run (F5) -- IIS Express will host it.

## Exercises

1. **Swap in `InMemorySkillsRepository` for local development.** Change `SkillsSearchPage.Repository`
   to build an `InMemorySkillsRepository` with a few sample staff members instead of
   `SqlSkillsRepository`, so you can run the whole UI without a database.
2. **Add a test for the testing-program dropdown.** `SearchFilterBuilderTests` covers most
   criteria already -- add a case combining a testing program filter *and* a skill filter to make
   sure both apply as an AND, not an OR.
3. **Make `SqlSkillsRepository` async.** Convert its methods to `OpenAsync`/`ExecuteReaderAsync`
   and discuss what has to change in the Web Forms code-behind to call an async method from
   `Page_Load` (Web Forms' async page model, `Async="true"` on the `@Page` directive, etc.).
4. **Add pagination.** `SearchFilterBuilder.Search` returns everything that matches; add a
   `Skip`/`Take`-based overload and wire it to `GridView`'s built-in paging.
5. **Try to reintroduce the original bug on purpose.** Write a `SearchFilterBuilderTests` case
   using a keyword containing a single quote or SQL-comment sequence, confirm it *doesn't* change
   query behavior here, then look at how you'd write the equivalent test against the *original*
   `DataView.RowFilter`-based code to prove the bug existed.
