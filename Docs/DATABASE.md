# Database

Schema lives in [`InitSchema.sql`](../Database/InitSchema.sql) (source of
truth). Column-level rationale is documented as comments in that file.

## Conventions
- Dates are `TEXT`. Date-only columns use `YYYY-MM-DD` (`date()` CHECK);
  timestamps use `YYYY-MM-DD HH:MM:SS` in UTC (`datetime()` CHECK, see
  [Timestamps](#timestamps)).
- Soft delete: rows with `DeletedAt IS NOT NULL` are hidden by a global query
  filter.
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
  data must satisfy it (see [Seed data](#seed-data)).

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

## Seed data

Sample data for development lives in `Database/SampleData/<TableName>.csv`
(one file per table) and is loaded by `Database/PopulateSeedData.sql`. Master
data (`ProjectType`, `SkillCategory`) is loaded separately by
`Database/PopulateMasterData.sql`, which must run first.

### Running

The script uses sqlite3 CLI dot-commands (`.import`), so it must go through
`sqlite3.exe`, not a GUI "execute SQL" box. Run it from the repository root
(CSV paths are relative), after the schema has been applied:

```powershell
sqlite3.exe Database/Quaply.db ".read Database/PopulateMasterData.sql" ".read Database/PopulateSeedData.sql"
```

### Timestamp columns in the CSV

`CreatedAt` is not in the CSV. Every CSV for a table with timestamps ends with
`UpdatedAt, DeletedAt` (junction tables have none), and both columns take one
of three kinds of value:

| Value in CSV                  | `UpdatedAt`                       | `DeletedAt`                       |
| ----------------------------- | --------------------------------- | --------------------------------- |
| empty                         | never modified (`= CreatedAt`)    | not deleted (`NULL`)              |
| integer `N`                   | `now - N days` + jitter (dynamic) | `now - N days` + jitter (dynamic) |
| `YYYY-MM-DD HH:MM:SS` literal | used as-is (static)               | used as-is (static)               |

Dynamic values are recomputed on every run, so "modified 3 days ago" stays true
whenever the seed is loaded. Jitter is a per-`Id` offset of some hours, minutes
and seconds so rows do not all share the same time of day.

### Choosing `UpdatedAt` for the modified-date filter

| Bucket          | CSV value |
| --------------- | --------- |
| Within 7 days   | `1`-`6`   |
| 7 to 30 days    | `9`-`28`  |
| Older than 30 d | `32`+     |
| Never modified  | empty     |

Avoid exactly `7` and `30`. Jitter adds up to about 24 hours, and
`datetime('now')` is UTC while the app may compare in local time (UTC+7), so
the boundary is unstable. Probe it with `6`/`9` and `28`/`32` instead.

### How `CreatedAt` is generated

`CreatedAt = anchor - (35..89 days) - jitter`, where the anchor is `UpdatedAt`
if given, else `DeletedAt` if present, else now. The 35-day minimum is larger
than the 30-day filter on purpose: rows that were never modified
(`UpdatedAt = CreatedAt`) always fall outside the 7-day and 30-day filters.

### Ordering rule

`CreatedAt <= UpdatedAt <= DeletedAt` must hold (see
[Timestamps](#timestamps)); the schema does not check it. For CSV authors:

- If both `UpdatedAt` and `DeletedAt` are integers, `UpdatedAt` must be
  strictly greater than `DeletedAt` (a bigger `N` is further in the past), or
  left empty.
- If either is a literal timestamp, check the order by hand.
- `UpdatedAt` is independent of `DeletedAt`. Soft delete does not touch
  `UpdatedAt` in this app, so do not set `UpdatedAt = DeletedAt`.

### Sanity check

After seeding, this must return zero rows for every table:

```sql
SELECT "Id"
FROM "<Table>"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt");
```

Zero rows means the data is fine (sqlite3 prints nothing). If rows come back,
they are the offenders. For example, if the `WorkExperience` CSV row with
`Id` 21 has `UpdatedAt = 5` and `DeletedAt = 7`, then `now - 5 days` is newer
than `now - 7 days`, which breaks the rule, and `Id` 21 is listed. To fix it,
open that table's CSV, go to that `Id`, and either set `UpdatedAt` to a number
greater than `DeletedAt` (e.g. `10`) or leave it empty.

This only catches ordering errors. It cannot tell whether a row landed in the
intended bucket: a typo such as `60` instead of `6` is still a valid ordering
and will not be reported.

### Maintaining the script

- Timestamps are resolved once, in `temp.seed_ts`, which is built from all the
  staging tables (after the `.import` lines, before the first `INSERT`). Each
  table's `INSERT` just joins it. To seed a new table with timestamps, add one
  `UNION ALL` line to `seed_ts` and the `JOIN` to that table's `INSERT`.
- Jitter multipliers are coprime with 24 and 60 so values spread evenly
  (`UpdatedAt`/`DeletedAt` add `Id*7` h, `Id*13` min, `Id*19` s; `CreatedAt`
  subtracts `Id*5` h, `Id*17` min, `Id*23` s, with a day offset of
  `35 + (Id*11) % 55`). The exact numbers have no meaning.

## Regenerating EF models

### Examples

Regenerating EF models (Development is the default environment):

```powershell
.\Scripts\Invoke-Scaffold.ps1
```

> Note: `Invoke-Scaffold` wraps `Invoke-Database`.
