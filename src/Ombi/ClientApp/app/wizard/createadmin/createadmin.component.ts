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
                this.router.navigate(["login"]);
            } else {
                this.notificationService.error("There was an error... You might want to put this on Github...");
            }
        });
    }
}
