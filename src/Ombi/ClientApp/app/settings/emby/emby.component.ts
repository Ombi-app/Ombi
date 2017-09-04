import { Component, OnInit } from '@angular/core';

import { IEmbySettings, IEmbyServer } from '../../interfaces/ISettings'
import { SettingsService } from '../../services/settings.service';
import { NotificationService } from "../../services/notification.service";
import { TesterService } from '../../services/applications/tester.service';

@Component({
    templateUrl: './emby.component.html'
})
export class EmbyComponent implements OnInit {

    constructor(private settingsService: SettingsService,
        private notificationService: NotificationService,
    private testerService : TesterService) { }

    settings: IEmbySettings;

    ngOnInit(): void {
        this.settingsService.getEmby().subscribe(x => this.settings = x);
    }

    addTab() {
        if (this.settings.servers == null) {
            this.settings.servers = [];
        } 
        this.settings.servers.push({
            name: "New*",
            id: Math.floor(Math.random() * (99999 - 0 + 1) + 1),
            apiKey: "",
            administratorId: "",
            enableEpisodeSearching: false,
            ip: "",
            port: 0,
            ssl: false,
            subDir: ""
        } as IEmbyServer);
    }

    test(server: IEmbyServer) {
        this.testerService.embyTest(server).subscribe(x => {
            if (x) {
                this.notificationService.success("Connected", `Successfully connected to the Emby server ${server.name}!`);
            } else {
                this.notificationService.error("Connected", `We could not connect to the Emby server  ${server.name}!`);
            }
        });
    }

    removeServer(server: IEmbyServer) {
        var index = this.settings.servers.indexOf(server, 0);
        if (index > -1) {
            this.settings.servers.splice(index, 1);
        }
    }


    save() {
        this.settingsService.saveEmby(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Emby settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Emby settings");
            }
        });
    }
}