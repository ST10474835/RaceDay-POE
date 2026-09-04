# RaceDay — Event Management System

## About
RaceDay is a full-stack, API-driven event management system built for the South African road running, walking, and cycling community. It allows Event Organisers to create and manage events, categories, and results, while Participants can browse events, enrol in categories, and track their personal results.

This repository contains **Part 1: System Planning and Database** — the Entity Relationship Diagram, API endpoint plan, and SQL Server database script that form the foundation for the RESTful API (Part 2) and MVC web application (Part 3).

## System Roles

**Organiser**
- Create, edit, and delete events
- Manage event categories
- Capture participant results
- View all event enrolments

**Participant**
- Create an account
- Browse available events
- Enter an event by selecting a category
- View their own enrolments and track their personal results

## Part 1 Contents (`/docs`)
| File | Description | 
|---|---|
| `RaceDay_ERD.png` | Entity Relationship Diagram — 6 entities with PK/FK and cardinality |
| `RaceDay_API_Endpoint_Plan.pdf` | Full API endpoint plan for Part 2 |
| `RaceDay_Database_Script.sql` | SQL Server script — schema creation and seed data |
| `RaceDay_Data_Dictionary.md` | Full column-by-column reference for every table |
| `RaceDay_Design_Decisions.md` | Explanation of key database design choices |

## Database Design
The system is modelled around 6 entities: `Roles`, `Users`, `Events`, `Categories`, `Enrolments`, and `Results`. `Enrolments` acts as the junction table resolving the many-to-many relationship between Participants and Categories. See `/docs/RaceDay_ERD.png` for the full diagram.

## CI/CD
This repository uses GitHub Actions to validate that all required Part 1 planning documents exist in `/docs` on every push.

**Successful build:**

![CI/CD Green Build](docs/cicd-success-screenshot.png)

## Video Walkthrough
[Part 1 Video Walkthrough](YOUR_YOUTUBE_LINK_HERE)

## How to Run the SQL Script
1. Open SQL Server Management Studio (SSMS)
2. Open `docs/RaceDay_Database_Script.sql`
3. Execute the script — it will create the `RaceDayDB` database, all 6 tables, and seed sample data
