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
    requestId: number;
    available: boolean;
    plexUrl: string;
    embyUrl: string;
    jellyfinUrl: string;
    quality: string;
    digitalReleaseDate: Date;
    subscribed: boolean;
    showSubscribe: boolean;

    // for the UI
    requestProcessing: boolean;
    processed: boolean;
    background: any;
}

export interface IMultiSearchResult {
    id: string;
   mediaType: string;
   title: string;
   poster: string;
}

export interface ILanguageRefine {
    code: string;
    name: string;
    nativeName: string;
}

export interface ISearchMovieResultContainer {
    movies: ISearchMovieResult[];
}
