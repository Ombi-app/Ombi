import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MatButtonModule } from "@angular/material/button";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from "@angular/material/input";
import { MatCardModule } from "@angular/material/card";
import { MatIconModule } from "@angular/material/icon";
import { TranslateModule } from "@ngx-translate/core";

import { IJellyfinSettings } from "../../interfaces";
import { JellyfinService } from "../../services";
import { NotificationService } from "../../services";

@Component({
    standalone: true,
    selector: "wizard-jellyfin",
    templateUrl: "./jellyfin.component.html",
    styleUrls: ["../welcome/welcome.component.scss"],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatCardModule,
        MatIconModule,
        TranslateModule
    ]
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
            serverId: undefined,
            jellyfinSelectedLibraries: []
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
