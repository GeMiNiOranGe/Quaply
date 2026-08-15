PRAGMA foreign_keys = ON;

-- have no key -----------------------------------------------------------------
CREATE TABLE "SkillCategory"
(
    "Id"   INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT    NOT NULL
)
STRICT;

CREATE TABLE "ProjectType"
(
    "Id"   INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT    NOT NULL
)
STRICT;

CREATE TABLE "Resume"
(
    "Id"         INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"       TEXT    NOT NULL,
    "ExportedAt" TEXT,

    CHECK ("ExportedAt" IS NULL OR datetime("ExportedAt") = "ExportedAt")
)
STRICT;

CREATE TABLE "Profile"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,
    "FullName"         TEXT    NOT NULL,
    "Email"            TEXT,
    "PhoneNumber"      TEXT,
    "LinkedInUsername" TEXT,
    "GitHubUsername"   TEXT,
    "PortfolioUrl"     TEXT,
    "DateOfBirth"      TEXT,

    CHECK ("DateOfBirth" IS NULL OR date("DateOfBirth") = "DateOfBirth")
)
STRICT;

CREATE TABLE "PersonalSummary"
(
    "Id"            INTEGER PRIMARY KEY AUTOINCREMENT,
    "PositionTitle" TEXT    NOT NULL,
    "Summary"       TEXT,
    "CreatedAt"     TEXT    NOT NULL DEFAULT (datetime('now'))
)
STRICT;

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

    CHECK ("StartDate" IS NULL OR date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate")
)
STRICT;

CREATE TABLE "Language"
(
    "Id"               INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"             TEXT    NOT NULL,
    "ProficiencyLevel" TEXT    NOT NULL
)
STRICT;

CREATE TABLE "Certification"
(
    "Id"             INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"           TEXT    NOT NULL,
    "Issuer"         TEXT,
    "IssueDate"      TEXT,
    "ExpirationDate" TEXT,

    CHECK ("IssueDate" IS NULL OR date("IssueDate") = "IssueDate"),
    CHECK ("ExpirationDate" IS NULL OR date("ExpirationDate") = "ExpirationDate")
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

    "WorkExperienceId" INTEGER,
    "ProjectTypeId"    INTEGER NOT NULL,

    FOREIGN KEY ("WorkExperienceId") REFERENCES "WorkExperience"("Id"),
    FOREIGN KEY ("ProjectTypeId")    REFERENCES "ProjectType"("Id"),

    CHECK ("HasGaps" IN (0, 1)),
    CHECK ("StartDate" IS NULL OR date("StartDate") = "StartDate"),
    CHECK ("EndDate" IS NULL OR date("EndDate") = "EndDate")
)
STRICT;

CREATE TABLE "Skill"
(
    "Id"              INTEGER PRIMARY KEY AUTOINCREMENT,
    "Name"            TEXT    NOT NULL,
    "IsHighlight"     INTEGER NOT NULL DEFAULT 0,

    "SkillCategoryId" INTEGER NOT NULL,

    FOREIGN KEY ("SkillCategoryId") REFERENCES "SkillCategory"("Id"),

    CHECK ("IsHighlight" IN (0, 1))
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
