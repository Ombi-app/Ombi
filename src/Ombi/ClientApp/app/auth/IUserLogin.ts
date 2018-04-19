export interface IUserLogin {
    username: string;
    password: string;
    rememberMe: boolean;
    usePlexOAuth: boolean;
}

export interface ILocalUser {
    roles: string[];
    name: string;
}
