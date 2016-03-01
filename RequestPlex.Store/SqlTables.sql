--Any DB changes need to be made in this file.

CREATE TABLE IF NOT EXISTS User
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    User								varchar(50) NOT NULL ,
    UserName							varchar(50) NOT NULL,
    Password							varchar(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS Settings
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Port								INTEGER NOT NULL,
	UserAuthentication					INTEGER NOT NULL,
	PlexAuthToken						varchar(50)				
);

CREATE TABLE IF NOT EXISTS Requested
(
	Id									INTEGER PRIMARY KEY AUTOINCREMENT,
	Type								INTEGER NOT NULL,
    Tmdbid								INTEGER NOT NULL,
	ImdbId								varchar(50) NOT NULL,
	Overview							varchar(50) NOT NULL,
	Title								varchar(50) NOT NULL,
	PosterPath							varchar(50) NOT NULL,
	ReleaseDate							varchar(50) NOT NULL,
	Status								varchar(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS GlobalSettings
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingsName						varchar(50) NOT NULL,
    Content								varchar(100) NOT NULL
);