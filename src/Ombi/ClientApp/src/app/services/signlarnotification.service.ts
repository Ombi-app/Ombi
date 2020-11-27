import { Injectable, EventEmitter } from '@angular/core';
import { AuthService } from '../auth/auth.service';

import { HubConnection } from '@aspnet/signalr';
import * as signalR from '@aspnet/signalr';

@Injectable()
export class SignalRNotificationService {

    private hubConnection: HubConnection | undefined;
    public Notification: EventEmitter<any>;

    constructor(private authService: AuthService) {
        this.Notification = new EventEmitter<any>();
    }

    public initialize(): void {

        this.stopConnection();
        let url = "hubs/notification";
        this.hubConnection = new signalR.HubConnectionBuilder().withUrl(url, {
            accessTokenFactory: () => {
                return this.authService.getToken();
            }
        }).configureLogging(signalR.LogLevel.Information).build();


        this.hubConnection.on("Notification", (data: any) => {
            this.Notification.emit(data);
        });

        let retryCount = 0;

        this.hubConnection.start().then((data: any) => {
            console.log('Now connected');
        }).catch((error: any) => {
            retryCount++;
            console.log('Could not connect ' + error);
            if (retryCount <= 3) {
                setTimeout(() => this.initialize(), 3000);
            }
        });
    }


    stopConnection() {
        if (this.hubConnection) {
            this.hubConnection.stop();
            this.hubConnection = null;
        }
    };
}