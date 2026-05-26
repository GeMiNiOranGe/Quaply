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
        "Title",
        "Email",
        "PhoneNumber",
        "LinkedInUrl",
        "GitHubUrl",
        "PortfolioUrl",
        "DateOfBirth"
    )
VALUES
    (
        1,
        'Ethan Carter',
        'Backend Software Engineer',
        'ethan.carter.dev@gmail.com',
        '+1 415 234 5678',
        'https://linkedin.com/in/ethan-carter-dev',
        'https://github.com/ethan-carter-dev',
        NULL,
        '1997-08-15'
    ),
    (
        2,
        'Ethan Carter',
        'DevOps / Cloud Engineer',
        'ethan.carter.dev@gmail.com',
        '+1 415 234 5678',
        'https://linkedin.com/in/ethan-carter-dev',
        'https://github.com/ethan-carter-dev',
        'https://ethancarter.dev',
        '1997-08-15'
    );

-- =============================================================================
-- PERSONAL SUMMARY
-- =============================================================================

INSERT INTO "PersonalSummary"
    (
        "Id",
        "PositionTitle",
        "Summary"
    )
VALUES
    (
        1,
        'Backend Software Engineer',
        'Backend developer with 5+ years of experience building scalable, high-performance REST and gRPC services. Proficient in Go and Java Spring Boot, with strong knowledge of microservices architecture, message queues, and relational/NoSQL databases. Passionate about clean code, domain-driven design, and performance optimization.'
    ),
    (
        2,
        'DevOps / Cloud Engineer',
        'DevOps engineer with 4+ years of experience designing and maintaining CI/CD pipelines, container orchestration on Kubernetes, and cloud infrastructure on AWS. Experienced in infrastructure-as-code (Terraform, Ansible) and platform reliability engineering. Focused on automating everything and reducing MTTR.'
    );

INSERT INTO "ProfilePersonalSummary"
        ("ProfileId", "PersonalSummaryId")
VALUES  (1,           1)
     ,  (2,           2)
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
        "EndDate"
    )
VALUES
    (
        1,
        'Stripe',
        'Backend Engineer',
        'Built and maintained high-throughput backend services for Stripe''s payment processing platform. Led migration of legacy monolith to microservices using Go and Kafka.',
        '2021-06',
        NULL
    ),
    (
        2,
        'Shopify',
        'Junior Backend Developer',
        'Developed e-commerce order management APIs using Java Spring Boot and MySQL. Implemented caching layers with Redis to reduce DB load by 40%.',
        '2019-08',
        '2021-05'
    ),
    (
        3,
        'Stripe',
        'DevOps Engineer',
        'Owned the internal Kubernetes clusters on AWS EKS for 20+ microservices. Implemented GitOps workflow with ArgoCD and reduced deployment time from 45 min to under 8 min.',
        '2022-01',
        NULL
    ),
    (
        4,
        'Amazon Web Services',
        'Infrastructure Engineer Intern',
        'Assisted in provisioning AWS EC2/RDS resources and writing Ansible playbooks for configuration management.',
        '2019-06',
        '2019-08'
    );

INSERT INTO "ProfileWorkExperience"
        ("ProfileId", "WorkExperienceId")
    -- Profile 1 (Backend): Stripe Backend + Shopify
VALUES  (1,           1)
     ,  (1,           2)
    -- Profile 2 (DevOps): Stripe DevOps + AWS Intern
     ,  (2,           3)
     ,  (2,           4)
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
        "EndDate"
    )
VALUES
    (
        1,
        'University of California, Berkeley',
        'Bachelor of Science',
        'Computer Science',
        '2015-09',
        '2019-05'
    );

-- Both profiles share the same education
INSERT INTO "ProfileEducation"
        ("ProfileId", "EducationId")
VALUES  (1,           1)
     ,  (2,           1)
;

-- =============================================================================
-- LANGUAGE
-- =============================================================================

INSERT INTO "Language"
        ("Id", "Name",    "ProficiencyLevel")
VALUES  (1,    'English', 'Native')
     ,  (2,    'Spanish', 'Limited Working Proficiency')
;

-- Both profiles share languages
INSERT INTO "ProfileLanguage"
        ("ProfileId", "LanguageId")
VALUES  (1,           1)
     ,  (1,           2)
     ,  (2,           1)
     ,  (2,           2)
;

-- =============================================================================
-- CERTIFICATION
-- Profile 1 (Backend): Java cert
-- Profile 2 (DevOps): AWS SAA + CKA
-- =============================================================================

INSERT INTO "Certification"
    (
        "Id",
        "Name",
        "Issuer",
        "IssueDate",
        "ExpirationDate"
    )
VALUES
    (
        1,
        'Oracle Certified Professional: Java SE 11 Developer',
        'Oracle',
        '2021-03',
        NULL
    ),
    (
        2,
        'AWS Certified Solutions Architect - Associate',
        'Amazon Web Services',
        '2022-09',
        '2025-09'
    ),
    (
        3,
        'Certified Kubernetes Administrator (CKA)',
        'Cloud Native Computing Foundation',
        '2023-04',
        '2026-04'
    );

INSERT INTO "ProfileCertification"
        ("ProfileId", "CertificationId")
VALUES  (1,           1)  -- Backend profile gets Java cert
     ,  (2,           2)  -- DevOps profile gets AWS SAA
     ,  (2,           3)  -- DevOps profile gets CKA
;

-- =============================================================================
-- SKILLS
-- =============================================================================

INSERT INTO "Skill"
        ("Id", "Name",                         "IsHighlight", "SkillCategoryId")
    -- Programming language
VALUES  (1,    'Go',                           1,             2)
     ,  (2,    'Java',                         1,             2)
     ,  (3,    'Python',                       0,             2)
     ,  (4,    'SQL',                          0,             2)
     ,  (5,    'Bash',                         0,             2)
     ,  (6,    'TypeScript',                   0,             2)
     ,  (7,    'HCL (Terraform)',              1,             2)
    -- Framework / Library
     ,  (8,    'Spring Boot',                  1,             3)
     ,  (9,    'gRPC',                         1,             3)
     ,  (10,   'Gin',                          1,             3)
     ,  (11,   'GORM',                         0,             3)
     ,  (12,   'Ansible',                      1,             3)
     ,  (13,   'ArgoCD',                       1,             3)
     ,  (14,   'Helm',                         0,             3)
    -- Database
     ,  (15,   'PostgreSQL',                   1,             4)
     ,  (16,   'MySQL',                        0,             4)
     ,  (17,   'Redis',                        1,             4)
     ,  (18,   'MongoDB',                      0,             4)
     ,  (19,   'Elasticsearch',                0,             4)
    -- Tool
     ,  (20,   'Docker',                       1,             5)
     ,  (21,   'Kubernetes',                   1,             5)
     ,  (22,   'Kafka',                        1,             5)
     ,  (23,   'Terraform',                    1,             5)
     ,  (24,   'GitHub Actions',               0,             5)
     ,  (25,   'Jenkins',                      0,             5)
     ,  (26,   'Prometheus',                   0,             5)
     ,  (27,   'Grafana',                      0,             5)
     ,  (28,   'Datadog',                      0,             5)
     ,  (29,   'AWS EKS',                      1,             5)
     ,  (30,   'AWS RDS',                      0,             5)
     ,  (31,   'AWS S3',                       0,             5)
     ,  (32,   'AWS Lambda',                   0,             5)
     ,  (33,   'Git',                          0,             5)
     ,  (34,   'Postman',                      0,             5)
    -- OS
     ,  (35,   'Linux (Ubuntu/CentOS)',        1,             6)
    -- Concept
     ,  (36,   'Microservices',                1,             7)
     ,  (37,   'Domain-Driven Design',         0,             7)
     ,  (38,   'RESTful API',                  1,             7)
     ,  (39,   'Event-Driven Architecture',    0,             7)
     ,  (40,   'CI/CD',                        1,             7)
     ,  (41,   'Infrastructure as Code',       1,             7)
     ,  (42,   'GitOps',                       1,             7)
     ,  (43,   'Site Reliability Engineering', 0,             7)
     ,  (44,   'Twelve-Factor App',            0,             7)
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
        "WorkExperienceId",
        "ProjectTypeId"
    )
VALUES
    -- -- PROFILE 1 (Backend) projects ------------------------------------------
    (
        1,
        'Stripe Notification Service',
        'A high-throughput push notification microservice handling 500k+ messages/day for Stripe users across iOS, Android, and Web.',
        'Backend Engineer',
        'Designed the service in Go with a Kafka consumer pipeline. Implemented retry logic, dead-letter queue, and per-device rate limiting. Wrote internal benchmarks achieving <5ms p99 latency.',
        NULL,
        NULL,
        '2022-03',
        NULL,
        0,
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
        '2020-01',
        '2021-05',
        0,
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
        '2023-01',
        NULL,
        0,
        NULL,  -- personal project
        2      -- Personal
    ),

    -- -- PROFILE 2 (DevOps) projects ------------------------------------------
    (
        4,
        'Internal Developer Platform - Stripe',
        'Built a self-service Kubernetes platform for 50+ engineers, abstracting cluster operations behind a GitOps workflow.',
        'DevOps Engineer',
        'Set up AWS EKS clusters with Terraform. Deployed ArgoCD for GitOps. Wrote Helm chart templates for standardized service deployments. Reduced onboarding time for new services from 3 days to 2 hours.',
        NULL,
        NULL,
        '2022-04',
        NULL,
        0,
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
        '2023-06',
        '2024-01',
        0,
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
        '2023-09',
        NULL,
        0,
        NULL,  -- personal
        5      -- OpenSource
    );

-- =============================================================================
-- PROJECT SKILLS
-- Profile 1 projects use backend-focused skills
-- Profile 2 projects use DevOps-focused skills
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
