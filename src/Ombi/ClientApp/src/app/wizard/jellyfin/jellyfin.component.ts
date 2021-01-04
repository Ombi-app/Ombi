import { Component, OnInit } from "@angular/core";

import { JellyfinService } from "../../services";
import { NotificationService } from "../../services";

import { IJellyfinSettings } from "../../interfaces";

@Component({
    selector: "wizard-jellyfin",
    templateUrl: "./jellyfin.component.html",
})
export class JellyfinComponent implements OnInit {

    public jellyfinSettings: IJellyfinSettings;

    constructor(private jellyfinService: JellyfinService,
                private notificationService: NotificationService) {
    }

    public ngOnInit() {
        this.jellyfinSettings = {
            servers: [],
            id: 0,
            enable: true,
        };
        this.jellyfinSettings.servers.push({
            ip: "",
            administratorId: "",
            id: 0,
            apiKey: "",
            enableEpisodeSearching: false,
            name: "Default",
            port: 8096,
            ssl: false,
            subDir: "",
            serverHostname: "",
            serverId: undefined
        });
    }

    public save() {
        this.jellyfinService.logIn(this.jellyfinSettings).subscribe(x => {
            if (x == null || !x.servers[0].apiKey) {
                this.notificationService.error("Username or password was incorrect. Could not authenticate with Jellyfin.");
                return;
            }
            
            this.notificationService.success("Done! Please press next");
        });
    }
}
