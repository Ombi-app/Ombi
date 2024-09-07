import { IPlexPin } from "../interfaces";

export interface IUserLogin {
    username: string;
    password: string;
    rememberMe: boolean;
    usePlexOAuth: boolean;
    plexTvPin: IPlexPin;
}

export interface ILocalUser {
    roles: string[];
    name: string;
    username:string;
    email: string;
}
