--Any DB changes need to be made in this file.

CREATE TABLE IF NOT EXISTS Users
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    UserGuid							varchar(50) NOT NULL ,
    UserName							varchar(50) NOT NULL,
    Salt								BLOB NOT NULL,
    Hash								BLOB NOT NULL,
	Claims								BLOB NOT NULL,
	UserProperties						BLOB
);


CREATE TABLE IF NOT EXISTS GlobalSettings
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingsName						varchar(50) NOT NULL,
    Content								varchar(100) NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS GlobalSettings_Id ON GlobalSettings (Id);

CREATE TABLE IF NOT EXISTS RequestBlobs
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    ProviderId							INTEGER NOT NULL,
    Type								INTEGER NOT NULL,
    Content								BLOB NOT NULL,
	MusicId								TEXT
);
CREATE UNIQUE INDEX IF NOT EXISTS RequestBlobs_Id ON RequestBlobs (Id);

CREATE TABLE IF NOT EXISTS Logs
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Date								varchar(100) NOT NULL,
    Level								varchar(100) NOT NULL,
    Logger								varchar(100) NOT NULL,
    Message								varchar(100) NOT NULL,
    CallSite							varchar(100) NOT NULL,
    Exception							varchar(100) NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS Logs_Id ON Logs (Id);

CREATE TABLE IF NOT EXISTS Audit
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Date								varchar(100) NOT NULL,
    Username							varchar(100) NOT NULL,
    ChangeType							varchar(100) NOT NULL,
    OldValue							varchar(100),
     NewValue							varchar(100)
);
CREATE UNIQUE INDEX IF NOT EXISTS Audit_Id ON Audit (Id);


CREATE TABLE IF NOT EXISTS DBInfo
(
    SchemaVersion									INTEGER
);

CREATE TABLE IF NOT EXISTS ScheduledJobs
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Name								varchar(100) NOT NULL,
    LastRun								varchar(100) NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ScheduledJobs_Id ON ScheduledJobs (Id);