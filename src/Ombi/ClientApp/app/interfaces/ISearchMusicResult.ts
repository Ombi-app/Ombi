export interface ISearchArtistResult {
    artistName: string;
    forignArtistId: string;

    banner: string;
    overview: string;
    poster: string;
    monitored: boolean;
    approved: boolean;
    requested: boolean;
    requestId: number;
    available: boolean;
   
    subscribed: boolean;
    showSubscribe: boolean;

    // for the UI
    requestProcessing: boolean;
    processed: boolean;
    background: any;
}
