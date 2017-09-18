import { Component } from "@angular/core";
import { Router } from "@angular/router";

import { PlexService } from "../../services";
import { NotificationService } from "../../services";

import { IPlexAuthentication } from "../../interfaces";

@Component({
    templateUrl: "./plex.component.html",
})
export class PlexComponent {

    public login: string;
    public password: string;

    private authenticationResult: IPlexAuthentication;

    constructor(private plexService: PlexService, private router: Router, private notificationService: NotificationService) { }

    public requestAuthToken() {
        this.plexService.logIn(this.login, this.password).subscribe(x => {
            if (x.user == null) {
                this.notificationService.error("Could Not Authenticate", "Username or password was incorrect. Could not authenticate with Plex.");
                return;
            }
            this.authenticationResult = x;

            this.router.navigate(["Wizard/CreateAdmin"]);
        });
    }
}
