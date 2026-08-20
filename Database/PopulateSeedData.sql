INSERT INTO "Resume"
    (
        "Id",
        "Name",
        "ExportedAt",
        "DeletedAt"
    )
VALUES
    (
        1,
        'Ethan Carter - Backend Developer',
        NULL,
        NULL
    ),
    (
        2,
        'Ethan Carter - DevOps / Cloud Engineer',
        NULL,
        NULL
    ),
    (
        3,
        'Ethan Carter - Draft Frontend CV',
        NULL,
        '2024-09-05 14:22:10'
    );

-- =============================================================================
-- PROFILE
-- Profile 1: Backend Developer
-- Profile 2: DevOps / Cloud Engineer
-- Both profiles belong to the same user: Ethan Carter (American)
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
VALUES
    (
        1,
        'Ethan Carter',
        'ethan.carter.dev@gmail.com',
        '+1 415 234 5678',
        'ethan-carter-dev',
        'ethan-carter-dev',
        NULL,
        '1997-08-15',
        NULL
    ),
    (
        2,
        'Ethan Carter',
        'ethan.carter.dev@gmail.com',
        '+1 415 234 5678',
        'ethan-carter-dev',
        'ethan-carter-dev',
        'https://ethancarter.dev',
        '1997-08-15',
        NULL
    ),
    (
        3,
        'Ethan Carter',
        'ethan.carter.old@yahoo.com',
        '+1 415 000 1111',
        'ethancarter1997',
        'ethancarter97',
        NULL,
        '1997-08-15',
        '2023-01-10 08:30:00'
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
        "DeletedAt"
    )
VALUES
    (
        1,
        'Backend Software Engineer',
        'Backend developer with 5+ years of experience building scalable, high-performance REST and gRPC services. Proficient in Go and Java Spring Boot, with strong knowledge of microservices architecture, message queues, and relational/NoSQL databases. Passionate about clean code, domain-driven design, and performance optimization.',
        NULL
    ),
    (
        2,
        'DevOps / Cloud Engineer',
        'DevOps engineer with 4+ years of experience designing and maintaining CI/CD pipelines, container orchestration on Kubernetes, and cloud infrastructure on AWS. Experienced in infrastructure-as-code (Terraform, Ansible) and platform reliability engineering. Focused on automating everything and reducing MTTR.',
        NULL
    ),
    (
        3,
        'Fullstack Developer',
        'Fullstack developer eager to work across frontend and backend, early-career profile.',
        '2021-02-14 11:00:00'
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
        "DeletedAt"
    )
VALUES
    (
        1,
        'Stripe',
        'Backend Engineer',
        'Built and maintained high-throughput backend services for Stripe''s payment processing platform. Led migration of legacy monolith to microservices using Go and Kafka.',
        '2021-06-01',
        NULL,
        NULL
    ),
    (
        2,
        'Shopify',
        'Junior Backend Developer',
        'Developed e-commerce order management APIs using Java Spring Boot and MySQL. Implemented caching layers with Redis to reduce DB load by 40%.',
        '2019-08-01',
        '2021-05-01',
        NULL
    ),
    (
        3,
        'Stripe',
        'DevOps Engineer',
        'Owned the internal Kubernetes clusters on AWS EKS for 20+ microservices. Implemented GitOps workflow with ArgoCD and reduced deployment time from 45 min to under 8 min.',
        '2022-01-01',
        NULL,
        NULL
    ),
    (
        4,
        'Amazon Web Services',
        'Infrastructure Engineer Intern',
        'Assisted in provisioning AWS EC2/RDS resources and writing Ansible playbooks for configuration management.',
        '2019-06-01',
        '2019-08-01',
        NULL
    ),
    (
        5,
        'Freelance',
        'Part-time Tutor',
        'Taught introductory programming to bootcamp students on weekends.',
        '2018-01-01',
        '2018-12-01',
        '2022-03-20 16:45:00'
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
        "DeletedAt"
    )
VALUES
    (
        1,
        'University of California, Berkeley',
        'Bachelor of Science',
        'Computer Science',
        '2015-09-01',
        '2019-05-01',
        NULL
    ),
    (
        2,
        'Coursera',
        'Online Certificate',
        'Data Structures',
        '2018-06-01',
        '2018-08-01',
        '2023-07-01 09:00:00'
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
        ("Id", "Name",    "ProficiencyLevel",            "DeletedAt")
VALUES  (1,    'English', 'Native',                      NULL)
     ,  (2,    'Spanish', 'Limited Working Proficiency', NULL)
     ,  (3,    'French',  'Elementary Proficiency',      '2024-01-15 12:00:00')
;

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
VALUES
    (
        1,
        'Oracle Certified Professional: Java SE 11 Developer',
        'Oracle',
        '2021-03-01',
        NULL,
        NULL
    ),
    (
        2,
        'AWS Certified Solutions Architect - Associate',
        'Amazon Web Services',
        '2022-09-01',
        '2025-09-01',
        NULL
    ),
    (
        3,
        'Certified Kubernetes Administrator (CKA)',
        'Cloud Native Computing Foundation',
        '2023-04-01',
        '2026-04-01',
        NULL
    ),
    (
        4,
        'MySQL 5.7 Database Administrator',
        'Oracle',
        '2018-05-01',
        '2021-05-01',
        '2022-08-01 10:00:00'
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
        ("Id", "Name",                         "IsHighlight", "DeletedAt",           "SkillCategoryId")
    -- Programming language
VALUES  (1,    'Go',                           1,             NULL,                  2)
     ,  (2,    'Java',                         1,             NULL,                  2)
     ,  (3,    'Python',                       0,             NULL,                  2)
     ,  (4,    'SQL',                          0,             NULL,                  2)
     ,  (5,    'Bash',                         0,             NULL,                  2)
     ,  (6,    'TypeScript',                   0,             NULL,                  2)
     ,  (7,    'HCL (Terraform)',              1,             NULL,                  2)
    -- Framework / Library
     ,  (8,    'Spring Boot',                  1,             NULL,                  3)
     ,  (9,    'gRPC',                         1,             NULL,                  3)
     ,  (10,   'Gin',                          1,             NULL,                  3)
     ,  (11,   'GORM',                         0,             NULL,                  3)
     ,  (12,   'Ansible',                      1,             NULL,                  3)
     ,  (13,   'ArgoCD',                       1,             NULL,                  3)
     ,  (14,   'Helm',                         0,             NULL,                  3)
    -- Database
     ,  (15,   'PostgreSQL',                   1,             NULL,                  4)
     ,  (16,   'MySQL',                        0,             NULL,                  4)
     ,  (17,   'Redis',                        1,             NULL,                  4)
     ,  (18,   'MongoDB',                      0,             NULL,                  4)
     ,  (19,   'Elasticsearch',                0,             NULL,                  4)
    -- Tool
     ,  (20,   'Docker',                       1,             NULL,                  5)
     ,  (21,   'Kubernetes',                   1,             NULL,                  5)
     ,  (22,   'Kafka',                        1,             NULL,                  5)
     ,  (23,   'Terraform',                    1,             NULL,                  5)
     ,  (24,   'GitHub Actions',               0,             NULL,                  5)
     ,  (25,   'Jenkins',                      0,             NULL,                  5)
     ,  (26,   'Prometheus',                   0,             NULL,                  5)
     ,  (27,   'Grafana',                      0,             NULL,                  5)
     ,  (28,   'Datadog',                      0,             NULL,                  5)
     ,  (29,   'AWS EKS',                      1,             NULL,                  5)
     ,  (30,   'AWS RDS',                      0,             NULL,                  5)
     ,  (31,   'AWS S3',                       0,             NULL,                  5)
     ,  (32,   'AWS Lambda',                   0,             NULL,                  5)
     ,  (33,   'Git',                          0,             NULL,                  5)
     ,  (34,   'Postman',                      0,             NULL,                  5)
    -- OS
     ,  (35,   'Linux (Ubuntu/CentOS)',        1,             NULL,                  6)
    -- Concept
     ,  (36,   'Microservices',                1,             NULL,                  7)
     ,  (37,   'Domain-Driven Design',         0,             NULL,                  7)
     ,  (38,   'RESTful API',                  1,             NULL,                  7)
     ,  (39,   'Event-Driven Architecture',    0,             NULL,                  7)
     ,  (40,   'CI/CD',                        1,             NULL,                  7)
     ,  (41,   'Infrastructure as Code',       1,             NULL,                  7)
     ,  (42,   'GitOps',                       1,             NULL,                  7)
     ,  (43,   'Site Reliability Engineering', 0,             NULL,                  7)
     ,  (44,   'Twelve-Factor App',            0,             NULL,                  7)
    --  [Deleted]
     ,  (45,   'PHP',                          0,             '2023-05-12 08:00:00', 2)
     ,  (46,   'jQuery',                       0,             '2023-05-12 08:00:00', 3)
     ,  (47,   'CircleCI',                     0,             '2024-02-20 15:30:00', 5)
;

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
VALUES
    -- -- Resume 1 (Backend) projects ------------------------------------------
    (
        1,
        'Stripe Notification Service',
        'A high-throughput push notification microservice handling 500k+ messages/day for Stripe users across iOS, Android, and Web.',
        'Backend Engineer',
        'Designed the service in Go with a Kafka consumer pipeline. Implemented retry logic, dead-letter queue, and per-device rate limiting. Wrote internal benchmarks achieving <5ms p99 latency.',
        NULL,
        NULL,
        '2022-03-01',
        NULL,
        0,
        NULL,
        1,  -- Stripe Backend
        3   -- Professional
    ),
    (
        2,
        'Order Management System - Shopify',
        'Core backend API for a B2B e-commerce order flow: cart, checkout, inventory reservation, invoicing.',
        'Junior Backend Developer',
        'Built REST APIs with Spring Boot. Integrated Redis for session/cache. Wrote JUnit tests achieving 85% code coverage. Coordinated with frontend team on API contracts.',
        NULL,
        NULL,
        '2020-01-01',
        '2021-05-01',
        0,
        NULL,
        2,  -- Shopify
        3   -- Professional
    ),
    (
        3,
        'go-taskq',
        'Open-source lightweight task queue library for Go with support for multiple backends (Redis, PostgreSQL).',
        'Author / Maintainer',
        'Designed the public API, implemented workers, retry strategies, and backoff. Published on GitHub with documentation and example apps. 200+ GitHub stars.',
        'https://github.com/ethan-carter-dev/go-taskq',
        NULL,
        '2023-01-01',
        NULL,
        0,
        NULL,
        NULL,  -- personal project
        2      -- Personal
    ),

    -- -- Resume 2 (DevOps) projects ------------------------------------------
    (
        4,
        'Internal Developer Platform - Stripe',
        'Built a self-service Kubernetes platform for 50+ engineers, abstracting cluster operations behind a GitOps workflow.',
        'DevOps Engineer',
        'Set up AWS EKS clusters with Terraform. Deployed ArgoCD for GitOps. Wrote Helm chart templates for standardized service deployments. Reduced onboarding time for new services from 3 days to 2 hours.',
        NULL,
        NULL,
        '2022-04-01',
        NULL,
        0,
        NULL,
        3,  -- Stripe DevOps
        3   -- Professional
    ),
    (
        5,
        'Observability Stack Migration',
        'Migrated monitoring from CloudWatch to a self-hosted Prometheus + Grafana + Alertmanager stack, covering 20+ microservices.',
        'DevOps Engineer',
        'Designed recording rules and alerting policies. Integrated Datadog for APM traces. Created runbook documentation and on-call playbooks.',
        NULL,
        NULL,
        '2023-06-01',
        '2024-01-01',
        0,
        NULL,
        3,  -- Stripe DevOps
        3   -- Professional
    ),
    (
        6,
        'k8s-cost-exporter',
        'Open-source Prometheus exporter that surfaces per-namespace AWS cost allocation data from Cost Explorer API.',
        'Author',
        'Built in Python with the official Prometheus client. Packaged as a Docker image with Helm chart. Featured in CNCF newsletter.',
        'https://github.com/ethan-carter-dev/k8s-cost-exporter',
        NULL,
        '2023-09-01',
        NULL,
        0,
        NULL,
        NULL,  -- personal
        5      -- OpenSource
    ),
    (
        7,
        'Legacy Portfolio Site',
        'Old personal portfolio website built early in career, replaced by a newer version.',
        'Author',
        'Built a static portfolio site with HTML/CSS/jQuery, no longer maintained.',
        NULL,
        NULL,
        '2018-03-01',
        '2018-06-01',
        0,
        '2023-05-12 08:10:00',
        NULL,
        2
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
