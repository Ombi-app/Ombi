--Any DB changes need to be made in this file.

CREATE TABLE IF NOT EXISTS Users
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    UserGuid							varchar(50) NOT NULL ,
    UserName							varchar(50) NOT NULL,
    Salt								BLOB NOT NULL,
    Hash								BLOB NOT NULL,
	UserProperties						BLOB,
	Permissions							INTEGER,
	Features							INTEGER,
	Claims								BLOB
);

CREATE TABLE IF NOT EXISTS UserLogins
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId								varchar(50) NOT NULL ,
    Type								INTEGER NOT NULL,
    LastLoggedIn						varchar(100) NOT NULL
);

CREATE INDEX IF NOT EXISTS UserLogins_UserId ON UserLogins (UserId);

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

CREATE TABLE IF NOT EXISTS VersionInfo
(
    Version									INTEGER NOT NULL,
	Description								VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS ScheduledJobs
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Name								varchar(100) NOT NULL,
    LastRun								varchar(100) NOT NULL,
	Running								INTEGER
);
CREATE UNIQUE INDEX IF NOT EXISTS ScheduledJobs_Id ON ScheduledJobs (Id);

CREATE TABLE IF NOT EXISTS UsersToNotify
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Username							varchar(100) NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS UsersToNotify_Id ON UsersToNotify (Id);

CREATE TABLE IF NOT EXISTS IssueBlobs
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    RequestId							INTEGER,
    Type								INTEGER NOT NULL,
    Content								BLOB NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS IssueBlobs_Id ON IssueBlobs (Id);

CREATE TABLE IF NOT EXISTS RequestLimit
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Username							varchar(100) NOT NULL,
	FirstRequestDate					varchar(100) NOT NULL,
	RequestCount						INTEGER NOT NULL,
	RequestType							INTEGER NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS RequestLimit_Id ON RequestLimit (Id);

CREATE TABLE IF NOT EXISTS PlexUsers
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    PlexUserId							varchar(100) NOT NULL,
	UserAlias							varchar(100) NOT NULL,
	Permissions							INTEGER,
	Features							INTEGER,
	Username							VARCHAR(100),
	EmailAddress						VARCHAR(100),
	LoginId								VARCHAR(100)
);
CREATE UNIQUE INDEX IF NOT EXISTS PlexUsers_Id ON PlexUsers (Id);

BEGIN;
CREATE TABLE IF NOT EXISTS PlexEpisodes
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    EpisodeTitle						VARCHAR(100) NOT NULL,
	ShowTitle							VARCHAR(100) NOT NULL,
	RatingKey							VARCHAR(100) NOT NULL,
	ProviderId							VARCHAR(100) NOT NULL,
	SeasonNumber						INTEGER NOT NULL,
	EpisodeNumber						INTEGER NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS PlexEpisodes_Id ON PlexEpisodes (Id);
CREATE INDEX IF NOT EXISTS PlexEpisodes_ProviderId ON PlexEpisodes (ProviderId);


CREATE TABLE IF NOT EXISTS RequestFaultQueue
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    PrimaryIdentifier					VARCHAR(100) NOT NULL,
	Type								INTEGER NOT NULL,
	FaultType								INTEGER NOT NULL,
    Content								BLOB NOT NULL,
	LastRetry							VARCHAR(100),
	Description							VARCHAR(100)
);
CREATE UNIQUE INDEX IF NOT EXISTS PlexUsers_Id ON PlexUsers (Id);

CREATE TABLE IF NOT EXISTS PlexContent
(
    Id								INTEGER PRIMARY KEY AUTOINCREMENT,
    Title							VARCHAR(100) NOT NULL,
	ReleaseYear						VARCHAR(100) NOT NULL,
	ProviderId						VARCHAR(100) NOT NULL,
	Url								VARCHAR(100) NOT NULL,
	Artist							VARCHAR(100),
	Seasons							BLOB,
	Type							INTEGER NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS PlexContent_Id ON PlexContent (Id);

COMMIT;