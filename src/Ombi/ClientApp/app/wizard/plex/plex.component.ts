import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";

import { PlexService, PlexTvService, SettingsService } from "../../services";
import { IdentityService, NotificationService } from "../../services";

@Component({
    templateUrl: "./plex.component.html",
})
export class PlexComponent implements OnInit {

    public login: string;
    public password: string;

    private clientId: string;

    constructor(private plexService: PlexService, private router: Router,
        private notificationService: NotificationService,
        private identityService: IdentityService, private plexTv: PlexTvService,
        private settingsService: SettingsService) { }

    public ngOnInit(): void {
        this.settingsService.getClientId().subscribe(x => this.clientId = x);
    }

    public requestAuthToken() {
        this.plexService.logIn(this.login, this.password).subscribe(x => {
            if (x.user == null) {
                this.notificationService.error("Username or password was incorrect. Could not authenticate with Plex.");
                return;
            }

            this.identityService.createWizardUser({
                username: "",
                password: "",
                usePlexAdminAccount: true,
            }).subscribe(y => {
                if (y) {
                    this.router.navigate(["login"]);
                } else {
                    this.notificationService.error("Could not get the Plex Admin Information");
                    return;
                }
            });
        }
        );
    }

    public oauth() {
        this.plexTv.GetPin(this.clientId, "Ombi").subscribe((pin: any) => {
            this.plexService.oAuth({ wizard: true, pin }).subscribe(x => {
                if (x.url) {
                    window.location.href = x.url;
                }
            });
        });
    }
}
