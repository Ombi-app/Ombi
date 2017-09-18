export interface ISearchMovieResult {
    backdropPath: string;
    adult: boolean;
    overview: string;
    releaseDate: Date;
    genreIds: number[];
    id: number;
    originalTitle: string;
    originalLanguage: string;
    title: string;
    posterPath: string;
    popularity: number;
    voteCount: number;
    video: boolean;
    voteAverage: number;
    alreadyInCp: boolean;
    trailer: string;
    homepage: string;
    imdbId: string;
    approved: boolean;
    requested: boolean;
    available: boolean;
    plexUrl: string;
    quality: string;
}
