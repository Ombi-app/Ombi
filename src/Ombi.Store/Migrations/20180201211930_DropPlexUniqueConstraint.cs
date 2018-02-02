using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Ombi.Store.Migrations
{
    public partial class DropPlexUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"


            CREATE TABLE `PlexServerContent2` (
	`Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`AddedAt`	TEXT NOT NULL,
	`ImdbId`	TEXT,
	`Key`	INTEGER NOT NULL,
	`Quality`	TEXT,
	`ReleaseYear`	TEXT,
	`TheMovieDbId`	TEXT,
	`Title`	TEXT,
	`TvDbId`	TEXT,
	`Type`	INTEGER NOT NULL,
	`Url`	TEXT
);

INSERT INTO PlexServerContent2 (AddedAt, ImdbId, Key, Quality, ReleaseYear, TheMovieDbId, Title, TvDbId, Type, Url)
   SELECT AddedAt, ImdbId, Key, Quality, ReleaseYear, TheMovieDbId, Title, TvDbId, Type, Url FROM PlexServerContent;


CREATE TABLE `PlexEpisode2` (
	`Id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	`EpisodeNumber`	INTEGER NOT NULL,
	`GrandparentKey`	INTEGER NOT NULL,
	`Key`	INTEGER NOT NULL,
	`ParentKey`	INTEGER NOT NULL,
	`SeasonNumber`	INTEGER NOT NULL,
	`Title`	TEXT
);
INSERT INTO PlexEpisode2 (EpisodeNumber, GrandparentKey, Key, ParentKey, SeasonNumber, Title)
   SELECT EpisodeNumber, GrandparentKey, Key, ParentKey, SeasonNumber, Title FROM PlexEpisode;

DROP TABLE PlexEpisode;
DROP TABLE PlexServerContent;

ALTER TABLE PlexServerContent2 RENAME TO PlexServerContent;
ALTER TABLE PlexEpisode2 RENAME TO PlexEpisode;

 PRAGMA foreign_key_check;


                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
