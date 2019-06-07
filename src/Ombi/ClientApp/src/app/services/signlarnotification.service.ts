import { Injectable, EventEmitter } from '@angular/core';
import { AuthService } from '../auth/auth.service';

import { HubConnection } from '@aspnet/signalr';
import * as signalR from '@aspnet/signalr';
import { PlatformLocation } from '@angular/common';
import { platformBrowser } from '@angular/platform-browser';

@Injectable()
export class SignalRNotificationService {

    private hubConnection: HubConnection | undefined;
    public Notification: EventEmitter<any>;

    constructor(private authService: AuthService, private platform: PlatformLocation) {
        this.Notification = new EventEmitter<any>();
    }

    public initialize(): void {

        this.stopConnection();
        let url = "/hubs/notification";
        const baseUrl = this.platform.getBaseHrefFromDOM();
        if(baseUrl !== null && baseUrl.length > 1) {
            url = baseUrl + url;
        } 
        this.hubConnection = new signalR.HubConnectionBuilder().withUrl(url, {
            accessTokenFactory: () => {
                return this.authService.getToken();
            }
        }).configureLogging(signalR.LogLevel.Information).build();


        this.hubConnection.on("Notification", (data: any) => {
            this.Notification.emit(data);
        });


        this.hubConnection.start().then((data: any) => {
            console.log('Now connected');
        }).catch((error: any) => {
            console.log('Could not connect ' + error);
            setTimeout(() => this.initialize(), 3000);
        });
    }


    stopConnection() {
        if (this.hubConnection) {
            this.hubConnection.stop();
            this.hubConnection = null;
        }
    };
}