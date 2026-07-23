PRAGMA foreign_keys = ON;

-- have no key -----------------------------------------------------------------
CREATE TABLE "SkillCategory"
(
    "Id"   INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT    NOT NULL
) STRICT;

CREATE TABLE "ProjectType"
(
    "Id"   INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT    NOT NULL
) STRICT;

CREATE TABLE "Profile"
(
    "Id"           INTEGER PRIMARY KEY AUTOINCREMENT,
    "FullName"     TEXT    NOT NULL,
    "Title"        TEXT,
    "Email"        TEXT,
    "PhoneNumber"  TEXT,
    "LinkedInUrl"  TEXT,
    "GitHubUrl"    TEXT,
    "PortfolioUrl" TEXT,
    "DateOfBirth"  TEXT,

    CHECK ("DateOfBirth" IS NULL OR date("DateOfBirth") = "DateOfBirth")
) STRICT;

CREATE TABLE "PersonalSummary"
(
    "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
    "PositionTitle" TEXT    NOT NULL,
    "Summary"       TEXT,
    "CreatedAt"     TEXT    NOT NULL DEFAULT (datetime('now'))
) STRICT;

CREATE TABLE "WorkExperience"
(
    "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
    "CompanyName"   TEXT    NOT NULL,
    "PositionTitle" TEXT,
    "Description"   TEXT,
    "StartDate"     TEXT,
    "EndDate"       TEXT,

    CHECK ("StartDate" IS NULL OR date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate")
) STRICT;

CREATE TABLE "Education"
(
    "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
    "SchoolName" TEXT    NOT NULL,
    "Degree"     TEXT,
    "Major"      TEXT,
    "StartDate"  TEXT,
    "EndDate"    TEXT,

    CHECK ("StartDate" IS NULL OR date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate")
) STRICT;

CREATE TABLE "Language"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"             TEXT    NOT NULL,
    "ProficiencyLevel" TEXT    NOT NULL
) STRICT;

CREATE TABLE "Certification"
(
    "Id"             INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"           TEXT    NOT NULL,
    "Issuer"         TEXT,
    "IssueDate"      TEXT,
    "ExpirationDate" TEXT,

    CHECK ("IssueDate" IS NULL OR date("IssueDate") = "IssueDate"),
    CHECK ("ExpirationDate" IS NULL OR date("ExpirationDate") = "ExpirationDate")
) STRICT;

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

    "WorkExperienceId" INTEGER REFERENCES "WorkExperience"("Id"),
    "ProjectTypeId"    INTEGER NOT NULL REFERENCES "ProjectType"("Id"),

    CHECK ("HasGaps" IN (0, 1)),
    CHECK ("StartDate" IS NULL OR date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate")
) STRICT;

CREATE TABLE "Skill"
(
    "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"            TEXT    NOT NULL,
    "IsHighlight"     INTEGER NOT NULL DEFAULT 0,

    "SkillCategoryId" INTEGER NOT NULL REFERENCES "SkillCategory"("Id"),

    CHECK ("IsHighlight" IN (0, 1))
) STRICT;

-- many-to-many relationship ---------------------------------------------------
CREATE TABLE "ProfilePersonalSummary"
(
    "Id"                INTEGER PRIMARY KEY AUTOINCREMENT,
    "ProfileId"         INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "PersonalSummaryId" INTEGER NOT NULL REFERENCES "PersonalSummary"("Id")
) STRICT;

CREATE TABLE "ProfileWorkExperience"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,
    "ProfileId"        INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "WorkExperienceId" INTEGER NOT NULL REFERENCES "WorkExperience"("Id")
) STRICT;

CREATE TABLE "ProfileEducation"
(
    "Id"          INTEGER PRIMARY KEY AUTOINCREMENT,
    "ProfileId"   INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "EducationId" INTEGER NOT NULL REFERENCES "Education"("Id")
) STRICT;

CREATE TABLE "ProfileLanguage"
(
    "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
    "ProfileId"  INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "LanguageId" INTEGER NOT NULL REFERENCES "Language"("Id")
) STRICT;

CREATE TABLE "ProfileCertification"
(
    "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
    "ProfileId"       INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "CertificationId" INTEGER NOT NULL REFERENCES "Certification"("Id")
) STRICT;

CREATE TABLE "ProjectSkill"
(
    "Id"        INTEGER PRIMARY KEY AUTOINCREMENT,
    "ProjectId" INTEGER NOT NULL REFERENCES "Project"("Id"),
    "SkillId"   INTEGER NOT NULL REFERENCES "Skill"("Id")
) STRICT;
