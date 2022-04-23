export interface IMovieDbKeyword {
    id: number;
    name: string;
}

export interface IWatchProvidersResults {
    provider_id: number;
    logo_path: string;
    provider_name: string;
}

export interface IDiscoverModel {
    type: string;
    releaseYear?: number|undefined;
    genreIds?: number[];
    keywordIds?: number[];
    watchProviders?: number[];
    companies?: number[];
}
export interface ILanguage {
    iso_639_1 : string;
    english_name : string;
    name : string;
}
