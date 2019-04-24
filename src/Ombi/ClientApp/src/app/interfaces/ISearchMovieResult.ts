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
    original_name: string;
    id: number;
    media_type: string;
    name: string;
    vote_count: number;
    vote_average: number;
    poster_path: string;
    first_air_date: string;
    popularity: number;
    genre_ids: number[];
    original_language: string;
    backdrop_path: string;
    overview: string;
    origin_country: string[];
    video: true;
    title: string;
    original_title: string;
    adult: true;
    release_date: string;
}

export interface ILanguageRefine {
    code: string;
    name: string;
    nativeName: string;
}

export interface ISearchMovieResultContainer {
    movies: ISearchMovieResult[];
}
