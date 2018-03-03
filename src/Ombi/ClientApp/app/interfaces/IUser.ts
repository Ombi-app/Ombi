import { ICheckbox } from "./index";

export interface IUser {
    id: string;
    userName: string;
    alias: string;
    claims: ICheckbox[];
    emailAddress: string;
    password: string;
    userType: UserType;
    lastLoggedIn: Date;
    hasLoggedIn: boolean;
    movieRequestLimit: number;
    episodeRequestLimit: number;
    userAccessToken: string;
    // FOR UI
    checked: boolean;
}

export interface ICreateWizardUser {
    username: string;
    password: string;
    usePlexAdminAccount: boolean;
}

export enum UserType {
    LocalUser = 1,
    PlexUser = 2,
    EmbyUser = 3,
}

export interface IIdentityResult {
    errors: string[];
    successful: boolean;
}

export interface IUpdateLocalUser extends IUser {
    currentPassword: string;
    confirmNewPassword: string;
}

export interface IResetPasswordToken {
    email: string;
    token: string;
    password: string;
}

export interface IMobileUsersViewModel {
    username: string;
    devices: number;
}

export interface IMassEmailUserModel {
    user: IUser;
    selected: boolean;
}

export interface IMassEmailModel {
    subject: string;
    body: string;
    users: IUser[];
}
