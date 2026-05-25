PRAGMA foreign_keys = ON;

-- have no key -----------------------------------------------------------------
CREATE TABLE "SkillCategory"
(
    "Id"   INTEGER PRIMARY KEY,
    "Name" TEXT    NOT NULL
);

CREATE TABLE "ProjectType"
(
    "Id"   INTEGER PRIMARY KEY,
    "Name" TEXT    NOT NULL
);

CREATE TABLE "Profile"
(
    "Id"           INTEGER PRIMARY KEY,
    "FullName"     TEXT    NOT NULL,
    "Title"        TEXT,
    "Email"        TEXT,
    "PhoneNumber"  TEXT,
    "LinkedInUrl"  TEXT,
    "GitHubUrl"    TEXT,
    "PortfolioUrl" TEXT,
    "DateOfBirth"  TEXT
);

CREATE TABLE "PersonalSummary"
(
    "Id"            INTEGER PRIMARY KEY,
    "PositionTitle" TEXT    NOT NULL,
    "Summary"       TEXT,
    "CreatedAt"     TEXT    NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE "WorkExperience"
(
    "Id"            INTEGER PRIMARY KEY,
    "CompanyName"   TEXT    NOT NULL,
    "PositionTitle" TEXT,
    "Description"   TEXT,
    "StartDate"     TEXT,
    "EndDate"       TEXT
);

CREATE TABLE "Education"
(
    "Id"         INTEGER PRIMARY KEY,
    "SchoolName" TEXT    NOT NULL,
    "Degree"     TEXT,
    "Major"      TEXT,
    "StartDate"  TEXT,
    "EndDate"    TEXT
);

CREATE TABLE "Language"
(
    "Id"               INTEGER PRIMARY KEY,
    "Name"             TEXT    NOT NULL,
    "ProficiencyLevel" TEXT    NOT NULL
);

CREATE TABLE "Certification"
(
    "Id"             INTEGER PRIMARY KEY,
    "Name"           TEXT    NOT NULL,
    "Issuer"         TEXT,
    "IssueDate"      TEXT,
    "ExpirationDate" TEXT
);

-- has a foreign key -----------------------------------------------------------
CREATE TABLE "Project"
(
    "Id"               INTEGER PRIMARY KEY,
    "Name"             TEXT    NOT NULL,
    "Brief"            TEXT,
    "Role"             TEXT,
    "Responsibilities" TEXT,
    "RepositoryUrl"    TEXT,
    "DemoUrl"          TEXT,
    "StartDate"        TEXT,
    "EndDate"          TEXT,
    "HasGaps"          INTEGER NOT NULL DEFAULT 0,

    "WorkExperienceId" INTEGER REFERENCES "WorkExperience"("Id"),
    "ProjectTypeId"    INTEGER NOT NULL REFERENCES "ProjectType"("Id"),

    CHECK ("HasGaps" IN (0, 1))
);

CREATE TABLE "Skill"
(
    "Id"              INTEGER PRIMARY KEY,
    "Name"            TEXT    NOT NULL,
    "IsHighlight"     INTEGER NOT NULL DEFAULT 0,

    "SkillCategoryId" INTEGER NOT NULL REFERENCES "SkillCategory"("Id"),

    CHECK ("IsHighlight" IN (0, 1))
);

-- many-to-many relationship ---------------------------------------------------
CREATE TABLE "ProfilePersonalSummary"
(
    "Id"                INTEGER PRIMARY KEY,
    "ProfileId"         INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "PersonalSummaryId" INTEGER NOT NULL REFERENCES "PersonalSummary"("Id")
);

CREATE TABLE "ProfileWorkExperience"
(
    "Id"               INTEGER PRIMARY KEY,
    "ProfileId"        INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "WorkExperienceId" INTEGER NOT NULL REFERENCES "WorkExperience"("Id")
);

CREATE TABLE "ProfileEducation"
(
    "Id"          INTEGER PRIMARY KEY,
    "ProfileId"   INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "EducationId" INTEGER NOT NULL REFERENCES "Education"("Id")
);

CREATE TABLE "ProfileLanguage"
(
    "Id"         INTEGER PRIMARY KEY,
    "ProfileId"  INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "LanguageId" INTEGER NOT NULL REFERENCES "Language"("Id")
);

CREATE TABLE "ProfileCertification"
(
    "Id"              INTEGER PRIMARY KEY,
    "ProfileId"       INTEGER NOT NULL REFERENCES "Profile"("Id"),
    "CertificationId" INTEGER NOT NULL REFERENCES "Certification"("Id")
);

CREATE TABLE "ProjectSkill"
(
    "Id"        INTEGER PRIMARY KEY,
    "ProjectId" INTEGER NOT NULL REFERENCES "Project"("Id"),
    "SkillId"   INTEGER NOT NULL REFERENCES "Skill"("Id")
);
