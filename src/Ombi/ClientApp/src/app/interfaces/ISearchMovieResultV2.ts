export interface ISearchMovieResultV2 {
    backdropPath: string;
    adult: boolean;
    overview: string;
    budget: number;
    genres: IGenresViewModel[],
    releaseDate: Date;
    revenue: number;
    runtime: number;
    tagline: string;
    productionCompanies: IProductionCompanies[],
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
    status: string;
    videos: IVideos;
    credits: ICreditsViewModel;
    releaseDates: IReleaseDatesDto;
    similar: IOtherMovies;
    recommendations: IOtherMovies;
    plexUrl: string;
    embyUrl: string;
    jellyfinUrl: string;
    quality: string;
    digitalReleaseDate: Date;
    subscribed: boolean;
    showSubscribe: boolean;
    externalIds: IExternalIds;
    keywords: IKeywords;
    belongsToCollection: ICollectionsModel;

    // for the UI
    requestProcessing: boolean;
    processed: boolean;
    background: any;
}


export interface ICollectionsModel {
    id: number;
    name: string;
    posterPath: string;
    backdropPath: string;
}

export interface IKeywords {
    keywordsValue: IKeywordsValue[];
}

export interface IKeywordsValue {
    id: number;
    name: string;
}

export interface IVideos {
    results: IVideoResult[];
}

export interface IExternalIds {
    facebookId: string;
    imdbId: string;
    instagramId: string;
    twitterId: string;
}

export interface IGenresViewModel {
    id: number;
    name: string;
}

export interface IProductionCompanies {
    id: number;
    logo_path: string;
    name: string;
    origin_country: string;
}

export interface IVideoResult {
    id: number;
    iso_639_1: string;
    iso_3166_1: string;
    key: string;
    name: string;
    site: string;
    size: number;
    type: string;
}

export interface ICreditsViewModel {
    cast: ICastViewModel[];
    crew: ICrewViewModel[];
}

export interface ICastViewModel {
    cast_id: number;
    character: string;
    credit_id: string;
    gender: number;
    id: number;
    name: string;
    order: number;
    profile_path: string;
}

export interface ICrewViewModel {
    credit_id: string;
    department: string;
    gender: number;
    id: number;
    job: string;
    name: string;
    profile_path: string;
}

export interface IReleaseDatesDto {
    results: IReleaseResultsDto[];
}

export interface IReleaseResultsDto {
    isoCode: string;
    releaseDate: IReleaseDateDto[];
}

export interface IReleaseDateDto {
    releaseDate: Date;
    type: ReleaseDateType;
}

export enum ReleaseDateType {
    Premiere = 1,
    TheatricalLimited = 2,
    Theatrical = 3,
    Digital = 4,
    Physical = 5,
    Tv = 6
}

export interface IOtherMovies {
    results: IOtherMoviesViewModel[];
}

export interface IOtherMoviesViewModel {
    adult: boolean;
    backdrop_path: string;
    id: number;
    original_language: string;
    original_title: string;
    overview: string;
    poster_path: string;
    release_date: string;
    title: string;
    video: boolean;
    vote_average: number;
    vote_count: number;
    popularity: number;
}
