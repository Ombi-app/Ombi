export interface IPlexAuthentication {
    user: IPlexUser;
}

export interface IPlexUser {
    email: string;
    uuid: string;
    username: string;
    title: string;
    authentication_token: string;
}

export interface IPlexLibraries {
    mediaContainer: IMediaContainer;
}

export interface IMediaContainer {
    directory: IDirectory[];
}

export interface IDirectory {
    key: string;
    title: string;
}

export interface IPlexServerViewModel {
    success: boolean;
    message: string;
    servers: IPlexServerResponse;
}

export interface IPlexServerResponse {
    accessToken: string;
    address: string;
    createdAt: string;
    home: string;
    host: string;
    localAddresses: string;
    machineIdentifier: string;
    name: string;
    owned: string;
    ownerId: string;
    port: string;
    scheme: string;
}
