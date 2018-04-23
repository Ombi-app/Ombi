import { Component } from "@angular/core";
import { Router } from "@angular/router";

import { AuthService } from "../../auth/auth.service";
import { IdentityService } from "../../services";
import { NotificationService } from "../../services";
import { SettingsService } from "../../services";

@Component({
    templateUrl: "./createadmin.component.html",
})
export class CreateAdminComponent {

    public username: string;
    public password: string;

    constructor(private identityService: IdentityService, private notificationService: NotificationService,
                private router: Router, private auth: AuthService, private settings: SettingsService) { }

    public createUser() {
        this.identityService.createWizardUser({username: this.username, password: this.password, usePlexAdminAccount: false}).subscribe(x => {
            if (x) {
                // Log me in.
                this.auth.login({ username: this.username, password: this.password, rememberMe: false, usePlexOAuth:false }).subscribe(c => {

                    localStorage.setItem("id_token", c.access_token);

                    // Mark that we have done the settings now
                    this.settings.getOmbi().subscribe(ombi => {
                        ombi.wizard = true;

                        this.settings.saveOmbi(ombi).subscribe(x => {

                            this.router.navigate(["search"]);
                        });

                    });

                });
            } else {
                this.notificationService.error("There was an error... You might want to put this on Github...");
            }
        });
    }
}
