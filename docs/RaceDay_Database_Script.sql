CREATE DATABASE RaceDayDB;

CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(30) NOT NULL UNIQUE
);


CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(160) NOT NULL UNIQUE,
    PasswordHash VARCHAR(250) NOT NULL,
    RoleID INT NOT NULL,
    DateRegistered DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);


CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    Description VARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventType VARCHAR(10) NOT NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES Users(UserID),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Run', 'Walk', 'Cycle'))
);


CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(50) NOT NULL,
    CategoryType VARCHAR(10) NOT NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT CK_Categories_CategoryType CHECK (CategoryType IN ('Age', 'Distance'))
);


CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantID, CategoryID)
);


CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME NULL,
    Position INT NULL,
    DateCaptured DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);

INSERT INTO Roles (RoleName) VALUES
('Organiser'),
('Participant');

INSERT INTO Users (FullName, Email, PasswordHash, RoleID) VALUES
('Asemahle Mhambi', 'asemahle.mhambi@raceday.co.za', 'HASHED_PW_1', 1),
('Vuyo Cosa', 'vuyo.cosa@raceday.co.za', 'HASHED_PW_2', 1),
('Onkgo Molefe', 'onkgo.molefe@raceday.co.za', 'HASHED_PW_3', 2),
('Bongani Sefo', 'bongani.sefo@raceday.co.za', 'HASHED_PW_4', 2),
('Unathi Zondo', 'unathi.zondo@raceday.co.za', 'HASHED_PW_5', 2);

INSERT INTO Events (OrganiserID, EventName, Description, EventDate, Location, Distance, EventType) VALUES
(1, 'Johannesburg City Marathon', 'Annual road marathon through the Johannesburg CBD.', '2026-11-15', 'Johannesburg, Gauteng', 42.20, 'Run'),
(1, 'Diepsloot Fun Walk', 'Community fun walk raising funds for local schools.', '2026-10-04', 'Diepsloot, Gauteng', 5.00, 'Walk'),
(2, 'Cape Winelands Cycle Tour', 'Scenic cycling tour through the Cape Winelands.', '2026-12-06', 'Stellenbosch, Western Cape', 100.00, 'Cycle');

INSERT INTO Categories (EventID, CategoryName, CategoryType) VALUES
(1, '10km', 'Distance'),
(1, 'Half Marathon (21km)', 'Distance'),
(1, 'Full Marathon (42km)', 'Distance'),
(2, 'Under 20', 'Age'),
(2, 'Senior', 'Age'),
(3, '100km', 'Distance');

INSERT INTO Enrolments (ParticipantID, CategoryID) VALUES
(3, 1),
(4, 2),
(5, 4),
(3, 6);

INSERT INTO Results (EnrolmentID, FinishTime, Position) VALUES
(1, '00:48:32', 5),
(2, '01:52:10', 12);