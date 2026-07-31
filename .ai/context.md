Project: Quaply

Type:
Application

Purpose:
Helping developers generate professional, job-specific CVs through fast checkbox-based selection - no typing, no reformatting, no wasted time.

Core Idea:
Fast CV assembly via checkbox selection; exports to PDF

Tech Stack:
WPF (.NET 10), SQLite, Entity Framework, Dependency Injection

Platform:
Windows only.
Optimize for native Windows development. Do not design around future cross-platform support. Cross-platform compatibility will only be considered after release if it becomes genuinely necessary.

Architecture:
- Ui Layer: MVVM architecture with UI components organized following Atomic Design (Atoms, Molecules, Organisms, Templates, Pages).
- Service Layer: Business logic and use cases.
- Data Layer: Unit Of Work + Repository pattern with local data sources.

Flow:
user reads job description -> open Quaply -> select experience, project, skills, education, certification -> export to PDF

Key Decisions:
- SQLite: bundled with the app for local-only deployment (no separate DB server needed)
- Database-first: schema designed upfront; EF Core models scaffolded from SQLite schema

Status:
Learning-focused, not production-ready - a public release is planned soon.

Meta:
- Team: individual
- Scale: small
- Timeline: unknown (ongoing, See CHANGELOG for details)
- Role: Solo developer; using AI assistants for development (WPF syntax learning, Software Architecture).
