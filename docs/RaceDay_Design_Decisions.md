# RaceDay — Key Design Decisions

This document explains the reasoning behind design choices in the database that aren't immediately obvious from the ERD alone.

## 1. Separate Roles table instead of a text column on Users

Rather than adding a plain `Role VARCHAR` column directly to `Users`, I created a dedicated `Roles` lookup table with a `RoleID` foreign key on `Users`. The system requires exactly two fixed roles (Organiser, Participant) that are referenced throughout the application for access control. A lookup table properly normalizes this, prevents inconsistent or misspelled role values being entered, and makes it straightforward to reference a role consistently across the schema and the API's role-based authorization checks.


## 2. Flexible category typing (Age or Distance)

The assignment's functional requirements state categories can be either age-based (e.g. "Under 20", "Senior") or distance-based (e.g. "10km", "21km"). Rather than forcing every category to have a numeric distance value (which wouldn't make sense for an age category), I used a free-text `CategoryName` column paired with a `CategoryType` column restricted to `'Age'` or `'Distance'` via a CHECK constraint. This keeps the table flexible enough to represent both category styles without null or meaningless columns for one type or the other.
