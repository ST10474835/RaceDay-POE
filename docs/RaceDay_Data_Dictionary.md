# RaceDay — Data Dictionary

Full column-by-column reference for every table in the RaceDay database, matching `RaceDay_ERD.png` and `RaceDay_Database_Script.sql` exactly.

## Roles

| Column | Type | Constraints | Purpose |
|---|---|---|---|
| RoleID | INT | PK, IDENTITY | Uniquely identifies each role |
| RoleName | VARCHAR(20) | NOT NULL, UNIQUE | The role name ("Organiser" or "Participant") |

## Users

| Column | Type | Constraints | Purpose |
|---|---|---|---|
| UserID | INT | PK, IDENTITY | Uniquely identifies each user |
| FullName | VARCHAR(100) | NOT NULL | The user's full name |
| Email | VARCHAR(150) | NOT NULL, UNIQUE | The user's login email — must be unique to prevent duplicate accounts |
| PasswordHash | VARCHAR(255) | NOT NULL | The hashed (never plain-text) password |
| RoleID | INT | FK → Roles(RoleID), NOT NULL | Links the user to their role (Organiser or Participant) |
| DateRegistered | DATETIME | NOT NULL, DEFAULT GETDATE() | Automatically records when the account was created |
