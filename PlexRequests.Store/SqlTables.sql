--Any DB changes need to be made in this file.

CREATE TABLE IF NOT EXISTS User
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    User								varchar(50) NOT NULL ,
    UserName							varchar(50) NOT NULL,
    Password							varchar(100) NOT NULL
);


CREATE TABLE IF NOT EXISTS GlobalSettings
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingsName						varchar(50) NOT NULL,
    Content								varchar(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS RequestBlobs
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    ProviderId							INTEGER NOT NULL,
    Type								INTEGER NOT NULL,
    Content								BLOB NOT NULL
);


CREATE TABLE IF NOT EXISTS Log
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Username							varchar(50) NOT NULL,
    Date								varchar(100) NOT NULL,
    Level								varchar(100) NOT NULL,
    Logger								varchar(100) NOT NULL,
    Message								varchar(100) NOT NULL,
    CallSite							varchar(100) NOT NULL,
    Exception							varchar(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS Requested
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
	Type								INTEGER NOT NULL,
	ProviderId							INTEGER NOT NULL,
	ImdbId varchar(50),
	Overview varchar(50),
	Title varchar(50) NOT NULL,
	PosterPath varchar(50) NOT NULL,
	ReleaseDate varchar(50) NOT NULL,
	Status varchar(50) NOT NULL,
	AdminNote varchar(50),
	Approved INTEGER NOT NULL,
	LatestTv INTEGER NOT NULL,
	RequestedBy varchar(50),
	RequestedDate varchar(50) NOT NULL,
	Available INTEGER(50),
	Issues INTEGER,
	OtherMessage varchar(50)
);