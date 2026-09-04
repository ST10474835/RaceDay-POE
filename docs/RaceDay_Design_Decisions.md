# RaceDay — Key Design Decisions

This document explains the reasoning behind design choices in the database that aren't immediately obvious from the ERD alone.

## 1. Separate Roles table instead of a text column on Users

Rather than adding a plain `Role VARCHAR` column directly to `Users`, I created a dedicated `Roles` lookup table with a `RoleID` foreign key on `Users`. The system requires exactly two fixed roles (Organiser, Participant) that are referenced throughout the application for access control. A lookup table properly normalizes this, prevents inconsistent or misspelled role values being entered, and makes it straightforward to reference a role consistently across the schema and the API's role-based authorization checks.
