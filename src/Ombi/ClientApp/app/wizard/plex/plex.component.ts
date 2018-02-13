import { Component } from "@angular/core";
import { Router } from "@angular/router";

import { ConfirmationService } from "primeng/primeng";

import { PlexService } from "../../services";
import { IdentityService, NotificationService, SettingsService } from "../../services";
import { AuthService } from "./../../auth/auth.service";

@Component({
    templateUrl: "./plex.component.html",
})
export class PlexComponent {

    public login: string;
    public password: string;

    constructor(private plexService: PlexService, private router: Router,
                private notificationService: NotificationService,
                private confirmationService: ConfirmationService,
                private identityService: IdentityService,
                private settings: SettingsService,
                private auth: AuthService) { }

    public requestAuthToken() {
        this.plexService.logIn(this.login, this.password).subscribe(x => {
            if (x.user == null) {
                this.notificationService.error("Username or password was incorrect. Could not authenticate with Plex.");
                return;
            }
            this.confirmationService.confirm({
              message: "Do you want your Plex user to be the main admin account on Ombi?",
              header: "Use Plex Account",
              icon: "fa fa-check",
              accept: () => {
                  this.identityService.createWizardUser({
                    username: "",
                    password: "",
                    usePlexAdminAccount: true,
                  }).subscribe(x => {
                    if (x) {
                        this.auth.login({ username: this.login, password: this.password, rememberMe:false }).subscribe(c => {
                            localStorage.setItem("id_token", c.access_token);
                  
                            // Mark that we have done the settings now
                            this.settings.getOmbi().subscribe(ombi => {
                                ombi.wizard = true;

                                this.settings.saveOmbi(ombi).subscribe(x => {
                                    this.settings.getUserManagementSettings().subscribe(usr => {

                                        usr.importPlexAdmin = true;
                                        this.settings.saveUserManagementSettings(usr).subscribe(saved => {
                                            this.router.navigate(["login"]);
                                        });
                                    });

                                });
                            });
                    });
                    } else {
                      this.notificationService.error("Could not get the Plex Admin Information");
                      return;
                    }
                  });
              },
              reject: () => {
                this.router.navigate(["Wizard/CreateAdmin"]);
              },
            });  
        });
    }
}
