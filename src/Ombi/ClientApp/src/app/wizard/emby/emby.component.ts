import { Component, OnInit } from "@angular/core";

import { EmbyService } from "../../services";
import { NotificationService } from "../../services";

import { IEmbySettings } from "../../interfaces";

@Component({
    selector: "wizard-emby",
    templateUrl: "./emby.component.html",
})
export class EmbyComponent implements OnInit {

    public embySettings: IEmbySettings;

    constructor(private embyService: EmbyService,
                private notificationService: NotificationService) {
    }

    public ngOnInit() {
        this.embySettings = {
            servers: [],
            id: 0,
            enable: true,
        };
        this.embySettings.servers.push({
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
        this.embyService.logIn(this.embySettings).subscribe(x => {
            if (x == null || !x.servers[0].apiKey) {
                this.notificationService.error("Username or password was incorrect. Could not authenticate with Emby.");
                return;
            }
            
            this.notificationService.success("Done! Please press next");
        });
    }
}
