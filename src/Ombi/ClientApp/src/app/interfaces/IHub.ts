import { UserType } from "./IUser";

export interface IConnectedUser {
    userId: string;
    displayName: string;
    userType: UserType
}