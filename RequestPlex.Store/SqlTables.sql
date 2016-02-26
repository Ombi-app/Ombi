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
    Port								INTEGER NOT NULL
);