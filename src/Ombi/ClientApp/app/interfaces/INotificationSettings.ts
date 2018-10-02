import { ISettings } from "./ICommon";

export interface INotificationSettings extends ISettings {
    enabled: boolean;
}

export interface IEmailNotificationSettings extends INotificationSettings {
    host: string;
    password: string;
    port: number;
    senderAddress: string;
    senderName: string;
    username: string;
    authentication: boolean;
    adminEmail: string;
    disableTLS: boolean;
    disableCertificateChecking: boolean;
    notificationTemplates: INotificationTemplates[];
}

export interface INotificationTemplates {
    subject: string;
    message: string;
    notificationType: NotificationType;
    notificationAgent: NotificationAgent;
    enabled: boolean;
}

export enum NotificationAgent {
    Email,
    Discord,
    Pushbullet,
    Pushover,
    Telegram,
}

export enum NotificationType {
    NewRequest = 0,
    Issue = 1,
    RequestAvailable = 2,
    RequestApproved = 3,
    AdminNote = 4,
    Test = 5,
    RequestDeclined = 6,
    ItemAddedToFaultQueue = 7,
    WelcomeEmail = 8,
    IssueResolved = 9,
    IssueComment = 10,
    Newsletter = 11,
}

export interface IDiscordNotifcationSettings extends INotificationSettings {
    webhookUrl: string;
    username: string;
    notificationTemplates: INotificationTemplates[];
}

export interface INewsletterNotificationSettings extends INotificationSettings {
    notificationTemplate: INotificationTemplates;
    disableMovies: boolean;
    disableTv: boolean;
    disableMusic: boolean;
    externalEmails: string[];
}

export interface ITelegramNotifcationSettings extends INotificationSettings {
    botApi: string;
    chatId: string;
    parseMode: string;
    notificationTemplates: INotificationTemplates[];
}

export interface ISlackNotificationSettings extends INotificationSettings {
    webhookUrl: string;
    username: string;
    channel: string;
    iconEmoji: string;
    iconUrl: string;
    notificationTemplates: INotificationTemplates[];
}

export interface IPushbulletNotificationSettings extends INotificationSettings {
    accessToken: string;
    notificationTemplates: INotificationTemplates[];
    channelTag: string;
}

export interface IPushoverNotificationSettings extends INotificationSettings {
    accessToken: string;
    notificationTemplates: INotificationTemplates[];
    userToken: string;
    priority: number;
    sound: string;
}

export interface IMattermostNotifcationSettings extends INotificationSettings {
    webhookUrl: string;
    username: string;
    channel: string;
    iconUrl: string;
    notificationTemplates: INotificationTemplates[];
}

export interface IMobileNotifcationSettings extends INotificationSettings {
    notificationTemplates: INotificationTemplates[];
}

export interface IMobileNotificationTestSettings {
    settings: IMobileNotifcationSettings;
    userId: string;
}
