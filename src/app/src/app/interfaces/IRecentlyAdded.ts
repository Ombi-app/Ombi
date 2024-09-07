export interface IRecentlyAddedMovies {
    id: number;
    title: string;
    overview: string;
    imdbId: string;
    theMovieDbId: string;
    releaseYear: string;
    addedAt: Date;
    quality: string;

    // For UI only
    posterPath: string;
}

export interface IRecentlyAddedTvShows extends IRecentlyAddedMovies {
    seasonNumber: number;
    episodeNumber: number;
    tvDbId: number;
}

export interface IRecentlyAddedRangeModel {
    from: Date;
    to: Date;
}

export enum RecentlyAddedType {
    Plex,
    Emby,
    Jellyfin,
}
