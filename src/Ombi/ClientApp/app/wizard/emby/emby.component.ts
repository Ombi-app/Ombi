import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";

import { EmbyService } from "../../services";
import { NotificationService } from "../../services";

import { IEmbySettings } from "../../interfaces";

@Component({
    templateUrl: "./emby.component.html",
})
export class EmbyComponent implements OnInit {

    private embySettings: IEmbySettings;

    constructor(private embyService: EmbyService,
                private router: Router,
                private notificationService: NotificationService) {
    }

    public ngOnInit() {
        this.embySettings = {
            servers: [],
            id:0,
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

        });
    }

    public save() {
        this.embyService.logIn(this.embySettings).subscribe(x => {
            if (x == null || !x.servers[0].apiKey) {
                this.notificationService.error("Username or password was incorrect. Could not authenticate with Emby.");
                return;
            }
            this.router.navigate(["Wizard/CreateAdmin"]);
        });
    }
}
