export interface IRecentlyAddedMovies {
    id: number;
    title: string;
    overview: string;
    imdbId: string;
    theMovieDbId: string;
    releaseYear: string;
    addedAt: Date;
    quality: string;
}

export interface IRecentlyAddedRangeModel {
    from: Date;
    to: Date; 
}

export enum RecentlyAddedType {
    Plex,
    Emby,
}
