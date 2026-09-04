# RaceDay — Key Design Decisions

This document explains the reasoning behind design choices in the database that aren't immediately obvious from the ERD alone.

## 1. Separate Roles table instead of a text column on Users

Rather than adding a plain `Role VARCHAR` column directly to `Users`, I created a dedicated `Roles` lookup table with a `RoleID` foreign key on `Users`. The system requires exactly two fixed roles (Organiser, Participant) that are referenced throughout the application for access control. A lookup table properly normalizes this, prevents inconsistent or misspelled role values being entered, and makes it straightforward to reference a role consistently across the schema and the API's role-based authorization checks.


## 2. Flexible category typing (Age or Distance)

The assignment's functional requirements state categories can be either age-based (e.g. "Under 20", "Senior") or distance-based (e.g. "10km", "21km"). Rather than forcing every category to have a numeric distance value (which wouldn't make sense for an age category), I used a free-text `CategoryName` column paired with a `CategoryType` column restricted to `'Age'` or `'Distance'` via a CHECK constraint. This keeps the table flexible enough to represent both category styles without null or meaningless columns for one type or the other.


## 3. Avoiding a redundant EventID on Enrolments

The functional requirements state the system must record the link between the Participant, the Event, and the selected Category. Since every `Category` already belongs to exactly one `Event` (via `Categories.EventID`), the event is always derivable by joining `Enrolments` → `Categories` → `Events`. I deliberately did not add a second, duplicate `EventID` column directly on `Enrolments`, since storing the same relationship in two places risks the two values going out of sync — a normalization anti-pattern. The event is still fully queryable and correctly enforced through the existing foreign key chain.
