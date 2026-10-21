# Database

Schema lives in [`InitSchema.sql`](../Database/InitSchema.sql) (source of
truth). Column-level rationale is documented as comments in that file.

## Conventions
- Dates are `TEXT`. Date-only columns use `YYYY-MM-DD` (`date()` CHECK);
  timestamps use `YYYY-MM-DD HH:MM:SS` in UTC (`datetime()` CHECK, see
  [Timestamps](#timestamps)).
- Soft delete: rows with `DeletedAt IS NOT NULL` are hidden by a global query
  filter.
- Dates are `TEXT`. Date-only columns use `YYYY-MM-DD` (`date()` CHECK);
  timestamps use `YYYY-MM-DD HH:MM:SS` in UTC (`datetime()` CHECK, see
  [Timestamps](#timestamps)).
- All tables are `STRICT`, so SQLite enforces column types.
- Foreign keys are enforced (`PRAGMA foreign_keys = ON`).
- Boolean flags are `INTEGER` with a `CHECK (... IN (0, 1))`.
- Primary keys are `INTEGER PRIMARY KEY AUTOINCREMENT`.
- Many-to-many relationships use a junction table with its own `Id` and a
  `UNIQUE` pair of foreign keys.

## ER diagram
<!-- (mermaid erDiagram here) -->

## Tables

### Lookup tables
- **SkillCategory**: a group of skills (e.g. Languages, Frameworks, Tools). One
  category has many `Skill`.
- **ProjectType**: the kind of a project (e.g. Personal, Commercial, Open
  Source). One type has many `Project`.

### Core content
These are the reusable pieces the user keeps in their library and picks from
when building a resume.

- **Profile**: personal identity and contact info: `FullName` (required),
  `Email`, `PhoneNumber`, `LinkedInUsername`, `GitHubUsername`, `PortfolioUrl`,
  `DateOfBirth` (all optional).
- **PersonalSummary**: a professional summary tailored to a target role:
  `TargetPositionTitle` (required) and `Summary` (optional text).
- **WorkExperience**: a job the user held: `CompanyName`, `PositionTitle`,
  `StartDate` (required), `Description`, `EndDate` (optional). A `NULL`
  `EndDate` means the job is ongoing. Can own many `Project`.
- **Education**: a school or study period: `SchoolName` (required), `Degree`,
  `Major`, `StartDate`, `EndDate` (optional).
- **Language**: a spoken language with its `ProficiencyLevel` (both required).
- **Certification**: a credential: `Name` (required), `Issuer`, `IssueDate`,
  `ExpirationDate` (optional).
- **Project**: a project the user worked on: `Name` (required), `Brief`, `Role`,
  `Responsibilities`, `RepositoryUrl`, `DemoUrl`, `StartDate`, `EndDate`
  (optional). `HasGaps` flags a project with incomplete information. Each
  project has exactly one `ProjectType` (required) and optionally belongs to
  a `WorkExperience` (a `NULL` `WorkExperienceId` means a personal or standalone
  project).
- **Skill**: a single skill, belonging to one `SkillCategory` (required).
  `IsHighlight` marks a skill to emphasize on a resume.

### Resume
- **Resume**: a generated document: `Name` (required) and `ExportedAt` (`NULL`
  until first exported to PDF). It holds no content itself. Its content is
  defined by the junction tables below.
- Exporting does not change UpdatedAt

### Junction tables
Each row means "this item is included in this resume". Each pair is unique, so
an item can appear in a given resume at most once.

| Table                   | Links                          | Relationship |
| ----------------------- | ------------------------------ | ------------ |
| `ResumeProfile`         | `Resume` <-> `Profile`         | many-to-many |
| `ResumePersonalSummary` | `Resume` <-> `PersonalSummary` | many-to-many |
| `ResumeWorkExperience`  | `Resume` <-> `WorkExperience`  | many-to-many |
| `ResumeEducation`       | `Resume` <-> `Education`       | many-to-many |
| `ResumeLanguage`        | `Resume` <-> `Language`        | many-to-many |
| `ResumeCertification`   | `Resume` <-> `Certification`   | many-to-many |
| `ProjectSkill`          | `Project` <-> `Skill`          | many-to-many |

Projects and skills are not linked to a resume directly. A resume includes a
project through its work experience, and a skill through the projects that use
it.

## Timestamps

Every entity table has `CreatedAt`, `UpdatedAt` and `DeletedAt` (junction tables
have none). `Resume` also has `ExportedAt`.

- **Format**: UTC, `YYYY-MM-DD HH:MM:SS`, exactly what SQLite's `datetime('now')`
  returns. A CHECK on every timestamp column (`CreatedAt`, `UpdatedAt`,
  `DeletedAt`, and `Resume.ExportedAt`) rejects anything else: a `T` separator,
  fractional seconds, or a timezone offset. The fixed format is what lets
  timestamps be compared as plain strings.
- **`CreatedAt`**: set once, on insert (`DEFAULT datetime('now')`).
- **`UpdatedAt`**: the last time the user edited the row's *content*. On insert
  it equals `CreatedAt`.
- **Lifecycle events do not touch `UpdatedAt`**: soft delete, restore, and
  export (`Resume.ExportedAt`). They record that something happened to the row,
  not that its content changed. If content and a lifecycle column change in the
  same save, `UpdatedAt` is updated.
- **Not enforced by the schema.** `DEFAULT` only provides the initial value.
  `UpdatedAt` is maintained in `DbContext.SaveChanges` (`ApplyUpdatedAt`).
- **Resume content changes**: junction tables have no timestamps, so ticking or
  unticking an item in a resume is recorded by the service layer bumping
  `Resume.UpdatedAt`.
- **Ordering invariant**: `CreatedAt <= UpdatedAt <= DeletedAt` for deleted
  rows, and `CreatedAt <= ExportedAt`. The schema does not check this; the seed
  data must satisfy it (see the header of `PopulateSeedData.sql`).

## Database Management

All database operations go through a single script:

```powershell
.\Scripts\Invoke-Database.ps1 -Action <Action> [-Environment <Environment>]
```

### Parameters

| Parameter      | Required | Default       |
| -------------- | -------- | ------------- |
| `-Action`      | No       | -             |
| `-Environment` | No       | `Development` |

### Actions

| Action                  | Description                                  |
|-------------------------|--------------------------------------------- |
| `Initialize`            | Creates the SQLite database from the schema. |
| `Remove`                | Deletes the database.                        |
| `Reset`                 | Removes and re-initializes the database.     |
| `RestoreData`           | Restores seed/backup data into the database. |
| `ExportInsertTemplates` | Exports `INSERT` templates for the tables.   |

### Environments

| Environment   | Description |
| ------------- | ----------- |
| `Development` | Development |
| `Testing`     | Testing     |
| `Staging`     | Staging     |
| `Production`  | Production  |

### Examples

Display an UI to manage database (Development is the default environment):

```powershell
.\Scripts\Invoke-Database.ps1
```

Initializing database (Development is the default environment):

```powershell
.\Scripts\Invoke-Database.ps1 -Action Initialize
```

Reset the Testing database:

```powershell
.\Scripts\Invoke-Database.ps1 -Action Reset -Environment Testing
```

Restore data in Staging:

```powershell
.\Scripts\Invoke-Database.ps1 -Action RestoreData -Environment Staging
```

### Notes

- Run the script from the repository root.
- `Remove` and `Reset` are destructive. Data in the target environment is lost.
- Be careful with `-Environment Production`, since it targets real data.

## Regenerating EF models

### Examples

Regenerating EF models (Development is the default environment):

```powershell
.\Scripts\Invoke-Scaffold.ps1
```

> Note: `Invoke-Scaffold` wraps `Invoke-Database`.
