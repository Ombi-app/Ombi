export interface IArtistSearchResult {
    name: string;
    id: string;
    startYear: string;
    endYear: string;
    type: string;
    country: string;
    region: string;
    disambiguation: string;
    releaseGroups: IReleaseGroups[];
    links: IArtistLinks;
    members: IBandMembers[];
}

export interface IReleaseGroups {
    id: string;
    title: string;
    releaseDate: string;
    type: string;
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