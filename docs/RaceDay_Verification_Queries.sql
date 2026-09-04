-- =========================================================
-- RaceDay Verification Queries
-- These queries confirm the database, tables, and sample
-- data were created correctly.
-- =========================================================

-- 1. Confirm all tables exist with row counts
SELECT
    t.name AS TableName,
    p.rows AS RowCount
FROM sys.tables t
JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
ORDER BY t.name;


-- 2. Users with their role names (tests Users -> Roles FK)
SELECT
    Users.UserID,
    Users.FullName,
    Users.Email,
    Roles.RoleName,
    Users.DateRegistered
FROM Users
INNER JOIN Roles ON Users.RoleID = Roles.RoleID
ORDER BY Users.UserID;

-- 3. Events with organiser names (tests Events -> Users FK)
SELECT
    Events.EventID,
    Events.EventName,
    Events.EventDate,
    Events.Location,
    Events.EventType,
    Users.FullName AS OrganiserName
FROM Events
INNER JOIN Users ON Events.OrganiserID = Users.UserID
ORDER BY Events.EventDate;

-- 4. Categories per event (tests Categories -> Events FK)
SELECT
    Events.EventName,
    Categories.CategoryName,
    Categories.CategoryType
FROM Categories
INNER JOIN Events ON Categories.EventID = Events.EventID
ORDER BY Events.EventName, Categories.CategoryName;

-- 5. Full enrolment chain: Participant -> Category -> Event
SELECT
    Users.FullName AS ParticipantName,
    Events.EventName,
    Categories.CategoryName,
    Enrolments.EnrolmentDate
FROM Enrolments
INNER JOIN Users ON Enrolments.ParticipantID = Users.UserID
INNER JOIN Categories ON Enrolments.CategoryID = Categories.CategoryID
INNER JOIN Events ON Categories.EventID = Events.EventID
ORDER BY Events.EventName, Users.FullName;

-- 6. Results linked back through the full chain
SELECT
    Users.FullName AS ParticipantName,
    Events.EventName,
    Categories.CategoryName,
    Results.FinishTime,
    Results.Position
FROM Results
INNER JOIN Enrolments ON Results.EnrolmentID = Enrolments.EnrolmentID
INNER JOIN Users ON Enrolments.ParticipantID = Users.UserID
INNER JOIN Categories ON Enrolments.CategoryID = Categories.CategoryID
INNER JOIN Events ON Categories.EventID = Events.EventID
ORDER BY Events.EventName;


-- 7. Constraint enforcement test:
-- This INSERT is EXPECTED TO FAIL, proving the UNIQUE
-- constraint on (ParticipantID, CategoryID) in Enrolments
-- correctly prevents duplicate enrolments.
-- (ParticipantID 3, CategoryID 1 already exists in seed data.)
INSERT INTO Enrolments (ParticipantID, CategoryID) VALUES (3, 1);
