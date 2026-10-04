PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

DELETE FROM "SkillCategory";
DELETE FROM "ProjectType";
DELETE FROM "Resume";
DELETE FROM "Profile";
DELETE FROM "PersonalSummary";
DELETE FROM "WorkExperience";
DELETE FROM "Education";
DELETE FROM "Language";
DELETE FROM "Certification";
DELETE FROM "Project";
DELETE FROM "Skill";
DELETE FROM "ResumeProfile";
DELETE FROM "ResumePersonalSummary";
DELETE FROM "ResumeWorkExperience";
DELETE FROM "ResumeEducation";
DELETE FROM "ResumeLanguage";
DELETE FROM "ResumeCertification";
DELETE FROM "ProjectSkill";

COMMIT;

PRAGMA foreign_keys = ON;
