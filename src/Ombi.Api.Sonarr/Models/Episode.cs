using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Api.Sonarr.Models
{
    public class Episode
    {
        public Episode()
        {
            
        }

        public Episode(Episode ep)
        {
            seriesId = ep.seriesId;
            episodeFileId = ep.episodeFileId;
            seasonNumber = ep.seasonNumber;
            episodeNumber = ep.episodeNumber;
            title = ep.title;
            airDate = ep.airDate;
            airDateUtc = ep.airDateUtc;
            overview = ep.overview;
            hasFile = ep.hasFile;
            monitored = ep.monitored;
            unverifiedSceneNumbering = ep.unverifiedSceneNumbering;
            id = ep.id;
            absoluteEpisodeNumber = ep.absoluteEpisodeNumber;
            sceneAbsoluteEpisodeNumber = ep.sceneAbsoluteEpisodeNumber;
            sceneEpisodeNumber = ep.sceneEpisodeNumber;
            sceneSeasonNumber = ep.sceneSeasonNumber;
        }
        public int seriesId { get; set; }
        public int episodeFileId { get; set; }
        public int seasonNumber { get; set; }
        public int episodeNumber { get; set; }
        public string title { get; set; }
        public string airDate { get; set; }
        public DateTime airDateUtc { get; set; }
        public string overview { get; set; }
        public bool hasFile { get; set; }
        public bool monitored { get; set; }
        public bool unverifiedSceneNumbering { get; set; }
        public int id { get; set; }
        public int absoluteEpisodeNumber { get; set; }
        public int sceneAbsoluteEpisodeNumber { get; set; }
        public int sceneEpisodeNumber { get; set; }
        public int sceneSeasonNumber { get; set; }
        public Episodefile episodeFile { get; set; }
    }

    public class Episodefile
    {
        public Episodefile()
        {
            
        }

        public Episodefile(Episodefile e)
        {
            seriesId = e.seriesId;
            seasonNumber = e.seasonNumber;
            relativePath = e.relativePath;
            path = e.path;
            size = e.size;
            dateAdded = e.dateAdded;
            sceneName = e.sceneName;
            quality = new EpisodeQuality(e.quality);
            qualityCutoffNotMet = e.qualityCutoffNotMet;
            id = e.id;
        }
        public int seriesId { get; set; }
        public int seasonNumber { get; set; }
        public string relativePath { get; set; }
        public string path { get; set; }
        public long size { get; set; }
        public DateTime dateAdded { get; set; }
        public string sceneName { get; set; }
        public EpisodeQuality quality { get; set; }
        public bool qualityCutoffNotMet { get; set; }
        public int id { get; set; }
    }

    public class EpisodeQuality
    {
        public EpisodeQuality()
        {
            
        }

        public EpisodeQuality(EpisodeQuality e)
        {
            quality = new Quality(e.quality);
            revision = new Revision(e.revision);
        }
        public Quality quality { get; set; }
        public Revision revision { get; set; }
    }

    public class Revision
    {
        public Revision()
        {
            
        }

        public Revision(Revision r)
        {
            version = r.version;
            real = r.real;
        }
        public int version { get; set; }
        public int real { get; set; }
    }


    public class EpisodeUpdateResult
    {
        public int seriesId { get; set; }
        public int episodeFileId { get; set; }
        public int seasonNumber { get; set; }
        public int episodeNumber { get; set; }
        public string title { get; set; }
        public string airDate { get; set; }
        public DateTime airDateUtc { get; set; }
        public string overview { get; set; }
        public bool hasFile { get; set; }
        public bool monitored { get; set; }
        public int sceneEpisodeNumber { get; set; }
        public int sceneSeasonNumber { get; set; }
        public int tvDbEpisodeId { get; set; }
        public int absoluteEpisodeNumber { get; set; }
        public bool downloading { get; set; }
        public int id { get; set; }
    }


}
