import { Component, OnInit } from "@angular/core";

import { IEmbyServer, IEmbySettings } from "../../interfaces";
import { JobService, NotificationService, SettingsService, TesterService } from "../../services";

@Component({
    templateUrl: "./emby.component.html",
})
export class EmbyComponent implements OnInit {

    public settings: IEmbySettings;

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private jobService: JobService) { }

    public ngOnInit() {
        this.settingsService.getEmby().subscribe(x => this.settings = x);
    }

    public addTab() {
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
            subDir: "",
        } as IEmbyServer);
    }

    public test(server: IEmbyServer) {
        this.testerService.embyTest(server).subscribe(x => {
            if (x === true) {
                this.notificationService.success("Connected", `Successfully connected to the Emby server ${server.name}!`);
            } else {
                this.notificationService.error("Connected", `We could not connect to the Emby server  ${server.name}!`);
            }
        });
    }

    public removeServer(server: IEmbyServer) {
        const index = this.settings.servers.indexOf(server, 0);
        if (index > -1) {
            this.settings.servers.splice(index, 1);
        }
    }

    public save() {
        this.settingsService.saveEmby(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Settings Saved", "Successfully saved Emby settings");
            } else {
                this.notificationService.success("Settings Saved", "There was an error when saving the Emby settings");
            }
        });
    }

    public runCacher(): void {
        this.jobService.runEmbyCacher().subscribe(x => {
            if(x) {
                this.notificationService.success("Running","Triggered the Emby Content Cacher");
            }
        });
    }
}
