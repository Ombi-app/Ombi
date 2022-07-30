import { ICheckbox } from ".";
import { IRemainingRequests } from "./IRemainingRequests";

export interface IUser {
    id: string;
    userName: string;
    alias: string;
    claims: ICheckbox[];
    emailAddress: string;
    password: string;
    userType: UserType;
    userAlias: string;
    lastLoggedIn: Date;
    hasLoggedIn: boolean;
    movieRequestLimit: number;
    episodeRequestLimit: number;
    musicRequestLimit: number;
    userAccessToken: string;
    language: string;
    userQualityProfiles: IUserQualityProfiles;
    streamingCountry: string;

    movieRequestLimitType?: RequestLimitType;
    episodeRequestLimitType?: RequestLimitType;
    musicRequestLimitType?: RequestLimitType;

    // FOR UI
    episodeRequestQuota: IRemainingRequests | null;
    movieRequestQuota: IRemainingRequests | null;
    musicRequestQuota: IRemainingRequests | null;
}

export enum RequestLimitType
{
    Day = 0,
    Week = 1,
    Month = 2,
}

export interface IUserDropdown {
    username: string;
    id: string;
    email: string;
}

export interface IUserQualityProfiles {
    sonarrQualityProfileAnime: number;
    sonarrRootPathAnime: number;
    sonarrRootPath: number;
    sonarrQualityProfile: number;
    radarrRootPath: number;
    radarrQualityProfile: number;
}

export interface ICreateWizardUser {
    username: string;
    password: string;
    usePlexAdminAccount: boolean;
}

export interface IWizardUserResult {
    result: boolean;
    errors: string[];
}

export interface IStreamingCountries {
    code: string;
}

export enum UserType {
    LocalUser = 1,
    PlexUser = 2,
    EmbyUser = 3,
    EmbyConnect = 4,
    JellyfinUser = 5,
}

export interface IIdentityResult {
    errors: string[];
    successful: boolean;
}

export interface IUpdateLocalUser {
    currentPassword: string;
    password: string;
    id: string;
    emailAddress: string;
    confirmNewPassword: string;
}

export interface IResetPasswordToken {
    email: string;
    token: string;
    password: string;
}

export interface IMobileUsersViewModel {
    username: string;
    userId: string;
    devices: number;
}

export interface ICloudMobileModel {
    userId: string;
    username: string;
    devices: ICloudMobileDevices[];
}
export interface ICloudMobileDevices {
    token: string;
    userId: string;
    addedAt: Date;
    user: IUser;
}

export interface IMassEmailUserModel {
    user: IUser;
    selected: boolean;
}

export interface IMassEmailModel {
    subject: string;
    body: string;
    users: IUser[];
    bcc: boolean;
}

export interface INotificationPreferences {
    id: number;
    userId: string;
    agent: INotificationAgent;
    enabled: boolean;
    value: string;
}

export enum INotificationAgent {

    Email = 0,
    Discord = 1,
    Pushbullet = 2,
    Pushover = 3,
    Telegram = 4,
    Slack = 5,
    Mattermost = 6,
    Mobile = 7,
    Gotify = 8,
    Webhook = 9,
    WhatsApp = 10
}

export interface IFeatureEnablement {
    name: string;
    enabled: boolean;
}