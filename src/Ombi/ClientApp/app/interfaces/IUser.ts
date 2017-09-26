import { ICheckbox } from "./index";

export interface IUser {
    id: string;
    username: string;
    alias: string;
    claims: ICheckbox[];
    emailAddress: string;
    password: string;
    userType: UserType;
    isSetup: boolean;
    // FOR UI
    checked: boolean;
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
