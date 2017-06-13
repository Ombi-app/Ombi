CREATE TABLE IF NOT EXISTS GlobalSettings
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    SettingsName						varchar(50) NOT NULL,
    Content								BLOB NOT NULL
);

CREATE TABLE IF NOT EXISTS PlexContent
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Title						varchar(50) NOT NULL,
    ProviderId						varchar(50) NOT NULL,
    Url						varchar(100) NOT NULL,
    Key						varchar(50) NOT NULL,
    AddedAt						varchar(50) NOT NULL,
    Type						INTEGER NOT NULL,
    ReleaseYear								varchar(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS RadarrCache
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    TheMovieDbId						INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS PlexSeasonsContent
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
	PlexContentId						integer not null,
    SeasonNumber									INTEGER NOT NULL,
    SeasonKey									INTEGER NOT NULL,
    ParentKey							INTEGER NOT NULL

);

CREATE TABLE IF NOT EXISTS RequestBlobs
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    ProviderId									INTEGER NOT NULL,
    Content									BLOB NOT NULL,
    Type							INTEGER NOT NULL

);

CREATE TABLE IF NOT EXISTS Users
(
    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Username									VARCHAR(100) NOT NULL,
    Alias									VARCHAR(100) NULL,
    ClaimsSerialized									BLOB NOT NULL,
    EmailAddress									VARCHAR(100) NULL,
    Password									VARCHAR(100) NULL,
    Salt									BLOB NULL,
    UserType									INTEGER NOT NULL

);

CREATE TABLE IF NOT EXISTS RequestHistory
(

    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    Type						INTEGER NOT NULL,
    RequestedDate						varchar(50) NOT NULL, 
	RequestId						INTEGER NOT NULL

);