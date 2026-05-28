PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

DELETE FROM "SkillCategory";
DELETE FROM "ProjectType";
DELETE FROM "Profile";
DELETE FROM "PersonalSummary";
DELETE FROM "WorkExperience";
DELETE FROM "Education";
DELETE FROM "Language";
DELETE FROM "Certification";
DELETE FROM "Project";
DELETE FROM "Skill";
DELETE FROM "ProfilePersonalSummary";
DELETE FROM "ProfileWorkExperience";
DELETE FROM "ProfileEducation";
DELETE FROM "ProfileLanguage";
DELETE FROM "ProfileCertification";
DELETE FROM "ProjectSkill";

COMMIT;

PRAGMA foreign_keys = ON;
