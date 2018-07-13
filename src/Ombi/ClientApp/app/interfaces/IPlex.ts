﻿export interface IPlexAuthentication {
    user: IPlexUser;
}

export interface IPlexPin {
    id: number;
    code: string;
}

export interface IPlexOAuthViewModel {
    wizard: boolean;
    pin: IPlexPin;
}

export interface IPlexOAuthAccessToken {
    accessToken: string;
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

export interface IPlexLibResponse {
    successful: boolean;
    message: string;
    data: IPlexLibraries;
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
    servers: IPlexServerResult;
}

export interface IPlexServerResult {
    friendlyName: string;
    machineIdentifier: string;
    identifier: string;
    server: IPlexServerResponse[];
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
