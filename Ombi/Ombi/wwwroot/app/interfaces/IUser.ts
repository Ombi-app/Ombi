export interface IUser {
    id: string,
    username: string,
    alias: string,
    claims: string[],
    emailAddress: string,
    password: string,
    userType : UserType,
}


export enum UserType {
    LocalUser = 1,
    PlexUser = 2,
    EmbyUser = 3
}
