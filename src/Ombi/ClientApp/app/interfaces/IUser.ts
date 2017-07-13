export interface IUser {
    id: string,
    username: string,
    alias: string,
    claims: ICheckbox[],
    emailAddress: string,
    password: string,
    userType: UserType,
}

export enum UserType {
    LocalUser = 1,
    PlexUser = 2,
    EmbyUser = 3
}


export interface ICheckbox {
    value: string,
    enabled: boolean,
}

export interface IIdentityResult {
    errors: string[],
    successful: boolean,
}

export interface IUpdateLocalUser extends IUser {
    currentPassword: string,
    confirmNewPassword: string
}

