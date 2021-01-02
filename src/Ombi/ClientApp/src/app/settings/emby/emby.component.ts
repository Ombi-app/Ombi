import { Component, OnInit } from "@angular/core";

import { IEmbyServer, IEmbySettings } from "../../interfaces";
import { EmbyService, JobService, NotificationService, SettingsService, TesterService } from "../../services";
import { MatTabChangeEvent } from "@angular/material/tabs";
import {FormControl} from '@angular/forms';

@Component({
    templateUrl: "./emby.component.html",
    styleUrls: ["./emby.component.scss"]
})
export class EmbyComponent implements OnInit {

    public settings: IEmbySettings;
    public hasDiscoveredOrDirty: boolean;
    selected = new FormControl(0);

    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private jobService: JobService,
                private embyService: EmbyService) { }

    public ngOnInit() {
        this.settingsService.getEmby().subscribe(x => this.settings = x);
    }

    public async discoverServerInfo(server: IEmbyServer) {
        const result = await this.embyService.getPublicInfo(server).toPromise();
        server.name = result.serverName;
        server.serverId = result.id;
        this.hasDiscoveredOrDirty = true;
    }

    public addTab(event: MatTabChangeEvent) {
        const tabName = event.tab.textLabel;
        if (tabName == "Add Server"){
            if (this.settings.servers == null) {
                this.settings.servers = [];
            }
            this.settings.servers.push({
                name: "New " + this.settings.servers.length + "*",
                id: Math.floor(Math.random() * (99999 - 0 + 1) + 1),
                apiKey: "",
                administratorId: "",
                enableEpisodeSearching: false,
                ip: "",
                port: 0,
                ssl: false,
                subDir: "",
            } as IEmbyServer);
        this.selected.setValue(this.settings.servers.length - 1);
        }
    }

    public toggle() {
     this.hasDiscoveredOrDirty = true;
    }

    public test(server: IEmbyServer) {
        this.testerService.embyTest(server).subscribe(x => {
            if (x === true) {
                this.notificationService.success(`Successfully connected to the Emby server ${server.name}!`);
            } else {
                this.notificationService.error(`We could not connect to the Emby server  ${server.name}!`);
            }
        });
    }

    public removeServer(server: IEmbyServer) {
        const index = this.settings.servers.indexOf(server, 0);
        if (index > -1) {
            this.settings.servers.splice(index, 1);
            this.selected.setValue(this.settings.servers.length - 1);
        }
    }

    public save() {
        this.settingsService.saveEmby(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Emby settings");
            } else {
                this.notificationService.success("There was an error when saving the Emby settings");
            }
        });
    }

    public runCacher(): void {
        this.jobService.runEmbyCacher().subscribe(x => {
            if(x) {
                this.notificationService.success("Triggered the Emby Content Cacher");
            }
        });
    }
}
