export interface IUserLogin {
    username: string;
    password: string;
    rememberMe: boolean;
}

export interface ILocalUser {
    roles: string[];
    name: string;
}
