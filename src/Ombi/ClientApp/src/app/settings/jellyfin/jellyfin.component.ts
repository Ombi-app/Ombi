import { Component, OnInit } from "@angular/core";

import { IJellyfinServer, IJellyfinSettings } from "../../interfaces";
import { JellyfinService, JobService, NotificationService, SettingsService, TesterService } from "../../services";
import { MatTabChangeEvent } from "@angular/material/tabs";
import {FormControl} from '@angular/forms';

@Component({
    templateUrl: "./jellyfin.component.html",
    styleUrls: ["./jellyfin.component.scss"]
})
export class JellyfinComponent implements OnInit {

    public settings: IJellyfinSettings;
    public hasDiscoveredOrDirty: boolean;
    selected = new FormControl(0);


    constructor(private settingsService: SettingsService,
                private notificationService: NotificationService,
                private testerService: TesterService,
                private jobService: JobService,
                private jellyfinService: JellyfinService) { }

    public ngOnInit() {
        this.settingsService.getJellyfin().subscribe(x => this.settings = x);
    }

    public async discoverServerInfo(server: IJellyfinServer) {
        const result = await this.jellyfinService.getPublicInfo(server).toPromise();
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
            } as IJellyfinServer);
        this.selected.setValue(this.settings.servers.length - 1);
        }
    }

    public toggle() {
     this.hasDiscoveredOrDirty = true;
    }

    public test(server: IJellyfinServer) {
        this.testerService.jellyfinTest(server).subscribe(x => {
            if (x === true) {
                this.notificationService.success(`Successfully connected to the Jellyfin server ${server.name}!`);
            } else {
                this.notificationService.error(`We could not connect to the Jellyfin server  ${server.name}!`);
            }
        });
    }

    public removeServer(server: IJellyfinServer) {
        const index = this.settings.servers.indexOf(server, 0);
        if (index > -1) {
            this.settings.servers.splice(index, 1);
            this.selected.setValue(this.settings.servers.length - 1);
        }
    }

    public save() {
        this.settingsService.saveJellyfin(this.settings).subscribe(x => {
            if (x) {
                this.notificationService.success("Successfully saved Jellyfin settings");
            } else {
                this.notificationService.success("There was an error when saving the Jellyfin settings");
            }
        });
    }

    public runCacher(): void {
        this.jobService.runJellyfinCacher().subscribe(x => {
            if(x) {
                this.notificationService.success("Triggered the Jellyfin Content Cacher");
            }
        });
    }
}
