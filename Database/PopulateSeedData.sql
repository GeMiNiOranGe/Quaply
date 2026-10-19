-- =============================================================================
-- HOW TO RUN (this file uses sqlite3 CLI dot-commands, e.g. ".import" -
-- it must be run through the sqlite3.exe command-line shell, not through a
-- generic "execute SQL" box in a GUI tool that only understands plain SQL):
--
--   sqlite3 Database/Quaply.db
--   sqlite> .read Database/PopulateMasterData.sql
--   sqlite> .read Database/PopulateSeedData.sql
--
-- or, from PowerShell / a build script, in one shot:
--
--   sqlite3.exe Database/Quaply.db ".read Database/PopulateMasterData.sql" ".read Database/PopulateSeedData.sql"
--
-- Assumes the schema (CREATE TABLE ...) and PopulateMasterData.sql
-- (ProjectType, SkillCategory) have already been applied to the target db.
-- CSV files are expected at Database/SampleData/<TableName>.csv relative to the
-- working directory .import is run from - adjust the paths below if needed.
--
-- -----------------------------------------------------------------------------
--
-- TIMESTAMP COLUMN CONVENTION (applies to every table below that has
-- CreatedAt / UpdatedAt / DeletedAt - all except the junction tables)
--
-- CSV columns, placed in this order at the end of each file: UpdatedAt, DeletedAt
-- Both columns share the same 3-way value convention:
--   - empty               -> UpdatedAt: never modified (UpdatedAt = CreatedAt)
--                            DeletedAt: not deleted (NULL)
--   - a plain integer,    -> dynamic: datetime('now', '-7 days') plus a
--     e.g. 7                 per-Id hour/minute/second jitter
--                            (recomputed on every run, relative to "now")
--   - a literal timestamp -> used as-is (static/historical), must be
--                            'YYYY-MM-DD HH:MM:SS' (DeletedAt has a CHECK)
--
-- CreatedAt is NOT in the CSV. It is generated, anchored to the row's own dates:
--   anchor = UpdatedAt if given, else DeletedAt if present, else now
--   CreatedAt = anchor - (35..89 days) - jitter
-- The 35-day minimum is deliberate: it is larger than the 30-day filter, so
-- "never modified" rows always fall outside the 7-day and 30-day filters.
--
-- INVARIANT (the schema does NOT enforce it, the seed data must):
--   CreatedAt <= UpdatedAt <= DeletedAt
-- Consequences for CSV authors:
--   * if both are integers, UpdatedAt must be STRICTLY GREATER than DeletedAt
--     (bigger N = further in the past), or leave UpdatedAt empty.
--   * if either one is a literal timestamp, check the order by hand.
--   * UpdatedAt is independent of DeletedAt: soft-delete does not touch
--     UpdatedAt in this app, so do not set UpdatedAt = DeletedAt.
--
-- Picking UpdatedAt values for the "modified date" filter (7d / 30d / all time):
--   inside 7d: 1-6 | 7d-30d: 9-28 | outside 30d: 32+ | never modified: empty
--   Avoid exactly 7 and 30. The jitter adds up to ~24h, and datetime('now') is
--   UTC while the app may compare in local time (UTC+7), so the exact boundary
--   is unstable. Probe the boundary with 6/9 and 28/32 instead.
--
-- Per-table pattern (repeat for each table, in this order):
--   1. INSERT ... SELECT  UpdatedAt/DeletedAt are converted by the CASE blocks.
--                         UpdatedAt is NOT NULL, so empty gets datetime('now')
--                         as a placeholder; CreatedAt stays at its DEFAULT.
--   2. UPDATE CreatedAt   generated from the anchor above.
--   3. UPDATE UpdatedAt   empty-in-CSV rows: UpdatedAt = CreatedAt.
--   Step 3 must run after step 2 (it reads the generated CreatedAt).
--   Steps 2-3 read temp."stg_<Table>", so they must run before staging is dropped.
--   Inside the subquery, "Id"/"UpdatedAt" resolve to the staging table's
--   columns (innermost scope), not the real table's.
--
-- Jitter multipliers (must be coprime with 24 / 60 to spread values evenly):
--   UpdatedAt / DeletedAt: Id*7 % 24 h, Id*13 % 60 min, Id*19 % 60 s (added)
--   CreatedAt:             Id*5 % 24 h, Id*17 % 60 min, Id*23 % 60 s (subtracted)
--   The 35..89 day offset is 35 + (Id*11) % 55.
--
-- If an AFTER UPDATE trigger that auto-sets UpdatedAt is added later, create it
-- AFTER this seed runs, otherwise it overwrites the timestamps generated here.
--
-- Sanity check after seeding (must return zero rows for every table):
--   SELECT "Id"
--   FROM "<Table>"
--   WHERE "CreatedAt" > "UpdatedAt"
--       OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt");
--
--   Example output:
--     * Data is fine: Zero rows (nothing printed by sqlite3 .read). This is
--       the result you want.
--     * Data has an error: The offending rows are listed. For instance,
--       if the CSV row for WorkExperience Id 21 is changed to UpdatedAt = 5
--       and DeletedAt = 7, then UpdatedAt = now - 5 days is NEWER than
--       DeletedAt = now - 7 days, which breaks the invariant:
--
--         Table           Id
--         --------------  --
--         WorkExperience  21
--
--   How to fix: open the CSV of the reported table, go to that Id, and either
--   set UpdatedAt to a number greater than DeletedAt (e.g. 10) or leave
--   UpdatedAt empty.
--
--   Limits: this only catches ordering errors. It cannot tell whether a row
--   landed in the intended filter bucket (7d / 30d / all time); a typo such as
--   60 instead of 6 is still a valid ordering and will not be reported.
-- =============================================================================

-- re-enabled at the end, after all data is in
PRAGMA foreign_keys = OFF;

-- -----------------------------------------------------------------------------
-- STAGING TABLES
-- Plain TEXT columns, no CHECK/STRICT, so .import always succeeds regardless
-- of blank vs. populated fields. Dropped automatically (TEMP) when the
-- connection closes, or explicitly at the end of this script.
-- -----------------------------------------------------------------------------

DROP TABLE IF EXISTS temp."stg_Resume";
CREATE TEMP TABLE "stg_Resume" (
    "Id" TEXT,
    "Name" TEXT,
    "ExportedAt" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Profile";
CREATE TEMP TABLE "stg_Profile" (
    "Id" TEXT,
    "FullName" TEXT,
    "Email" TEXT,
    "PhoneNumber" TEXT,
    "LinkedInUsername" TEXT,
    "GitHubUsername" TEXT,
    "PortfolioUrl" TEXT,
    "DateOfBirth" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_PersonalSummary";
CREATE TEMP TABLE "stg_PersonalSummary" (
    "Id" TEXT,
    "TargetPositionTitle" TEXT,
    "Summary" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_WorkExperience";
CREATE TEMP TABLE "stg_WorkExperience" (
    "Id" TEXT,
    "CompanyName" TEXT,
    "PositionTitle" TEXT,
    "Description" TEXT,
    "StartDate" TEXT,
    "EndDate" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Education";
CREATE TEMP TABLE "stg_Education" (
    "Id" TEXT,
    "SchoolName" TEXT,
    "Degree" TEXT,
    "Major" TEXT,
    "StartDate" TEXT,
    "EndDate" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Language";
CREATE TEMP TABLE "stg_Language" (
    "Id" TEXT,
    "Name" TEXT,
    "ProficiencyLevel" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Certification";
CREATE TEMP TABLE "stg_Certification" (
    "Id" TEXT,
    "Name" TEXT,
    "Issuer" TEXT,
    "IssueDate" TEXT,
    "ExpirationDate" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Skill";
CREATE TEMP TABLE "stg_Skill" (
    "Id" TEXT,
    "Name" TEXT,
    "IsHighlight" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT,
    "SkillCategoryId" TEXT
);

DROP TABLE IF EXISTS temp."stg_Project";
CREATE TEMP TABLE "stg_Project" (
    "Id" TEXT,
    "Name" TEXT,
    "Brief" TEXT,
    "Role" TEXT,
    "Responsibilities" TEXT,
    "RepositoryUrl" TEXT,
    "DemoUrl" TEXT,
    "StartDate" TEXT,
    "EndDate" TEXT,
    "HasGaps" TEXT,
    "WorkExperienceId" TEXT,
    "ProjectTypeId" TEXT,
    "UpdatedAt" TEXT,
    "DeletedAt" TEXT
);

-- -----------------------------------------------------------------------------
-- IMPORT CSVs INTO STAGING (dot-commands - sqlite3 CLI only)
-- -----------------------------------------------------------------------------
.mode csv
.import --skip 1 Database/SampleData/Resume.csv stg_Resume
.import --skip 1 Database/SampleData/Profile.csv stg_Profile
.import --skip 1 Database/SampleData/PersonalSummary.csv stg_PersonalSummary
.import --skip 1 Database/SampleData/WorkExperience.csv stg_WorkExperience
.import --skip 1 Database/SampleData/Education.csv stg_Education
.import --skip 1 Database/SampleData/Language.csv stg_Language
.import --skip 1 Database/SampleData/Certification.csv stg_Certification
.import --skip 1 Database/SampleData/Skill.csv stg_Skill
.import --skip 1 Database/SampleData/Project.csv stg_Project

-- -----------------------------------------------------------------------------
-- STAGING -> REAL TABLES
-- NULLIF('', '') turns blank text into NULL for nullable text columns.
-- The DeletedAt CASE handles the 3-way convention described at the top.
-- Insertion order respects FK dependencies (WorkExperience/Skill before
-- Project; ProjectType/SkillCategory must already exist via master data).
--
-- Hour/minute/second jitter on dynamic DeletedAt values: '+' || ((Id * 7) % 24) || ' hours',
-- '+' || ((Id * 13) % 60) || ' minutes', '+' || ((Id * 19) % 60) || ' seconds' below.
-- 7 and 24 are coprime (gcd = 1), so as Id increases by 1, (Id * 7) % 24 cycles
-- through all 24 possible hour values before repeating - same idea for 13/60
-- (minutes) and 19/60 (seconds). All three multipliers (7, 13, 19) are also
-- different from each other, so hours/minutes/seconds don't drift in lockstep.
-- Picking a non-coprime multiplier (e.g. Id * 12 % 24) would collapse the result
-- to only a couple of values ({0, 12}), making consecutive rows land on visibly
-- repeating/clustered timestamps instead of a spread-out, natural-looking one.
-- The exact numbers 7/13/19 have no other significance - any small multipliers
-- coprime with 24 (hours) or 60 (minutes/seconds) work the same.
-- -----------------------------------------------------------------------------

INSERT INTO "Resume"
    (
        "Id",
        "Name",
        "ExportedAt",
        "UpdatedAt",
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    NULLIF("ExportedAt", ''),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END
FROM "stg_Resume";

UPDATE "Resume"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_Resume"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "Resume"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_Resume"
    WHERE COALESCE("UpdatedAt", '') = ''
);

-- =============================================================================
-- PROFILE
-- =============================================================================

INSERT INTO "Profile"
    (
        "Id",
        "FullName",
        "Email",
        "PhoneNumber",
        "LinkedInUsername",
        "GitHubUsername",
        "PortfolioUrl",
        "DateOfBirth",
        "UpdatedAt",
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "FullName",
    NULLIF("Email", ''),
    NULLIF("PhoneNumber", ''),
    NULLIF("LinkedInUsername", ''),
    NULLIF("GitHubUsername", ''),
    NULLIF("PortfolioUrl", ''),
    NULLIF("DateOfBirth", ''),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END
FROM "stg_Profile";

UPDATE "Profile"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_Profile"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "Profile"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_Profile"
    WHERE COALESCE("UpdatedAt", '') = ''
);

INSERT INTO "ResumeProfile"
        ("ResumeId", "ProfileId")
VALUES  (1,           1)
     ,  (2,           2)
;

-- =============================================================================
-- PERSONAL SUMMARY
-- =============================================================================

INSERT INTO "PersonalSummary"
    (
        "Id",
        "TargetPositionTitle",
        "Summary",
        "UpdatedAt",
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "TargetPositionTitle",
    NULLIF("Summary", ''),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END
FROM "stg_PersonalSummary";

UPDATE "PersonalSummary"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_PersonalSummary"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "PersonalSummary"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_PersonalSummary"
    WHERE COALESCE("UpdatedAt", '') = ''
);

INSERT INTO "ResumePersonalSummary"
        ("ResumeId", "PersonalSummaryId")
VALUES  (1,          1)
     ,  (2,          2)
;

-- =============================================================================
-- WORK EXPERIENCE
-- =============================================================================

INSERT INTO "WorkExperience"
    (
        "Id",
        "CompanyName",
        "PositionTitle",
        "Description",
        "StartDate",
        "EndDate",
        "UpdatedAt",
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "CompanyName",
    "PositionTitle",
    NULLIF("Description", ''),
    "StartDate",
    NULLIF("EndDate", ''),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END
FROM "stg_WorkExperience";

UPDATE "WorkExperience"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_WorkExperience"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "WorkExperience"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_WorkExperience"
    WHERE COALESCE("UpdatedAt", '') = ''
);

INSERT INTO "ResumeWorkExperience"
        ("ResumeId", "WorkExperienceId")
    -- Resume 1 (Backend): Stripe Backend + Shopify
VALUES  (1,          1)
     ,  (1,          2)
    -- Resume 2 (DevOps): Stripe DevOps + AWS Intern
     ,  (2,          3)
     ,  (2,          4)
;

-- =============================================================================
-- EDUCATION
-- =============================================================================

INSERT INTO "Education"
    (
        "Id",
        "SchoolName",
        "Degree",
        "Major",
        "StartDate",
        "EndDate",
        "UpdatedAt",
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "SchoolName",
    NULLIF("Degree", ''),
    NULLIF("Major", ''),
    NULLIF("StartDate", ''),
    NULLIF("EndDate", ''),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END
FROM "stg_Education";

UPDATE "Education"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_Education"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "Education"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_Education"
    WHERE COALESCE("UpdatedAt", '') = ''
);

-- Both resumes share the same education
INSERT INTO "ResumeEducation"
        ("ResumeId", "EducationId")
VALUES  (1,          1)
     ,  (2,          1)
;

-- =============================================================================
-- LANGUAGE
-- =============================================================================

INSERT INTO "Language"
    (
        "Id",
        "Name",
        "ProficiencyLevel",
        "UpdatedAt",
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    "ProficiencyLevel",
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END
FROM "stg_Language";

UPDATE "Language"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_Language"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "Language"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_Language"
    WHERE COALESCE("UpdatedAt", '') = ''
);

-- Both resumes share languages
INSERT INTO "ResumeLanguage"
        ("ResumeId", "LanguageId")
VALUES  (1,          1)
     ,  (1,          2)
     ,  (2,          1)
     ,  (2,          2)
;

-- =============================================================================
-- CERTIFICATION
-- Resume 1 (Backend): Java cert
-- Resume 2 (DevOps): AWS SAA + CKA
-- =============================================================================

INSERT INTO "Certification"
    (
        "Id",
        "Name",
        "Issuer",
        "IssueDate",
        "ExpirationDate",
        "UpdatedAt",
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    NULLIF("Issuer", ''),
    NULLIF("IssueDate", ''),
    NULLIF("ExpirationDate", ''),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END
FROM "stg_Certification";

UPDATE "Certification"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_Certification"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "Certification"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_Certification"
    WHERE COALESCE("UpdatedAt", '') = ''
);

INSERT INTO "ResumeCertification"
        ("ResumeId", "CertificationId")
VALUES  (1,          1)  -- Backend resume gets Java cert
     ,  (2,          2)  -- DevOps resume gets AWS SAA
     ,  (2,          3)  -- DevOps resume gets CKA
;

-- =============================================================================
-- SKILLS
-- =============================================================================

INSERT INTO "Skill"
    (
        "Id",
        "Name",
        "IsHighlight",
        "UpdatedAt",
        "DeletedAt",
        "SkillCategoryId"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    CAST("IsHighlight" AS INTEGER),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END,
    CAST("SkillCategoryId" AS INTEGER)
FROM "stg_Skill";

UPDATE "Skill"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_Skill"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "Skill"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_Skill"
    WHERE COALESCE("UpdatedAt", '') = ''
);

-- =============================================================================
-- PROJECTS
-- =============================================================================

INSERT INTO "Project"
    (
        "Id",
        "Name",
        "Brief",
        "Role",
        "Responsibilities",
        "RepositoryUrl",
        "DemoUrl",
        "StartDate",
        "EndDate",
        "HasGaps",
        "UpdatedAt",
        "DeletedAt",
        "WorkExperienceId",
        "ProjectTypeId"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    NULLIF("Brief", ''),
    NULLIF("Role", ''),
    NULLIF("Responsibilities", ''),
    NULLIF("RepositoryUrl", ''),
    NULLIF("DemoUrl", ''),
    NULLIF("StartDate", ''),
    NULLIF("EndDate", ''),
    CAST("HasGaps" AS INTEGER),
    CASE
        WHEN "UpdatedAt" IS NULL OR "UpdatedAt" = '' THEN datetime('now')
        WHEN "UpdatedAt" = CAST(CAST("UpdatedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "UpdatedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "UpdatedAt"
    END,
    CASE
        WHEN "DeletedAt" IS NULL OR "DeletedAt" = '' THEN NULL
        WHEN "DeletedAt" = CAST(CAST("DeletedAt" AS INTEGER) AS TEXT) THEN datetime(
            'now',
            '-' || "DeletedAt" || ' days',
            '+' || ((CAST("Id" AS INTEGER) * 7) % 24) || ' hours',
            '+' || ((CAST("Id" AS INTEGER) * 13) % 60) || ' minutes',
            '+' || ((CAST("Id" AS INTEGER) * 19) % 60) || ' seconds'
        )
        ELSE "DeletedAt"
    END,
    CAST(NULLIF("WorkExperienceId", '') AS INTEGER),
    CAST("ProjectTypeId" AS INTEGER)
FROM "stg_Project";

UPDATE "Project"
SET "CreatedAt" = datetime(
    CASE
        WHEN "Id" IN (
            SELECT CAST("Id" AS INTEGER)
            FROM temp."stg_Project"
            WHERE COALESCE("UpdatedAt", '') = ''
        ) THEN COALESCE("DeletedAt", 'now')
        ELSE "UpdatedAt"
    END,
    -- Approximately 35-89 days, intentionally greater than 30 so that
    -- rows that have never been edited always remain outside the 30-day filter.
    '-' || (35 + ("Id" * 11) % 55) || ' days',
    '-' || (("Id" * 5)  % 24) || ' hours',
    '-' || (("Id" * 17) % 60) || ' minutes',
    '-' || (("Id" * 23) % 60) || ' seconds'
);

UPDATE "Project"
SET "UpdatedAt" = "CreatedAt"
WHERE "Id" IN (
    SELECT CAST("Id" AS INTEGER)
    FROM temp."stg_Project"
    WHERE COALESCE("UpdatedAt", '') = ''
);

-- =============================================================================
-- PROJECT SKILLS
-- Resume 1 projects use backend-focused skills
-- Resume 2 projects use DevOps-focused skills
-- =============================================================================

INSERT INTO "ProjectSkill"
        ("ProjectId", "SkillId")
    -- Project 1: Stripe Notification Service
VALUES  (1,           1)   -- Go
     ,  (1,           22)  -- Kafka
     ,  (1,           17)  -- Redis
     ,  (1,           20)  -- Docker
     ,  (1,           36)  -- Microservices
     ,  (1,           39)  -- Event-Driven Architecture
    -- Project 2: Order Management System
     ,  (2,           2)   -- Java
     ,  (2,           8)   -- Spring Boot
     ,  (2,           16)  -- MySQL
     ,  (2,           17)  -- Redis
     ,  (2,           38)  -- RESTful API
    -- Project 3: go-taskq
     ,  (3,           1)   -- Go
     ,  (3,           17)  -- Redis
     ,  (3,           15)  -- PostgreSQL
     ,  (3,           33)  -- Git
    -- Project 4: Internal Developer Platform
     ,  (4,           21)  -- Kubernetes
     ,  (4,           23)  -- Terraform
     ,  (4,           13)  -- ArgoCD
     ,  (4,           14)  -- Helm
     ,  (4,           29)  -- AWS EKS
     ,  (4,           40)  -- CI/CD
     ,  (4,           41)  -- Infrastructure as Code
     ,  (4,           42)  -- GitOps
    -- Project 5: Observability Stack
     ,  (5,           26)  -- Prometheus
     ,  (5,           27)  -- Grafana
     ,  (5,           28)  -- Datadog
     ,  (5,           21)  -- Kubernetes
     ,  (5,           43)  -- SRE
    -- Project 6: k8s-cost-exporter
     ,  (6,           3)   -- Python
     ,  (6,           26)  -- Prometheus
     ,  (6,           20)  -- Docker
     ,  (6,           14)  -- Helm
     ,  (6,           31)  -- AWS S3
;

-- Sanity check
SELECT 'Resume' AS "Table", "Id"
FROM "Resume"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'Profile', "Id"
FROM "Profile"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'PersonalSummary', "Id"
FROM "PersonalSummary"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'WorkExperience', "Id"
FROM "WorkExperience"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'Education', "Id"
FROM "Education"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'Language', "Id"
FROM "Language"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'Certification', "Id"
FROM "Certification"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'Skill', "Id"
FROM "Skill"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt")
UNION ALL
SELECT 'Project', "Id"
FROM "Project"
WHERE "CreatedAt" > "UpdatedAt"
    OR ("DeletedAt" IS NOT NULL AND "UpdatedAt" > "DeletedAt");

PRAGMA foreign_keys = ON;
