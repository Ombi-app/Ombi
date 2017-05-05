
export interface IPlexAuthentication {
    user: IPlexUser
}

export interface IPlexUser {
    email: string,
    uuid: string,
    username: string,
    title: string,
    authentication_token: string,
}

export interface IPlexLibraries {
    mediaContainer:IMediaContainer;
}

export interface IMediaContainer {
    directory:IDirectory[];
}

export interface IDirectory {
    key: string,
    title: string,
}
