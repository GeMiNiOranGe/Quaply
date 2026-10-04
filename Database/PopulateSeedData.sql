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
-- DeletedAt column convention in every CSV (last column, header "DeletedAt"):
--   - empty                       -> not deleted (DeletedAt = NULL)
--   - a plain integer, e.g. "7"   -> dynamic: datetime('now', '-7 days')
--                                     (recomputed fresh every time this
--                                     script runs, relative to "now")
--   - a literal timestamp, e.g.
--     "2024-09-05 14:22:10"       -> used as-is (static/historical date)
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
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_PersonalSummary";
CREATE TEMP TABLE "stg_PersonalSummary" (
    "Id" TEXT,
    "TargetPositionTitle" TEXT,
    "Summary" TEXT,
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
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Language";
CREATE TEMP TABLE "stg_Language" (
    "Id" TEXT,
    "Name" TEXT,
    "ProficiencyLevel" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Certification";
CREATE TEMP TABLE "stg_Certification" (
    "Id" TEXT,
    "Name" TEXT,
    "Issuer" TEXT,
    "IssueDate" TEXT,
    "ExpirationDate" TEXT,
    "DeletedAt" TEXT
);

DROP TABLE IF EXISTS temp."stg_Skill";
CREATE TEMP TABLE "stg_Skill" (
    "Id" TEXT,
    "Name" TEXT,
    "IsHighlight" TEXT,
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
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    NULLIF("ExportedAt", ''),
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
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "TargetPositionTitle",
    NULLIF("Summary", ''),
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
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    "ProficiencyLevel",
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
        "DeletedAt"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    NULLIF("Issuer", ''),
    NULLIF("IssueDate", ''),
    NULLIF("ExpirationDate", ''),
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
        "DeletedAt",
        "SkillCategoryId"
    )
SELECT
    CAST("Id" AS INTEGER),
    "Name",
    CAST("IsHighlight" AS INTEGER),
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

PRAGMA foreign_keys = ON;
