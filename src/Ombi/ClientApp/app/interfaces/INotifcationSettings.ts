export interface ISettings {
    id: number,
}

export interface INotificationSettings extends ISettings {
    enabled: boolean,
}

export interface IEmailNotificationSettings extends INotificationSettings {
    host: string,
    password: string,
    port: number,
    sender: string,
    username: string,
    authentication: boolean,
    adminEmail: string,
    notificationTemplates: INotificationTemplates[],
}

export interface INotificationTemplates {
    subject: string,
    message: string,
    notificationType: NotificationType,
    notificationAgent: NotificationAgent,
    enabled:boolean,
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
    ItemAddedToFaultQueue
}

export interface IDiscordNotifcationSettings extends INotificationSettings{
    webhookUrl : string,
    username: string,
    notificationTemplates: INotificationTemplates[],
}

export interface IPushbulletNotificationSettings extends INotificationSettings {
    accessToken: string,
    notificationTemplates: INotificationTemplates[],
    channelTag: string;
}