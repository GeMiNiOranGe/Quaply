Project: Quaply

Type:
Application

Purpose:
Helping developers generate professional, job-specific CVs through fast checkbox-based selection - no typing, no reformatting, no wasted time.

Core Idea:
Fast CV assembly via checkbox selection; exports to PDF

Tech Stack:
WPF (.NET 10), SQLite, Entity Framework

Platform:
Windows only

Architecture:
MVVM

Flow:
user reads job description -> open Quaply -> select experience, project, skills, education, certification -> export to PDF

Key Decisions:
SQLite: bundled with the app for local-only deployment (no separate DB server needed)

Status:
Learning-focused, not production-ready

Meta:
- Team: individual
- Scale: small
- Timeline: unknown (ongoing, See CHANGELOG for details)
- Role: Solo developer; used AI assistance for WPF syntax learning.
