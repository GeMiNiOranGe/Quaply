PRAGMA foreign_keys = ON;

-- have no key -----------------------------------------------------------------
CREATE TABLE "SkillCategory"
(
    "Id"        INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"      TEXT    NOT NULL,

    "CreatedAt" TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt" TEXT,

    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "ProjectType"
(
    "Id"        INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"      TEXT    NOT NULL,

    "CreatedAt" TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt" TEXT,

    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "Resume"
(
    "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"       TEXT    NOT NULL,
    "ExportedAt" TEXT,

    "CreatedAt"  TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"  TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"  TEXT,

    CHECK ("ExportedAt" IS NULL OR datetime("ExportedAt") = "ExportedAt"),
    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "Profile"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Not null: a profile must have a name to be usable on a Resume at all;
    -- this is the one piece of identity info every generated document needs.
    "FullName"         TEXT    NOT NULL,

    -- Nullable: not every user wants to expose an email on their Resume,
    -- or they may not have filled in contact details yet.
    "Email"            TEXT,

    -- Nullable: same reasoning as Email - optional contact channel,
    -- some Resumes omit a phone number by choice (e.g. remote-only applications).
    "PhoneNumber"      TEXT,

    -- Nullable: not every candidate has (or wants to share) a LinkedIn profile.
    "LinkedInUsername" TEXT,

    -- Nullable: only relevant for candidates who maintain a public GitHub;
    -- irrelevant for many non-developer roles or private-repo users.
    "GitHubUsername"   TEXT,

    -- Nullable: a portfolio site is optional and not every profession has one.
    "PortfolioUrl"     TEXT,

    -- Nullable: date of birth is optional/sensitive info; many Resume formats
    -- (especially in countries with anti-age-discrimination norms)
    -- omit it entirely.
    "DateOfBirth"      TEXT,

    "CreatedAt"        TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"        TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"        TEXT,

    CHECK ("DateOfBirth" IS NULL OR date("DateOfBirth") = "DateOfBirth"),
    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "PersonalSummary"
(
    "Id"                  INTEGER PRIMARY KEY AUTOINCREMENT,
    "TargetPositionTitle" TEXT    NOT NULL,
    "Summary"             TEXT,

    "CreatedAt"           TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"           TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"           TEXT,

    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "WorkExperience"
(
    "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Not null: a work experience entry must be tied to a specific company;
    -- without it the record is meaningless and would leave an invalid gap on
    -- the Resume.
    "CompanyName"   TEXT    NOT NULL,

    -- Not null: the position title is core information for a work experience
    -- entry; employers always need to know what role the candidate held.
    "PositionTitle" TEXT    NOT NULL,

    -- Nullable: job description is optional supplementary info;
    -- users may skip it if they haven't written details yet.
    "Description"   TEXT,

    -- Not null: the start date is required to
    --   (1) order experiences chronologically (most recent first)
    --   (2) compute duration for display on the Resume.
    -- A "work experience" without a known start date doesn't make sense.
    "StartDate"     TEXT    NOT NULL,

    -- Nullable: NULL represents an ongoing/current job (e.g. "Present").
    -- Making this NOT NULL would make it impossible to represent a job still
    -- in progress.
    "EndDate"       TEXT,

    "CreatedAt"     TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"     TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"     TEXT,

    CHECK (date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate"),
    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "Education"
(
    "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
    "SchoolName" TEXT    NOT NULL,
    "Degree"     TEXT,
    "Major"      TEXT,
    "StartDate"  TEXT,
    "EndDate"    TEXT,

    "CreatedAt"  TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"  TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"  TEXT,

    CHECK ("StartDate" IS NULL OR date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate"),
    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "Language"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"             TEXT    NOT NULL,
    "ProficiencyLevel" TEXT    NOT NULL,

    "CreatedAt"        TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"        TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"        TEXT,

    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "Certification"
(
    "Id"             INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"           TEXT    NOT NULL,
    "Issuer"         TEXT,
    "IssueDate"      TEXT,
    "ExpirationDate" TEXT,

    "CreatedAt"      TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"      TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"      TEXT,

    CHECK ("IssueDate" IS NULL OR date("IssueDate") = "IssueDate"),
    CHECK ("ExpirationDate" IS NULL OR date("ExpirationDate") = "ExpirationDate"),
    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

-- has a foreign key -----------------------------------------------------------
CREATE TABLE "Project"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"             TEXT    NOT NULL,
    "Brief"            TEXT,
    "Role"             TEXT,
    "Responsibilities" TEXT,
    "RepositoryUrl"    TEXT,
    "DemoUrl"          TEXT,
    "HasGaps"          INTEGER NOT NULL DEFAULT 0,
    "StartDate"        TEXT,
    "EndDate"          TEXT,

    "CreatedAt"        TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"        TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"        TEXT,

    "WorkExperienceId" INTEGER,
    "ProjectTypeId"    INTEGER NOT NULL,

    FOREIGN KEY ("WorkExperienceId") REFERENCES "WorkExperience"("Id"),
    FOREIGN KEY ("ProjectTypeId")    REFERENCES "ProjectType"("Id"),

    CHECK ("HasGaps" IN (0, 1)),
    CHECK ("StartDate" IS NULL OR date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate"),
    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

CREATE TABLE "Skill"
(
    "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"            TEXT    NOT NULL,
    "IsHighlight"     INTEGER NOT NULL DEFAULT 0,

    "CreatedAt"       TEXT    NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt"       TEXT    NOT NULL DEFAULT (datetime('now')),
    "DeletedAt"       TEXT,

    "SkillCategoryId" INTEGER NOT NULL,

    FOREIGN KEY ("SkillCategoryId") REFERENCES "SkillCategory"("Id"),

    CHECK ("IsHighlight" IN (0, 1)),
    CHECK ("DeletedAt" IS NULL OR datetime("DeletedAt") = "DeletedAt")
)
STRICT;

-- many-to-many relationship ---------------------------------------------------
CREATE TABLE "ResumeProfile"
(
    "Id"        INTEGER PRIMARY KEY AUTOINCREMENT,
    "ResumeId"  INTEGER NOT NULL,
    "ProfileId" INTEGER NOT NULL,

    UNIQUE ("ResumeId", "ProfileId"),

    FOREIGN KEY ("ResumeId")  REFERENCES "Resume"("Id"),
    FOREIGN KEY ("ProfileId") REFERENCES "Profile"("Id")
)
STRICT;

CREATE TABLE "ResumePersonalSummary"
(
    "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
    "ResumeId"          INTEGER NOT NULL,
    "PersonalSummaryId" INTEGER NOT NULL,

    UNIQUE ("ResumeId", "PersonalSummaryId"),

    FOREIGN KEY ("ResumeId")          REFERENCES "Resume"("Id"),
    FOREIGN KEY ("PersonalSummaryId") REFERENCES "PersonalSummary"("Id")
)
STRICT;

CREATE TABLE "ResumeWorkExperience"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,
    "ResumeId"         INTEGER NOT NULL,
    "WorkExperienceId" INTEGER NOT NULL,

    UNIQUE ("ResumeId", "WorkExperienceId"),

    FOREIGN KEY ("ResumeId")         REFERENCES "Resume"("Id"),
    FOREIGN KEY ("WorkExperienceId") REFERENCES "WorkExperience"("Id")
)
STRICT;

CREATE TABLE "ResumeEducation"
(
    "Id"          INTEGER PRIMARY KEY AUTOINCREMENT,
    "ResumeId"    INTEGER NOT NULL,
    "EducationId" INTEGER NOT NULL,

    UNIQUE ("ResumeId", "EducationId"),

    FOREIGN KEY ("ResumeId")    REFERENCES "Resume"("Id"),
    FOREIGN KEY ("EducationId") REFERENCES "Education"("Id")
)
STRICT;

CREATE TABLE "ResumeLanguage"
(
    "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
    "ResumeId"   INTEGER NOT NULL,
    "LanguageId" INTEGER NOT NULL,

    UNIQUE ("ResumeId", "LanguageId"),

    FOREIGN KEY ("ResumeId")   REFERENCES "Resume"("Id"),
    FOREIGN KEY ("LanguageId") REFERENCES "Language"("Id")
)
STRICT;

CREATE TABLE "ResumeCertification"
(
    "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
    "ResumeId"        INTEGER NOT NULL,
    "CertificationId" INTEGER NOT NULL,

    UNIQUE ("ResumeId", "CertificationId"),

    FOREIGN KEY ("ResumeId")        REFERENCES "Resume"("Id"),
    FOREIGN KEY ("CertificationId") REFERENCES "Certification"("Id")
)
STRICT;

CREATE TABLE "ProjectSkill"
(
    "Id"        INTEGER PRIMARY KEY AUTOINCREMENT,
    "ProjectId" INTEGER NOT NULL,
    "SkillId"   INTEGER NOT NULL,

    UNIQUE ("ProjectId", "SkillId"),

    FOREIGN KEY ("ProjectId") REFERENCES "Project"("Id"),
    FOREIGN KEY ("SkillId")   REFERENCES "Skill"("Id")
)
STRICT;
