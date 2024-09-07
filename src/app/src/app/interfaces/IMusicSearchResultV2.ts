export interface IArtistSearchResult {
    name: string;
    id: string;
    startYear: string;
    endYear: string;
    type: string;
    country: string;
    region: string;
    disambiguation: string;
    banner: string;
    logo: string;
    poster: string;
    fanArt: string;
    releaseGroups: IReleaseGroups[];
    links: IArtistLinks;
    members: IBandMembers[];
    overview: string;

    background: any;
}

export interface IReleaseGroups {
    id: string;
    title: string;
    releaseDate: string;
    releaseType: string; 
    approved: boolean;
    requested: boolean;
    requestId: number;
    available: boolean; 
    subscribed: boolean;
    showSubscribe: boolean;
    monitored: boolean;
    partiallyAvailable: boolean;
    fullyAvailable: boolean;

    image: string; // Set by another api call

    selected: boolean; // Set via UI
}

export interface IArtistLinks {
    image: string;
    imdb: string;
    lastFm: string;
    discogs: string;
    allMusic: string;
    homePage: string;
    youTube: string;
    facebook: string;
    twitter: string;
    bbcMusic: string;
    mySpace: string;
    onlineCommunity: string;
    spotify: string;
    instagram: string;
    vk: string;
    deezer: string;
    google: string;
    apple: string;
}

export interface IBandMembers {
    name: string;
    attributes: string[];
    isCurrentMember: boolean;
    start: string;
    end: string;
}

export interface IAlbumArt {
    image: string;
}