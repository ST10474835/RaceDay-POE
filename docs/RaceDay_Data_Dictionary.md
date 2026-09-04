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


## Events

| Column | Type | Constraints | Purpose |
|---|---|---|---|
| EventID | INT | PK, IDENTITY | Uniquely identifies each event |
| OrganiserID | INT | FK → Users(UserID), NOT NULL | Links the event to the organiser who created it |
| EventName | VARCHAR(150) | NOT NULL | The event's name |
| Description | VARCHAR(500) | NULL | Optional details about the event |
| EventDate | DATE | NOT NULL | The date the event takes place |
| Location | VARCHAR(150) | NOT NULL | Where the event is held |
| Distance | DECIMAL(5,2) | NOT NULL | The event's overall distance |
| EventType | VARCHAR(10) | NOT NULL, CHECK IN ('Run','Walk','Cycle') | Restricts the event to one of three valid types |
| DateCreated | DATETIME | NOT NULL, DEFAULT GETDATE() | Automatically records when the event was created |

## Categories

| Column | Type | Constraints | Purpose |
|---|---|---|---|
| CategoryID | INT | PK, IDENTITY | Uniquely identifies each category |
| EventID | INT | FK → Events(EventID), NOT NULL | Links the category to its parent event |
| CategoryName | VARCHAR(50) | NOT NULL | The category's name (e.g. "10km" or "Under 20") |
| CategoryType | VARCHAR(10) | NOT NULL, CHECK IN ('Age','Distance') | Identifies whether the category is age-based or distance-based |


## Enrolments

| Column | Type | Constraints | Purpose |
|---|---|---|---|
| EnrolmentID | INT | PK, IDENTITY | Uniquely identifies each enrolment |
| ParticipantID | INT | FK → Users(UserID), NOT NULL | Links the enrolment to the participant |
| CategoryID | INT | FK → Categories(CategoryID), NOT NULL | Links the enrolment to the chosen category (and therefore its event) |
| EnrolmentDate | DATETIME | NOT NULL, DEFAULT GETDATE() | Automatically records when the participant enrolled |
| — | — | UNIQUE (ParticipantID, CategoryID) | Prevents a participant from enrolling in the same category twice |

## Results

| Column | Type | Constraints | Purpose |
|---|---|---|---|
| ResultID | INT | PK, IDENTITY | Uniquely identifies each result |
| EnrolmentID | INT | FK → Enrolments(EnrolmentID), NOT NULL, UNIQUE | Links the result to exactly one enrolment |
| FinishTime | TIME | NULL | The participant's finish time |
| Position | INT | NULL | The participant's finishing position |
| DateCaptured | DATETIME | NOT NULL, DEFAULT GETDATE() | Automatically records when the result was entered |
