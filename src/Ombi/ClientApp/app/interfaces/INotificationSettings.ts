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
    NewRequest,
    Issue,
    RequestAvailable,
    RequestApproved,
    AdminNote,
    Test,
    RequestDeclined,
    ItemAddedToFaultQueue,
    WelcomeEmail,
}

export interface IDiscordNotifcationSettings extends INotificationSettings {
    webhookUrl: string;
    username: string;
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
}

export interface IMattermostNotifcationSettings extends INotificationSettings {
    webhookUrl: string;
    username: string;
    channel: string;
    iconUrl: string;
    notificationTemplates: INotificationTemplates[];
}
