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

CREATE TABLE IF NOT EXISTS NotificationTemplates
(

    Id									INTEGER PRIMARY KEY AUTOINCREMENT,
    NotificationType						INTEGER NOT NULL,
    Agent						INTEGER NOT NULL,
    Subject						BLOB NULL, 
    Message						BLOB  NULL,
	Enabled						INTEGER  NOT NULL

);

CREATE TABLE IF NOT EXISTS MovieIssues
(

    Id							INTEGER PRIMARY KEY AUTOINCREMENT,
    Subject						INTEGER NOT NULL,
    Description					INTEGER NOT NULL,
	MovieId						INTEGER NOT NULL,
	
    FOREIGN KEY (MovieId) REFERENCES MovieRequests(Id)
);

CREATE TABLE IF NOT EXISTS TvIssues
(

    Id							INTEGER PRIMARY KEY AUTOINCREMENT,
    Subject						INTEGER NOT NULL,
    Description					INTEGER NOT NULL,
	ChildId						INTEGER NOT NULL,
	
    FOREIGN KEY (ChildId) REFERENCES TvChildRequests(ChildId)
);


CREATE  TABLE IF NOT EXISTS MovieRequests ( 
    Id					INTEGER PRIMARY KEY AUTOINCREMENT, 
    ImdbId				VARCHAR(20) NOT NULL,
    TheMovieDbId		INTEGER NOT NULL,
    Overview			VARCHAR(100) NOT NULL,
	Title				VARCHAR(50) NOT NULL,
	PosterPath			VARCHAR(100) NOT NULL,
	ReleaseDate			VARCHAR(100) NOT NULL,
	Status				VARCHAR(100) NOT NULL,
	Approved			INTEGER NOT NULL,
	Available			INTEGER NOT NULL,
	RequestedDate		VARCHAR(100) NOT NULL,
	RequestedUserId		INTEGER NOT NULL,
	IssueId				INTEGER NULL,
	Denied				INTEGER NULL,
	DeniedReason		VARCHAR(100) NULL,
	RequestType			INTEGER NOT NULL,

    FOREIGN KEY (IssueId) REFERENCES MovieIssues(Id),
    FOREIGN KEY (RequestedUserId) REFERENCES Users(Id)
);

CREATE  TABLE IF NOT EXISTS TvRequests ( 
    Id					INTEGER PRIMARY KEY AUTOINCREMENT, 
    ImdbId				VARCHAR(20) NOT NULL,
    TvDbId		INTEGER NOT NULL,
    Overview			VARCHAR(100) NOT NULL,
	Title				VARCHAR(50) NOT NULL,
	PosterPath			VARCHAR(100) NOT NULL,
	ReleaseDate			VARCHAR(100) NOT NULL,
	Status				VARCHAR(100) NULL,
	RootFolder			INTEGER NULL
);

CREATE  TABLE IF NOT EXISTS ChildRequests ( 
    Id					INTEGER PRIMARY KEY AUTOINCREMENT, 
	Approved			INTEGER NOT NULL,
	Available			INTEGER NOT NULL,
	RequestedDate		VARCHAR(100) NOT NULL,
	RequestedUserId		INTEGER NOT NULL,
	IssueId				INTEGER NULL,
	Denied				INTEGER NULL,
	DeniedReason		VARCHAR(100) NULL,
	ParentRequestId		INTEGER NOT NULL,
	RequestType			INTEGER NOT NULL,

    FOREIGN KEY (IssueId) REFERENCES TvIssues(Id),
    FOREIGN KEY (ParentRequestId) REFERENCES TvRequests(Id),
    FOREIGN KEY (RequestedUserId) REFERENCES Users(Id)
);

CREATE  TABLE IF NOT EXISTS SeasonRequests ( 
    Id					INTEGER PRIMARY KEY AUTOINCREMENT, 
	SeasonNumber		INTEGER NOT NULL,
	ChildRequestId			INTEGER NOT NULL,
	
    FOREIGN KEY (ChildRequestId) REFERENCES ChildRequests(Id)
);

CREATE  TABLE IF NOT EXISTS EpisodeRequests ( 
    Id					INTEGER PRIMARY KEY AUTOINCREMENT, 
	EpisodeNumber			INTEGER NOT NULL,
	Title		VARCHAR(100) NOT NULL,
	AirDate		VARCHAR(100) NOT NULL,
	Url		VARCHAR(100) NOT NULL,
	SeasonId INTEGER NOT NULL,
	Available INTEGER NOT NULL,
	Requested INTEGER NOT NULL,
	Approved INTEGER NOT NULL,
	

    FOREIGN KEY (SeasonId) REFERENCES SeasonRequests(Id)
);
