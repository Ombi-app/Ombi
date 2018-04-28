import { Component } from "@angular/core";
import { Router } from "@angular/router";

import { PlexService } from "../../services";
import { IdentityService, NotificationService } from "../../services";

@Component({
    templateUrl: "./plex.component.html",
})
export class PlexComponent {

    public login: string;
    public password: string;

    constructor(private plexService: PlexService, private router: Router,
                private notificationService: NotificationService,
                private identityService: IdentityService) { }

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
              },
        );
    }

    public oauth() {
        this.plexService.oAuth(true).subscribe(x => {
            if(x.url) {
                window.location.href = x.url;
            }
        });
    }
}
