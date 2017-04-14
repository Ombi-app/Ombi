
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
