export interface DatabaseSettings {
    type: string;
    host: string;
    port: number;
    name: string;
    user: string;
    password: string;
}

export interface DatabaseConfigurationResult {
    success: boolean;
    message: string;
}